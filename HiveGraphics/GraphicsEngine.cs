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

            if (bloomTexture != null)
            {
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                SpriteBatch.Draw(bloomTexture, new Rectangle(0, 0, ScreenSize.X, ScreenSize.Y), Color.White);
                SpriteBatch.End();
            }
        }

        public void DrawString(string text)
        {
            var fontSize = Art.NameFont.MeasureString(text);
            SpriteBatch.DrawString(Art.NameFont, text, new Vector2((ScreenSize.X / 2) - fontSize.X / 2, (ScreenSize.Y / 2) - fontSize.Y / 2), Color.DeepPink);
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

        public void DrawBloom(Texture2D texture, Vector2 position, Color color,
            float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            BloomRenderer.Draw(texture, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawBloom(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
            Color color, float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            BloomRenderer.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawBloom(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            BloomRenderer.Draw(texture, destinationRectangle, color);
        }

        public void DrawBloomLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float width)
        {
            BloomRenderer.DrawLine(texture, start, end, color, width);
        }

        internal void SetRenderTarget(RenderTarget2D value)
        {
            Device.SetRenderTarget(value);
        }
    }
}
