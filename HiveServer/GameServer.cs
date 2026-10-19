using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;
using HiveContracts;
using HiveLib.Rules;

namespace HiveServer
{
    public class GameServer : IHostedService
    {
        private readonly List<ConnectedHiveClient> _clients = new();
        private readonly List<HiveGame> _games = new();
        private readonly Queue<ConnectedHiveClient> _waitingQueue = new();
        private readonly object _syncRoot = new();

        private AsyncTcpListener _listener;
        private readonly int _port;

        // Verbose logging of connections, matches, moves and disconnects for manual testing.
        public bool TestMode { get; set; }

        private void Trace(string message)
        {
            if (TestMode)
                Console.WriteLine($"[TEST {DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public GameServer() : this(GetConfiguredPort())
        {
        }

        public GameServer(int port)
        {
            _port = port;
        }

        private static int GetConfiguredPort()
        {
            var portText = Environment.GetEnvironmentVariable("HIVE_PORT");
            if (!string.IsNullOrWhiteSpace(portText) && int.TryParse(portText, out var port))
                return port;

            return 7777;
        }

        private async Task ClientConnected(TcpClient tcpClient)
        {
            var client = new ConnectedHiveClient(tcpClient)
            {
                MessageReceived = HandleClientMessage
            };

            client.Disconnected = ClientClosed;

            Console.WriteLine($"Adding Client {client.Identifier} - {tcpClient.Client.RemoteEndPoint}!");
            lock (_syncRoot)
            {
                _clients.Add(client);
            }

            Trace($"Clients connected: {_clients.Count}");
            await client.RunAsync();
        }

        private async Task HandleClientMessage(ConnectedHiveClient client, string message)
        {
            var normalized = message.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            if (normalized.StartsWith("MOVE|", StringComparison.OrdinalIgnoreCase))
            {
                await RouteMove(client, normalized);
                return;
            }

            if (normalized.StartsWith("CHAT|", StringComparison.OrdinalIgnoreCase))
            {
                await RouteChat(client, normalized);
                return;
            }

            switch (normalized.ToUpperInvariant())
            {
                case "HELLO":
                    await client.SendMessage($"SERVER_HELLO {client.Identifier}");
                    break;
                case "PING":
                    await client.SendMessage("PONG");
                    break;
                case "JOIN":
                    await QueueForMatch(client);
                    break;
                case "REMATCH":
                    await RequestRematch(client);
                    break;
                case "BYE":
                    client.Disconnect();
                    break;
                default:
                    await client.SendMessage($"UNKNOWN_COMMAND:{normalized}");
                    break;
            }
        }

        private async Task QueueForMatch(ConnectedHiveClient client)
        {
            ConnectedHiveClient playerOne = null;
            ConnectedHiveClient playerTwo = null;
            HiveGame game = null;
            var alreadyQueued = false;
            var alreadyInGame = false;

            lock (_syncRoot)
            {
                alreadyInGame = client.CurrentGame != null;
                alreadyQueued = !alreadyInGame && _waitingQueue.Contains(client);

                if (!alreadyInGame && !alreadyQueued)
                    _waitingQueue.Enqueue(client);

                if (!alreadyInGame && !alreadyQueued && _waitingQueue.Count >= 2)
                {
                    playerOne = _waitingQueue.Dequeue();
                    playerTwo = _waitingQueue.Dequeue();
                    game = new HiveGame(playerOne, playerTwo);
                    playerOne.CurrentGame = game;
                    playerTwo.CurrentGame = game;
                    _games.Add(game);
                }
            }

            if (alreadyInGame || alreadyQueued)
            {
                await client.SendMessage(alreadyInGame ? "ERROR ALREADY_IN_GAME" : "ERROR ALREADY_WAITING");
                return;
            }

            if (playerOne == null || playerTwo == null)
            {
                await client.SendMessage("WAITING_FOR_OPPONENT");
                return;
            }

            await playerOne.SendMessage($"MATCHED {playerOne.Identifier} {playerTwo.Identifier}");
            await playerTwo.SendMessage($"MATCHED {playerTwo.Identifier} {playerOne.Identifier}");
            await playerOne.SendMessage("YOU_ARE_PLAYER_1");
            await playerTwo.SendMessage("YOU_ARE_PLAYER_2");

            Console.WriteLine($"Matched players in game {game.Id}");
            Trace($"Game {game.Id}: LIGHT={playerOne.Identifier} DARK={playerTwo.Identifier}");
        }

        private async Task RouteMove(ConnectedHiveClient client, string message)
        {
            ConnectedHiveClient opponent = null;
            MoveResult result = null;
            lock (_syncRoot)
            {
                var game = client.CurrentGame;
                if (game != null)
                {
                    opponent = game.GetOpponent(client);
                    if (TryParseMove(message, game.GetTeam(client), out var move))
                        result = game.State.TryApplyMove(move);
                    else
                        result = MoveResult.Rejected("INVALID_MOVE_FORMAT");
                }
            }

            if (opponent == null)
            {
                await client.SendMessage("ERROR NOT_IN_GAME");
                return;
            }

            Trace($"Move from {client.Identifier}: {message} -> {(result.IsValid ? "accepted" : result.ErrorCode)}");
            if (!result.IsValid)
            {
                await client.SendMessage($"MOVE_REJECTED {result.ErrorCode}");
                return;
            }

            await opponent.SendMessage(message);
        }

        private async Task RequestRematch(ConnectedHiveClient client)
        {
            ConnectedHiveClient opponent = null;
            string error = null;
            var start = false;

            lock (_syncRoot)
            {
                var game = client.CurrentGame;
                if (game == null)
                    error = "ERROR NOT_IN_GAME";
                else if (!game.State.IsFinished)
                    error = "ERROR GAME_NOT_FINISHED";
                else
                {
                    opponent = game.GetOpponent(client);
                    client.WantsRematch = true;
                    if (opponent.WantsRematch)
                    {
                        game.Reset();
                        start = true;
                    }
                }
            }

            if (error != null)
            {
                await client.SendMessage(error);
                return;
            }

            Trace($"Rematch requested by {client.Identifier} (start={start})");
            if (start)
            {
                await client.SendMessage("REMATCH_START");
                await opponent.SendMessage("REMATCH_START");
            }
            else
            {
                await opponent.SendMessage("REMATCH_REQUESTED");
            }
        }

        private async Task RouteChat(ConnectedHiveClient client, string message)
        {
            ConnectedHiveClient opponent = null;
            lock (_syncRoot)
            {
                opponent = client.CurrentGame?.GetOpponent(client);
            }

            if (opponent == null)
            {
                await client.SendMessage("ERROR NOT_IN_GAME");
                return;
            }

            Trace($"Chat from {client.Identifier}: {message}");
            await opponent.SendMessage(message.Length > 305 ? message.Substring(0, 305) : message);
        }

        private static bool TryParseMove(string message, BugTeam senderTeam, out HiveMove move)
        {
            move = default;
            var parts = message.Split('|');
            if (parts.Length != 5 || !parts[0].Equals("MOVE", StringComparison.OrdinalIgnoreCase) ||
                !Enum.TryParse(parts[2], true, out BugType bugType) ||
                !TryParseHex(parts[3], out var from) || !TryParseHex(parts[4], out var to))
                return false;

            move = new HiveMove(senderTeam, bugType, from, to);
            return true;
        }

        private static bool TryParseHex(string value, out Hex hex)
        {
            hex = default;
            var coordinates = value.Split(',');
            if (coordinates.Length != 3 || !int.TryParse(coordinates[0], out var q) ||
                !int.TryParse(coordinates[1], out var r) || !int.TryParse(coordinates[2], out var s) ||
                q + r + s != 0)
                return false;

            hex = new Hex(q, r, s);
            return true;
        }

        private void ClientClosed(ConnectedHiveClient tcpClient, bool closedByRemote)
        {
            Console.WriteLine($"Removing Client {tcpClient.Identifier}! {(closedByRemote ? "closedByRemote" : "")}");
            ConnectedHiveClient opponent = null;

            lock (_syncRoot)
            {
                _clients.Remove(tcpClient);
                if (_waitingQueue.Contains(tcpClient))
                {
                    var remaining = new Queue<ConnectedHiveClient>();
                    while (_waitingQueue.Count > 0)
                    {
                        var queuedClient = _waitingQueue.Dequeue();
                        if (!ReferenceEquals(queuedClient, tcpClient))
                            remaining.Enqueue(queuedClient);
                    }
                    _waitingQueue.Clear();
                    while (remaining.Count > 0)
                        _waitingQueue.Enqueue(remaining.Dequeue());
                }

                if (tcpClient.CurrentGame != null)
                {
                    var game = tcpClient.CurrentGame;
                    opponent = game.GetOpponent(tcpClient);
                    _games.Remove(game);
                    tcpClient.CurrentGame = null;
                    opponent.CurrentGame = null;
                }
            }

            Trace($"Clients connected: {_clients.Count}, active games: {_games.Count}");
            if (opponent != null)
                _ = opponent.SendMessage("OPPONENT_DISCONNECTED");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _listener = new AsyncTcpListener
            {
                IPAddress = IPAddress.IPv6Any,
                Port = _port,
                ClientConnectedCallback = ClientConnected
            };

            Console.WriteLine($"Hive server listening on port {_port}");
            using var cancellationRegistration = cancellationToken.Register(() =>
            {
                try
                {
                    _listener?.Stop(true);
                }
                catch (InvalidOperationException)
                {
                }
            });

            await _listener.RunAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _listener?.Stop(true);
            }
            catch (InvalidOperationException)
            {
            }
            return Task.CompletedTask;
        }
    }
}
