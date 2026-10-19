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

        public ConnectToServerEngine(HiveGameClient client)
        {
            _client = client;
            _client.MessageReceived += OnMessage;
        }

        public BugTeam AssignedTeam => _assignedTeam == 1 ? BugTeam.Dark : BugTeam.Light;

        private void OnMessage(string message)
        {
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
                _ = _client.JoinGame();
            }

            if (_assignedTeam >= 0)
            {
                _client.MessageReceived -= OnMessage;
                _gameState = GameState.Playing;
            }
        }
    }
}
