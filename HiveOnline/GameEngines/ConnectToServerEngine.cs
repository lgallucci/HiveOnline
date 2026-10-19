using HiveClient;
using HiveContracts;
using HiveGraphics;
using Microsoft.Xna.Framework;
using System;
using System.Threading;

namespace HiveOnline.GameEngines
{
    class ConnectToServerEngine : GameEngine
    {
        private readonly HiveGameClient _client;
        private bool _joinSent;
        private volatile string _status = "Connecting to server...";
        private int _assignedTeam = -1;

        private readonly string _playerName;
        private volatile string _opponentName = "Opponent";

        public string OpponentName => _opponentName;

        public ConnectToServerEngine(HiveGameClient client, string playerName)
        {
            _client = client;
            _playerName = playerName;
            _client.MessageReceived += OnMessage;
        }

        public BugTeam AssignedTeam => _assignedTeam == 1 ? BugTeam.Dark : BugTeam.Light;

        private void OnMessage(string message)
        {
            if (message.StartsWith("OPPONENT_NAME|", StringComparison.Ordinal))
            {
                _opponentName = message.Substring("OPPONENT_NAME|".Length);
                return;
            }

            switch (message)
            {
                case "WAITING_FOR_OPPONENT":
                    _status = "Waiting for an opponent...";
                    break;
                case "YOU_ARE_PLAYER_1":
                    Interlocked.Exchange(ref _assignedTeam, 0);
                    break;
                case "YOU_ARE_PLAYER_2":
                    Interlocked.Exchange(ref _assignedTeam, 1);
                    break;
                default:
                    if (message.StartsWith("ERROR", StringComparison.OrdinalIgnoreCase))
                        _status = message;
                    break;
            }
        }

        private async System.Threading.Tasks.Task JoinAsync()
        {
            await _client.SendName(_playerName);
            await _client.JoinGame();
        }

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _graphicsEngine.DrawStringCentered(_status, Color.DeepPink);
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
        }

        public override void UpdateInactive(GameTime gameTime, ref GameState _gameState) => Update(gameTime, ref _gameState);

        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            if (!_joinSent && _client.IsConnected)
            {
                _joinSent = true;
                _status = "Joining game...";
                _ = JoinAsync();
            }

            if (_assignedTeam >= 0)
            {
                _client.MessageReceived -= OnMessage;
                _gameState = GameState.Playing;
            }
        }
    }
}
