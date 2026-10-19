using HiveClient;
using HiveContracts;
using HiveGraphics;
using HiveOnline.GameEngines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace HiveOnline
{
    public enum GameState
    {
        OpeningScreen = 0,
        ConnectToServer = 1,
        Playing = 2,
        Lobby = 3
    }

    class HiveOnlineGame : Game
    {
        // SHA-256 of the server's public key, printed by "HiveServer --generate-cert". Leave empty for an
        // unencrypted server (local testing); HIVE_TLS_PIN overrides it.
        private const string DefaultServerKeyPin = ""; //"196FEC9D9FD84FC6B90D514167E9B997CF4EEC4B123CD30055B4CF6E745729DB";

        private GameMode _gameMode;
        private HiveGameClient _hiveClient;
        private readonly string _address;
        private readonly int _port;
        private Task _connectTask;
        private BugTeam _team = BugTeam.Light;
        private string _playerName = string.Empty;
        private string _opponentName;

        private GraphicsEngine _graphicsEngine;
        private GameEngine _gameEngine;
        private GameState _gameState;
        int _screenWidth = 1200;
        int _screenHeight = 800;

        int framesPerSecond = 0;
        int frameCount = 0;
        TimeSpan frameTimer = new TimeSpan(0);

        public HiveOnlineGame(string address = "localhost", int port = 7777)
        {
            _address = address;
            _port = port;
            _graphicsEngine = new GraphicsEngine(this);

            Content.RootDirectory = "Content";

            //this.IsFixedTimeStep = false;
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d); //60);
            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        private void SetWindowSize()
        {
            _gameEngine.SetScreenSize(_screenWidth, _screenHeight);

            _graphicsEngine.SetScreenSize(_screenWidth, _screenHeight);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _screenWidth = Window.ClientBounds.Width;
            _screenHeight = Window.ClientBounds.Height;

            SetWindowSize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _gameEngine = new OpeningScreenEngine();
            SetWindowSize();
            _graphicsEngine.Load(GraphicsDevice, Content);
        }

        // Tells the server we are leaving (so the opponent is notified), then drops the connection.
        // A fresh client is created the next time multiplayer is chosen.
        private void LeaveServerGracefully()
        {
            var client = _hiveClient;
            _hiveClient = null;
            _connectTask = null;
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAny(client.SendMessage("BYE"), Task.Delay(1000));
                }
                catch (Exception)
                {
                }
                client.Dispose();
            });
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (_hiveClient != null)
            {
                try
                {
                    _hiveClient.SendMessage("BYE").Wait(1000);
                }
                catch (Exception)
                {
                }
                _hiveClient.Dispose();
            }
            base.OnExiting(sender, args);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _graphicsEngine.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            if (IsActive || _hiveClient != null)
            {
                GameState _previousState = _gameState;
                if (IsActive)
                    _gameEngine.Update(gameTime, ref _gameState);
                else
                    _gameEngine.UpdateInactive(gameTime, ref _gameState);

                if (_gameEngine is OpeningScreenEngine openingScreen && openingScreen.SelectedMode.HasValue)
                {
                    _gameMode = openingScreen.SelectedMode.Value;
                    _playerName = openingScreen.PlayerName;
                    if (_gameMode == GameMode.LocalAi)
                    {
                        if (_playerName.Length == 0)
                            _playerName = "Player";
                        _opponentName = "AI";
                    }
                    else if (_gameMode == GameMode.LocalTwoPlayer)
                    {
                        if (_playerName.Length == 0)
                            _playerName = "Player1";
                        _opponentName = "Player2";
                    }
                }

                // if (_gameMode != GameMode.Multiplayer && _gameState == GameState.OpeningScreen && _previousState == GameState.OpeningScreen)
                // {
                //     _gameState = GameState.Playing;
                // }

                if (_gameState == GameState.Playing && _previousState != GameState.Playing)
                {
                    var useAi = _gameMode == GameMode.LocalAi;
                    var networkClient = _gameMode == GameMode.Multiplayer ? _hiveClient : null;
                    if (_gameEngine is LobbyEngine lobbyEngine)
                    {
                        _team = lobbyEngine.AssignedTeam;
                        _opponentName = lobbyEngine.OpponentName;
                        Window.Title = $"Hive - {_team}";
                    }
                    _gameEngine = new RunningGameEngine(_screenWidth, _screenHeight, _team, networkClient, useAi, _playerName, _opponentName);
                    SetWindowSize();
                }
                if (_gameEngine is RunningGameEngine finished && finished.RestartRequested)
                {
                    finished.Detach();
                    var rematchClient = _gameMode == GameMode.Multiplayer ? _hiveClient : null;
                    _gameEngine = new RunningGameEngine(_screenWidth, _screenHeight, _team, rematchClient, _gameMode == GameMode.LocalAi, _playerName, _opponentName);
                    SetWindowSize();
                }
                if (_gameState == GameState.OpeningScreen && _previousState != GameState.OpeningScreen)
                {
                    if (_gameEngine is RunningGameEngine && _hiveClient != null)
                        LeaveServerGracefully();
                    Window.Title = "Hive";
                    _gameEngine = new OpeningScreenEngine();
                    SetWindowSize();
                }
                if (_gameState == GameState.ConnectToServer && _previousState != GameState.ConnectToServer)
                {
                    var serverKeyPin = Environment.GetEnvironmentVariable("HIVE_TLS_PIN") ?? DefaultServerKeyPin;
                    _hiveClient ??= new HiveGameClient(_address, _port)
                    {
                        UseTls = serverKeyPin.Length > 0 || Environment.GetEnvironmentVariable("HIVE_TLS") == "1",
                        ServerKeyPin = serverKeyPin
                    };
                    _connectTask ??= Task.Run(() => _hiveClient.ConnectAsync());
                    _gameEngine = new ConnectToServerEngine(_hiveClient);
                }
                if (_gameState == GameState.Lobby && _previousState != GameState.Lobby)
                {
                    _gameEngine = new LobbyEngine(_hiveClient, _playerName);
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(53, 101, 77));

            _graphicsEngine.BeginSprites();

            _graphicsEngine.BloomEnabled = true;
            _gameEngine.Draw(_graphicsEngine);

            //_graphicsEngine.DrawFps(framesPerSecond);

            if (_gameEngine is not OpeningScreenEngine)
                _graphicsEngine.DrawStringTop(_gameMode.ToString(), Color.White);

            _graphicsEngine.EndSprites();

            CalculateFps(gameTime);

            base.Draw(gameTime);
        }

        private void CalculateFps(GameTime gameTime)
        {
            frameTimer += gameTime.ElapsedGameTime;
            frameCount++;
            if (frameTimer > TimeSpan.FromSeconds(1))
            {
                framesPerSecond = frameCount;
                frameCount = 0;
                frameTimer = new TimeSpan(0);
            }
        }
    }
}
