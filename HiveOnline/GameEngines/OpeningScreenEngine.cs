using HiveContracts;
using HiveGraphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace HiveOnline.GameEngines
{
    class OpeningScreenEngine : GameEngine
    {
        private const int ButtonWidth = 360;
        private const int ButtonHeight = 60;
        private const int ButtonSpacing = 20;

        private static readonly (GameMode Mode, string Label)[] Options =
        {
            (GameMode.LocalAi, "Play vs AI"),
            (GameMode.LocalTwoPlayer, "Local 2 Player"),
            (GameMode.Multiplayer, "Online Multiplayer"),
        };

        private Point _screenSize = new Point(1200, 800);
        private MouseState _previousMouse = Mouse.GetState();

        public GameMode? SelectedMode { get; private set; }

        private Rectangle GetButton(int index)
        {
            var totalHeight = Options.Length * ButtonHeight + (Options.Length - 1) * ButtonSpacing;
            var top = _screenSize.Y / 2 - totalHeight / 2 + 40;
            return new Rectangle(_screenSize.X / 2 - ButtonWidth / 2, top + index * (ButtonHeight + ButtonSpacing), ButtonWidth, ButtonHeight);
        }

        public override void Draw(GraphicsEngine _graphicsEngine)
        {
            _screenSize = _graphicsEngine.ScreenSize;
            var mouse = Mouse.GetState();

            _graphicsEngine.DrawStringTop("HIVE !   Choose a game mode", Color.DeepPink);
            for (var i = 0; i < Options.Length; i++)
            {
                var bounds = GetButton(i);
                _graphicsEngine.DrawButton(bounds, Options[i].Label, bounds.Contains(mouse.X, mouse.Y));
            }
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            _screenSize = new Point(screenWidth, screenHeight);
        }

        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            var mouse = Mouse.GetState();
            var clicked = _previousMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
            _previousMouse = mouse;

            if (!clicked)
                return;

            for (var i = 0; i < Options.Length; i++)
            {
                if (!GetButton(i).Contains(mouse.X, mouse.Y))
                    continue;

                SelectedMode = Options[i].Mode;
                _gameState = SelectedMode == GameMode.Multiplayer ? GameState.ConnectToServer : GameState.Playing;
                return;
            }
        }
    }
}
