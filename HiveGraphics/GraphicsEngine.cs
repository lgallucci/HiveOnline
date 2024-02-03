using HiveContracts;
using HiveLib;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public Point ScreenSize { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static GraphicsDevice Device { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static BloomFilter BloomFilter { get; set; }

        public GraphicsEngine(Game game)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(game);

            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;

            BloomFilter = new BloomFilter();
        }
        public void Load(GraphicsDevice device, ContentManager content)
        {
            Device = device;
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(device);

            BloomFilter.Load(device, content, ScreenSize.X, ScreenSize.Y);
            BloomFilter.BloomPreset = BloomFilter.BloomPresets.Small;

            Art.Load(content, device);
        }

        public void DrawFps(int framesPerSecond)
        {
            SpriteBatch.DrawString(Art.ChatFont, $"FPS: {framesPerSecond}", new Vector2(1, 1), Color.Red);
        }

        public void SetScreenSize(int screenWidth, int screenHeight)
        {
            ScreenSize = new Point(screenWidth, screenHeight);

            GraphicsDeviceManager.PreferredBackBufferWidth = screenWidth;
            GraphicsDeviceManager.PreferredBackBufferHeight = screenHeight;

            GraphicsDeviceManager.ApplyChanges();
        }

        public void BeingSprites()
        {
            SpriteBatch.Begin();
        }

        public void EndSprites()
        {
            SpriteBatch.End();
        }

        public void DrawString(string text)
        {
            var fontSize = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, (ScreenSize.Y / 2) - fontSize.Y / 2), Color.DeepPink);
        }

        public void Unload()
        {
            BloomFilter.Dispose();
        }

        internal static void SetRenderTarget(RenderTarget2D value)
        {
            Device.SetRenderTarget(value);
        }
    }
}
