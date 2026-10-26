using HiveClient;
using HiveContracts;
using HiveGraphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HiveOnline.GameEngines
{
    // Shows players waiting for a game; click one to challenge, or accept an incoming challenge.
    class LobbyEngine : GameEngine
    {
        private readonly HiveGameClient _client;
        private readonly ConcurrentQueue<string> _messages = new();
        private readonly List<(Guid Id, string Name)> _players = new();
        private readonly List<(Guid Id, string Name)> _incoming = new();
        private Guid? _challengedId;
        private string _status = "Waiting for other players...";
        private int _assignedTeam = -1;
        private string _opponentName = "Opponent";
        private bool _left;
        private bool _connectionLost;
        private readonly string _playerName;
        private readonly string _expansionCodes;
        private BugType[] _gameExpansions = Array.Empty<BugType>();
        private int _width = 800;
        private MouseState _previousMouse = Mouse.GetState();

        private const int RowHeight = 44;
        private const int FirstRowY = 120;

        public BugTeam AssignedTeam => _assignedTeam == 1 ? BugTeam.Dark : BugTeam.Light;
        public string OpponentName => _opponentName;

        // The expansions of the player who received the challenge.
        public IReadOnlyCollection<BugType> GameExpansions => _gameExpansions;

        public LobbyEngine(HiveGameClient client, string playerName, IEnumerable<BugType> expansions = null)
        {
            _client = client;
            _playerName = playerName;
            _expansionCodes = ExpansionCodes.ToCodes(expansions);
            _client.MessageReceived += OnMessage;
            _ = EnterAsync(playerName);
        }

        private async System.Threading.Tasks.Task EnterAsync(string playerName)
        {
            await _client.SendName(playerName);
            await _client.SendExpansions(_expansionCodes);
            await _client.EnterLobby();
        }

        private void OnMessage(string message) => _messages.Enqueue(message);

        private static (Guid Id, string Name)? ParsePlayer(string text)
        {
            var parts = text.Split(',', 2);
            return parts.Length == 2 && Guid.TryParse(parts[0], out var id) ? (id, parts[1]) : null;
        }

        private void ProcessMessages(ref GameState gameState)
        {
            if (!_client.IsConnected)
            {
                // The server forgets us when the connection drops, so rejoin the lobby after reconnecting.
                _connectionLost = true;
                _players.Clear();
                _incoming.Clear();
                _challengedId = null;
                _status = (_client.ConnectionProblem ?? "Connection to the server was lost.") + " Reconnecting...";
                while (_messages.TryDequeue(out _)) { }
                return;
            }

            if (_connectionLost)
            {
                _connectionLost = false;
                _status = "Reconnected";
                _ = EnterAsync(_playerName);
            }

            while (_messages.TryDequeue(out var message))
            {
                var fields = message.Split('|');
                switch (fields[0])
                {
                    case "LOBBY_LIST":
                        _players.Clear();
                        foreach (var field in fields.Skip(1))
                            if (ParsePlayer(field) is { } player)
                                _players.Add(player);
                        if (_challengedId.HasValue && _players.All(p => p.Id != _challengedId))
                            _challengedId = null;
                        break;
                    case "CHALLENGED":
                        if (fields.Length > 1 && ParsePlayer(fields[1]) is { } challenger)
                        {
                            _incoming.RemoveAll(c => c.Id == challenger.Id);
                            _incoming.Add(challenger);
                        }
                        break;
                    case "CHALLENGE_CANCELED":
                        if (fields.Length > 1 && Guid.TryParse(fields[1], out var canceledId))
                            _incoming.RemoveAll(c => c.Id == canceledId);
                        break;
                    case "CHALLENGE_DECLINED":
                        _challengedId = null;
                        _status = "Your challenge was declined";
                        break;
                    case "EXPANSIONS":
                        _gameExpansions = ExpansionCodes.FromCodes(fields.Length > 1 ? fields[1] : null);
                        break;
                    case "OPPONENT_NAME":
                        if (fields.Length > 1)
                            _opponentName = fields[1];
                        break;
                    case "YOU_ARE_PLAYER_1":
                        _assignedTeam = 0;
                        break;
                    case "YOU_ARE_PLAYER_2":
                        _assignedTeam = 1;
                        break;
                    default:
                        if (message.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                            _status = message;
                        break;
                }
            }

            if (_assignedTeam >= 0)
            {
                _client.MessageReceived -= OnMessage;
                gameState = GameState.Playing;
            }
        }

        private Rectangle BackBounds => new Rectangle(20, 20, 120, 40);

        private Rectangle PlayerRow(int index) => new Rectangle(_width - 420, FirstRowY + index * RowHeight, 400, RowHeight - 6);

        private const int IncomingStartY = 80;

        // Each challenge is a stacked Accept / Decline pair under the Back button.
        private Rectangle AcceptBounds(int index) => new Rectangle(20, IncomingStartY + index * (RowHeight * 2 + 10), 260, RowHeight - 6);

        private Rectangle DeclineBounds(int index) => new Rectangle(20, IncomingStartY + index * (RowHeight * 2 + 10) + RowHeight, 260, RowHeight - 6);

        public override void Draw(GraphicsEngine graphicsEngine)
        {
            _width = graphicsEngine.ScreenSize.X;
            var mouse = Mouse.GetState();

            graphicsEngine.DrawStringTop("Lobby - click a player to challenge them", Color.DeepPink, 60);
            graphicsEngine.DrawButton(BackBounds, "Back", BackBounds.Contains(mouse.X, mouse.Y));

            if (_players.Count == 0)
                graphicsEngine.DrawButton(PlayerRow(0), "No other players waiting", false);

            for (var i = 0; i < _players.Count; i++)
            {
                var row = PlayerRow(i);
                var label = _players[i].Id == _challengedId ? $"{_players[i].Name} (challenge sent)" : _players[i].Name;
                graphicsEngine.DrawButton(row, label, row.Contains(mouse.X, mouse.Y));
            }

            for (var i = 0; i < _incoming.Count; i++)
            {
                var accept = AcceptBounds(i);
                var decline = DeclineBounds(i);
                graphicsEngine.DrawButton(accept, $"Accept {_incoming[i].Name}", accept.Contains(mouse.X, mouse.Y));
                graphicsEngine.DrawButton(decline, "Decline " + _incoming[i].Name, decline.Contains(mouse.X, mouse.Y));
            }

            graphicsEngine.DrawStringTop(_status, Color.White, graphicsEngine.ScreenSize.Y - 50);
        }

        public override void SetScreenSize(int screenWidth, int screenHeight) => _width = screenWidth;

        public override void UpdateInactive(GameTime gameTime, ref GameState gameState) => ProcessMessages(ref gameState);

        public override void Update(GameTime gameTime, ref GameState gameState)
        {
            ProcessMessages(ref gameState);
            if (gameState != GameState.ConnectToServer && gameState != GameState.Lobby)
                return;

            var mouse = Mouse.GetState();
            var clicked = _previousMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
            _previousMouse = mouse;
            if (!clicked || _left)
                return;

            if (BackBounds.Contains(mouse.X, mouse.Y))
            {
                _left = true;
                _client.MessageReceived -= OnMessage;
                _ = _client.LeaveLobby();
                gameState = GameState.OpeningScreen;
                return;
            }

            for (var i = 0; i < _players.Count; i++)
            {
                if (PlayerRow(i).Contains(mouse.X, mouse.Y))
                {
                    _challengedId = _players[i].Id;
                    _status = $"Challenge sent to {_players[i].Name}";
                    _ = _client.Challenge(_players[i].Id);
                    return;
                }
            }

            for (var i = 0; i < _incoming.Count; i++)
            {
                var challenger = _incoming[i];
                if (AcceptBounds(i).Contains(mouse.X, mouse.Y))
                {
                    _ = _client.AcceptChallenge(challenger.Id);
                    _status = $"Starting game with {challenger.Name}...";
                    return;
                }

                if (DeclineBounds(i).Contains(mouse.X, mouse.Y))
                {
                    _incoming.RemoveAt(i);
                    _ = _client.DeclineChallenge(challenger.Id);
                    return;
                }
            }
        }
    }
}
