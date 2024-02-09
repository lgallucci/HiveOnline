using HiveClient;
using HiveContracts;
using HiveGraphics;
using HiveLib;
using HiveLib.Bugs;
using HiveOnline.GameAssets;
using HiveOnline.GameEngines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace HiveOnline
{
    public enum GameState
    {
        OpeningScreen = 0,
        ConnectToServer = 1,
        Playing = 2
    }

    class HiveOnlineGame : Game
    {
        private HiveGameClient _hiveClient;
        private string _address = string.Empty;
        private int _port = 60000;

        private GraphicsEngine _graphicsEngine;
        private GameEngine _gameEngine;
        private GameState _gameState;
        int _screenWidth = 1600;
        int _screenHeight = 900;

        int framesPerSecond = 0;
        int frameCount = 0;
        TimeSpan frameTimer = new TimeSpan(0);

        public HiveOnlineGame()
        {
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
            _hiveClient = new HiveGameClient(_address, _port);
            SetWindowSize();
            _graphicsEngine.Load(GraphicsDevice, Content);
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

            if (IsActive)
            {
                GameState _previousState = _gameState;
                _gameEngine.Update(ref _gameState);

                if (_gameState == GameState.Playing && _previousState != GameState.Playing)
                    _gameEngine = new RunningGameEngine(_screenWidth, _screenHeight, BugTeam.Light);
                if (_gameState == GameState.OpeningScreen && _previousState != GameState.OpeningScreen)
                    _gameEngine = new OpeningScreenEngine();
                if (_gameState == GameState.ConnectToServer && _previousState != GameState.ConnectToServer)
                    _gameEngine = new ConnectToServerEngine();
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

            _graphicsEngine.BeingSprites();

            _gameEngine.Draw(_graphicsEngine);

            _graphicsEngine.DrawFps(framesPerSecond);

            CalculateFps(gameTime);

            _graphicsEngine.EndSprites();

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
