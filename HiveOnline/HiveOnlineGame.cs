using HiveClient;
using HiveContracts;
using HiveGraphics;
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
        private IBoard _board;
        int _screenWidth = 1600;
        int _screenHeight = 900;

        GraphicsDeviceManager _graphics;
        SpriteBatch spriteBatch;

        int framesPerSecond = 0;
        int frameCount = 0;
        TimeSpan frameTimer = new TimeSpan(0);

        public HiveOnlineGame()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            _graphicsEngine = new GraphicsEngine(Content, _graphics.GraphicsDevice);
            _gameEngine = new RunningGameEngine();
            _board = new Board(new HiveContracts.Point(_screenWidth, _screenHeight));
            _hiveClient = new HiveGameClient(_address, _port);

            this.IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            _graphics.SynchronizeWithVerticalRetrace = false;
            SetWindowSize();


            _board.AddTile(new BlankTile { Location = new Hex(0, 0, 0) });

            base.Initialize();
        }

        private void SetWindowSize()
        {
            _graphicsEngine.ScreenSize = new HiveContracts.Point(_screenWidth, _screenHeight);

            _graphics.PreferredBackBufferWidth = _screenWidth;
            _graphics.PreferredBackBufferHeight = _screenHeight;

            _graphics.ApplyChanges();
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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            _gameEngine.Update(ref _board);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(53, 101, 77));
            // TODO: Add your drawing code here
            spriteBatch.Begin();

            _graphicsEngine.Draw(_graphics, spriteBatch, framesPerSecond, _board);

            CalculateFps(gameTime);

            spriteBatch.End();

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
