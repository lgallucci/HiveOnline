using HiveClient;
using HiveGraphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HiveOnline.GameEngines
{
    class ConnectToServerEngine : GameEngine
    {
        private readonly HiveGameClient _client;
        private MouseState _previousMouse = Mouse.GetState();
        private int _width = 800;
        private int _height = 600;

        public ConnectToServerEngine(HiveGameClient client)
        {
            _client = client;
        }

        private Rectangle BackBounds => new Rectangle(_width / 2 - 100, _height / 2 + 130, 200, 50);

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _width = _graphicsEngine.ScreenSize.X;
            _height = _graphicsEngine.ScreenSize.Y;
            var mouse = Mouse.GetState();

            var problem = _client.ConnectionProblem;
            _graphicsEngine.DrawStringTop($"Connecting to {_client.Address}:{_client.Port}...", Color.DeepPink, _height / 2 - 80);
            if (problem != null)
            {
                _graphicsEngine.DrawStringTop("Connection failed", Color.Red, _height / 2 - 20);
                _graphicsEngine.DrawStringTop(problem, Color.White, _height / 2 + 20);
                _graphicsEngine.DrawStringTop("Still retrying - or go back to the main menu.", Color.White, _height / 2 + 60);
            }

            _graphicsEngine.DrawButton(BackBounds, "Back", BackBounds.Contains(mouse.X, mouse.Y));
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            _width = screenWidth;
            _height = screenHeight;
        }

        public override void UpdateInactive(GameTime gameTime, ref GameState _gameState)
        {
            if (_client.IsConnected)
                _gameState = GameState.Lobby;
        }

        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            var mouse = Mouse.GetState();
            var clicked = _previousMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
            _previousMouse = mouse;

            if (clicked && BackBounds.Contains(mouse.X, mouse.Y))
            {
                _gameState = GameState.OpeningScreen;
                return;
            }

            if (_client.IsConnected)
                _gameState = GameState.Lobby;
        }
    }
}
