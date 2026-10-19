using FontStashSharp;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public Point ScreenSize { get; set; }
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public GraphicsDevice Device { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public BloomRenderer BloomRenderer { get; private set; }

        // The glow is composited over the whole scene, so it must be off while a dialog is shown.
        public bool BloomEnabled { get; set; } = true;
        public RenderContext Context { get; }
        private RenderTarget2D _sceneRenderTarget;

        public GraphicsEngine(Game game)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(game);

            GraphicsDeviceManager.SynchronizeWithVerticalRetrace = false;
            GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;

            BloomRenderer = new BloomRenderer();
            Context = new RenderContext(BloomRenderer);
        }
        public void Load(GraphicsDevice device, ContentManager content)
        {
            Device = device;
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(device);
            Context.SpriteBatch = SpriteBatch;
            BloomRenderer.Load(device, content, ScreenSize.X, ScreenSize.Y);

            _sceneRenderTarget = CreateSceneRenderTarget();

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

            // Re-applying the size from inside a user resize fights the window manager and stops the
            // window from shrinking, so only apply when the back buffer actually differs.
            var backBuffer = Device?.PresentationParameters;
            if (backBuffer == null || backBuffer.BackBufferWidth != screenWidth || backBuffer.BackBufferHeight != screenHeight)
                GraphicsDeviceManager.ApplyChanges();

            if (Device != null && _sceneRenderTarget != null)
            {
                _sceneRenderTarget.Dispose();
                _sceneRenderTarget = CreateSceneRenderTarget();
                BloomRenderer.Resize(screenWidth, screenHeight);
            }
        }

        public void BeginSprites()
        {
            Device.SetRenderTarget(_sceneRenderTarget);
            Device.Clear(new Color(53, 101, 77));
            BloomRenderer.Begin();
            SpriteBatch.Begin();
        }

        public void EndSprites()
        {
            SpriteBatch.End();

            var bloomTexture = BloomRenderer.Render();

            Device.SetRenderTarget(null);
            SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            SpriteBatch.Draw(_sceneRenderTarget, Vector2.Zero, Color.White);
            SpriteBatch.End();

            if (bloomTexture != null && BloomEnabled)
            {
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                SpriteBatch.Draw(bloomTexture, new Rectangle(0, 0, ScreenSize.X, ScreenSize.Y), Color.White);
                SpriteBatch.End();
            }
        }

        public void DrawStringCentered(string text, Color color)
        {
            var fontSize = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, (ScreenSize.Y / 2) - fontSize.Y / 2), color);
        }

        public void DrawButton(Rectangle bounds, string text, bool hovered)
        {
            SpriteBatch.Draw(Art.Pixel, bounds, hovered ? new Color(60, 120, 90) : new Color(26, 50, 38));
            var border = hovered ? Color.DeepPink : Color.MintCream;
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), border);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), border);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), border);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), border);

            var size = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text,
                new Vector2(bounds.X + (bounds.Width - size.X) / 2, bounds.Y + (bounds.Height - size.Y) / 2), Color.White);
        }

        public void DrawPanel(Rectangle bounds)
        {
            SpriteBatch.Draw(Art.Pixel, bounds, new Color(18, 36, 27));
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 3), Color.MintCream);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Bottom - 3, bounds.Width, 3), Color.MintCream);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.X, bounds.Y, 3, bounds.Height), Color.MintCream);
            SpriteBatch.Draw(Art.Pixel, new Rectangle(bounds.Right - 3, bounds.Y, 3, bounds.Height), Color.MintCream);
        }

        public void DrawStringTop(string text, Color color, int y)
        {
            var fontSize = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, y), color);
        }

        public void DrawStringTop(string text, Color color)
        {
            var fontSize = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, 0), color);
        }

        public void Unload()
        {
            BloomRenderer.Dispose();
            _sceneRenderTarget?.Dispose();
        }

        private RenderTarget2D CreateSceneRenderTarget()
        {
            return new RenderTarget2D(Device, ScreenSize.X, ScreenSize.Y, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        }
    }
}
