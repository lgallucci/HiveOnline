using HiveLib;
using FontStashSharp;
using System;
using System.Collections.Generic;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public Point ScreenSize { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public static GraphicsDevice Device { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        public static SpriteBatch BloomSpriteBatch { get; set; }
        public static BloomFilter BloomFilter { get; set; }
        private static readonly List<BloomLine> BloomLines = new List<BloomLine>();
        private RenderTarget2D _sceneRenderTarget;
        private RenderTarget2D _bloomSourceRenderTarget;

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
            BloomSpriteBatch = new SpriteBatch(device);

            BloomFilter.Load(device, content, ScreenSize.X, ScreenSize.Y, SurfaceFormat.Color);
            BloomFilter.BloomPreset = BloomFilter.BloomPresets.Small;

            _sceneRenderTarget = CreateSceneRenderTarget();
            _bloomSourceRenderTarget = CreateBloomSourceRenderTarget();

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

            if (Device != null && _sceneRenderTarget != null)
            {
                _sceneRenderTarget.Dispose();
                _bloomSourceRenderTarget.Dispose();
                _sceneRenderTarget = CreateSceneRenderTarget();
                _bloomSourceRenderTarget = CreateBloomSourceRenderTarget();
            }
        }

        public void BeingSprites()
        {
            Device.SetRenderTarget(_sceneRenderTarget);
            Device.Clear(new Color(53, 101, 77));
            BloomLines.Clear();
            SpriteBatch.Begin();
        }

        public void EndSprites()
        {
            SpriteBatch.End();

            Device.SetRenderTarget(_bloomSourceRenderTarget);
            Device.Clear(Color.Transparent);
            BloomSpriteBatch.Begin();
            foreach (var line in BloomLines)
            {
                BloomSpriteBatch.DrawLine(line.Texture, line.Start, line.End, line.Color, line.Width);
            }
            BloomSpriteBatch.End();

            var bloomTexture = BloomFilter.Draw(_bloomSourceRenderTarget, ScreenSize.X, ScreenSize.Y);

            Device.SetRenderTarget(null);
            Device.Clear(Color.Transparent);

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, Color.White);
            SpriteBatch.End();

            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            SpriteBatch.Draw(bloomTexture, Vector2.Zero, Color.White);
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
            _sceneRenderTarget?.Dispose();
            _bloomSourceRenderTarget?.Dispose();
        }

        private RenderTarget2D CreateSceneRenderTarget()
        {
            return new RenderTarget2D(Device, ScreenSize.X, ScreenSize.Y, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        private RenderTarget2D CreateBloomSourceRenderTarget()
        {
            return new RenderTarget2D(Device, ScreenSize.X, ScreenSize.Y, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public static void QueueBloomLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float width)
        {
            var bloomColor = new Color(
                (byte)Math.Min(255, color.R * 1.4f),
                (byte)Math.Min(255, color.G * 1.4f),
                (byte)Math.Min(255, color.B * 1.4f),
                color.A);
            BloomLines.Add(new BloomLine(texture, start, end, bloomColor, width));
        }

        private readonly struct BloomLine
        {
            public BloomLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float width)
            {
                Texture = texture;
                Start = start;
                End = end;
                Color = color;
                Width = width;
            }

            public Texture2D Texture { get; }
            public Vector2 Start { get; }
            public Vector2 End { get; }
            public Color Color { get; }
            public float Width { get; }
        }

        internal static void SetRenderTarget(RenderTarget2D value)
        {
            Device.SetRenderTarget(value);
        }
    }
}
