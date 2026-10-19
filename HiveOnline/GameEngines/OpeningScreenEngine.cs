using HiveContracts;
using HiveGraphics;
using HiveOnline.GameAssets;
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

        private const int MaxNameLength = 16;
        private string _playerName = string.Empty;
        private bool _nameRequired;
        private bool _editingName;

        public GameMode? SelectedMode { get; private set; }

        public string PlayerName => _playerName.Trim();

        private Rectangle NameField => new Rectangle(_screenSize.X / 2 - ButtonWidth / 2, GetButton(0).Y - ButtonHeight - ButtonSpacing - 20, ButtonWidth, ButtonHeight);

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

            _graphicsEngine.DrawStringTop("HIVE !   Enter your name and choose a game mode", Color.DeepPink);

            var field = NameField;
            if (_nameRequired && PlayerName.Length == 0)
                _graphicsEngine.DrawStringTop("Enter a name to play online", Color.Red, field.Y - 40);
            _graphicsEngine.DrawButton(field, _editingName ? $"{_playerName}_" : (PlayerName.Length == 0 ? "Click to enter name" : _playerName),
                _editingName || field.Contains(mouse.X, mouse.Y));
            for (var i = 0; i < Options.Length; i++)
            {
                var bounds = GetButton(i);
                _graphicsEngine.DrawButton(bounds, Options[i].Label, bounds.Contains(mouse.X, mouse.Y));
            }
        }

        private void ApplyNameInput(ChatInputResult input)
        {
            if (input.Submit || input.Cancel)
                _editingName = false;
            else if (input.Backspace && _playerName.Length > 0)
                _playerName = _playerName.Substring(0, _playerName.Length - 1);
            else if (!string.IsNullOrEmpty(input.Text))
            {
                var clean = input.Text.Replace("|", string.Empty);
                if (_playerName.Length + clean.Length <= MaxNameLength)
                    _playerName += clean;
            }
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            _screenSize = new Point(screenWidth, screenHeight);
        }

        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            KeyboardHelper.Update();
            var mouse = Mouse.GetState();
            var clicked = _previousMouse.LeftButton == ButtonState.Released && mouse.LeftButton == ButtonState.Pressed;
            _previousMouse = mouse;

            if (_editingName)
                ApplyNameInput(KeyboardHelper.ReadChatInput());

            if (!clicked)
                return;

            _editingName = NameField.Contains(mouse.X, mouse.Y);
            if (_editingName)
                return;

            for (var i = 0; i < Options.Length; i++)
            {
                if (!GetButton(i).Contains(mouse.X, mouse.Y))
                    continue;

                if (Options[i].Mode == GameMode.Multiplayer && PlayerName.Length == 0)
                {
                    _nameRequired = true;
                    return;
                }

                SelectedMode = Options[i].Mode;
                _gameState = SelectedMode == GameMode.Multiplayer ? GameState.ConnectToServer : GameState.Playing;
                return;
            }
        }
    }
}
