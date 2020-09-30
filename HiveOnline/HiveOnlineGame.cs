using HiveContracts;
using HiveGraphics;
using HiveLib.Bugs;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace HiveOnline
{
    class HiveOnlineGame : Game
    {
        private GraphicsEngine _graphicsEngine;
        private GameEngine _gameEngine;
        private IBoard _board;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        int framesPerSecond = 0;
        int frameCount = 0;
        TimeSpan frameTimer = new TimeSpan(0);

        public HiveOnlineGame()
        {
            graphics = new GraphicsDeviceManager(this);

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

            var screenHeight = 1920;
            var screenWidth = 1080;

            _graphicsEngine = new GraphicsEngine(Content, graphics.GraphicsDevice);
            _gameEngine = new GameEngine();
            _board = new Board(new HiveContracts.Point(screenHeight, screenWidth));

            this.IsMouseVisible = true;

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = screenHeight;
            graphics.PreferredBackBufferHeight = screenWidth;

            graphics.ApplyChanges();

            _board.AddTile(new BlankTile { Location = new Hex(0, 0, 0) });

            base.Initialize();
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

            _graphicsEngine.Draw(graphics, spriteBatch, framesPerSecond, _board);

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
