using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HiveServer
{
    // Lobby: players list themselves, challenge each other, and an accepted challenge starts a game.
    public partial class GameServer
    {
        // challenger id -> challenged player id (one outgoing challenge per player)
        private readonly Dictionary<Guid, Guid> _challenges = new();

        private ConnectedHiveClient FindLobbyClient(Guid id) =>
            _clients.FirstOrDefault(c => c.InLobby && c.Identifier == id);

        private async Task EnterLobby(ConnectedHiveClient client)
        {
            var outbox = new List<(ConnectedHiveClient To, string Message)>();
            string error = null;

            lock (_syncRoot)
            {
                if (client.CurrentGame != null)
                    error = "ERROR ALREADY_IN_GAME";
                else if (string.IsNullOrWhiteSpace(client.Name))
                    error = "ERROR NAME_REQUIRED";
                else if (_waitingQueue.Contains(client))
                    error = "ERROR ALREADY_WAITING";
                else
                {
                    client.InLobby = true;
                    BuildLobbyBroadcast(outbox);
                }
            }

            if (error != null)
            {
                await client.SendMessage(error);
                return;
            }

            Trace($"{client.Name} ({client.Identifier}) entered the lobby");
            await Flush(outbox);
        }

        private async Task LeaveLobby(ConnectedHiveClient client)
        {
            var outbox = new List<(ConnectedHiveClient To, string Message)>();
            lock (_syncRoot)
            {
                RemoveFromLobbyLocked(client, outbox);
            }

            await Flush(outbox);
        }

        private async Task Challenge(ConnectedHiveClient client, string targetText)
        {
            var outbox = new List<(ConnectedHiveClient To, string Message)>();
            string error = null;

            lock (_syncRoot)
            {
                ConnectedHiveClient target = null;
                if (!client.InLobby)
                    error = "ERROR NOT_IN_LOBBY";
                else if (!Guid.TryParse(targetText, out var targetId) || (target = FindLobbyClient(targetId)) == null ||
                         ReferenceEquals(target, client))
                    error = "ERROR PLAYER_UNAVAILABLE";
                else if (!_challenges.TryGetValue(client.Identifier, out var existing) || existing != target.Identifier)
                {
                    if (_challenges.TryGetValue(client.Identifier, out var previousId) && FindLobbyClient(previousId) is { } previous)
                        outbox.Add((previous, $"CHALLENGE_CANCELED|{client.Identifier}"));

                    _challenges[client.Identifier] = target.Identifier;
                    outbox.Add((target, $"CHALLENGED|{client.Identifier},{client.Name}|{target.Expansions}"));
                    Trace($"{client.Name} challenged {target.Name}");
                }
            }

            if (error != null)
            {
                await client.SendMessage(error);
                return;
            }

            await Flush(outbox);
        }

        private async Task DeclineChallenge(ConnectedHiveClient client, string challengerText)
        {
            var outbox = new List<(ConnectedHiveClient To, string Message)>();

            lock (_syncRoot)
            {
                if (Guid.TryParse(challengerText, out var challengerId) &&
                    _challenges.TryGetValue(challengerId, out var targetId) && targetId == client.Identifier)
                {
                    _challenges.Remove(challengerId);
                    if (FindLobbyClient(challengerId) is { } challenger)
                        outbox.Add((challenger, $"CHALLENGE_DECLINED|{client.Identifier}"));
                    Trace($"{client.Name} declined a challenge");
                }
            }

            await Flush(outbox);
        }

        private async Task AcceptChallenge(ConnectedHiveClient client, string challengerText)
        {
            var outbox = new List<(ConnectedHiveClient To, string Message)>();
            ConnectedHiveClient challenger = null;
            HiveGame game = null;
            string error = null;

            lock (_syncRoot)
            {
                if (!client.InLobby)
                    error = "ERROR NOT_IN_LOBBY";
                else if (!Guid.TryParse(challengerText, out var challengerId) ||
                         (challenger = FindLobbyClient(challengerId)) == null ||
                         !_challenges.TryGetValue(challengerId, out var targetId) || targetId != client.Identifier)
                    error = "ERROR NO_CHALLENGE";
                else
                {
                    _challenges.Remove(challengerId);
                    RemoveFromLobbyLocked(challenger, outbox, false);
                    RemoveFromLobbyLocked(client, outbox, false);

                    // The challenger plays first (Light).
                    game = new HiveGame(challenger, client, client.Expansions);
                    challenger.CurrentGame = game;
                    client.CurrentGame = game;
                    _games.Add(game);
                    BuildLobbyBroadcast(outbox);
                }
            }

            if (error != null)
            {
                await client.SendMessage(error);
                return;
            }

            await Flush(outbox);
            await AnnounceMatch(game, challenger, client);
        }

        // Removes the client from the lobby and cancels every challenge it is part of.
        private void RemoveFromLobbyLocked(ConnectedHiveClient client, List<(ConnectedHiveClient To, string Message)> outbox, bool broadcast = true)
        {
            if (!client.InLobby)
                return;

            client.InLobby = false;

            if (_challenges.Remove(client.Identifier, out var targetId) && FindLobbyClient(targetId) is { } target)
                outbox.Add((target, $"CHALLENGE_CANCELED|{client.Identifier}"));

            foreach (var challengerId in _challenges.Where(c => c.Value == client.Identifier).Select(c => c.Key).ToList())
            {
                _challenges.Remove(challengerId);
                if (FindLobbyClient(challengerId) is { } challenger)
                    outbox.Add((challenger, $"CHALLENGE_CANCELED|{client.Identifier}"));
            }

            if (broadcast)
                BuildLobbyBroadcast(outbox);
        }

        // Name shown in the lobby, followed by the expansion letters when the player picked any: "Bob (LM)".
        private static string LobbyLabel(ConnectedHiveClient client) =>
            client.Expansions.Length > 0 ? $"{client.Name} ({client.Expansions})" : client.Name ?? string.Empty;

        // Each lobby member receives the list of the other members: LOBBY_LIST|id,name|id,name
        private void BuildLobbyBroadcast(List<(ConnectedHiveClient To, string Message)> outbox)
        {
            var members = _clients.Where(c => c.InLobby).ToList();
            foreach (var recipient in members)
            {
                var others = members.Where(m => !ReferenceEquals(m, recipient)).Select(m => $"{m.Identifier},{LobbyLabel(m)}");
                outbox.Add((recipient, string.Join("|", new[] { "LOBBY_LIST" }.Concat(others))));
            }
        }

        private static async Task Flush(List<(ConnectedHiveClient To, string Message)> outbox)
        {
            foreach (var (to, message) in outbox)
            {
                try
                {
                    await to.SendMessage(message);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
