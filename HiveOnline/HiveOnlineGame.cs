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

            _graphicsEngine = new GraphicsEngine(Content, graphics.GraphicsDevice);
            _gameEngine = new GameEngine();
            _board = new Board();

            this.IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();

            _board.AddHex(new Hex(0, 0, 0));

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
            var mouseState = Mouse.GetState();
            var fractionalHex = _board.Layout.PixelToHex(new HiveContracts.Point(mouseState.X, mouseState.Y));
            var hexHash = fractionalHex.HexRound().GetHashCode();
            if (mouseState.LeftButton == ButtonState.Pressed && _board.HexCoordinates.ContainsKey(hexHash))
            {
                var tile = _board.HexCoordinates[hexHash];
                for (int i = 0; i < 6; i++)
                {
                    var newHex = tile.Location.Neighbor(i);
                    if (!_board.HexCoordinates.ContainsKey(newHex.GetHashCode()))
                    {
                        _board.AddHex(newHex);
                    }
                }
            }

            _gameEngine.Run(ref _board);

            base.Update(gameTime);
        }

        int framesPerSecond = 0;
        int frameCount = 0;
        TimeSpan frameTimer = new TimeSpan(0);
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here
            spriteBatch.Begin();

            _graphicsEngine.Draw(graphics, spriteBatch, framesPerSecond, _board);

            frameTimer += gameTime.ElapsedGameTime;
            frameCount++;
            if (frameTimer > TimeSpan.FromSeconds(1))
            {
                framesPerSecond = frameCount;
                frameCount = 0;
                frameTimer = new TimeSpan(0);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
