using HiveClient;
using HiveGraphics;
using Microsoft.Xna.Framework;

namespace HiveOnline.GameEngines
{
    class ConnectToServerEngine : GameEngine
    {
        private readonly HiveGameClient _client;

        public ConnectToServerEngine(HiveGameClient client)
        {
            _client = client;
        }

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _graphicsEngine.DrawStringCentered("Connecting to server...", Color.DeepPink);
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
        }

        public override void UpdateInactive(GameTime gameTime, ref GameState _gameState) => Update(gameTime, ref _gameState);

        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            if (_client.IsConnected)
                _gameState = GameState.Lobby;
        }
    }
}
