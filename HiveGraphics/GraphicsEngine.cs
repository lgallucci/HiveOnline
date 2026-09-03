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
        private static readonly List<BloomSprite> BloomSprites = new List<BloomSprite>();
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
            BloomSprites.Clear();
            SpriteBatch.Begin();
        }

        public void EndSprites()
        {
            SpriteBatch.End();

            Device.SetRenderTarget(_bloomSourceRenderTarget);
            Device.Clear(Color.Transparent);
            BloomSpriteBatch.Begin();
            foreach (var sprite in BloomSprites)
            {
                if (sprite.DestinationRectangle.HasValue)
                {
                    BloomSpriteBatch.Draw(sprite.Texture, sprite.DestinationRectangle.Value, sprite.Color);
                }
                else
                {
                    BloomSpriteBatch.Draw(sprite.Texture, sprite.Position, sprite.SourceRectangle,
                        sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale,
                        sprite.Effects, sprite.LayerDepth);
                }
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

        public static void DrawBloom(Texture2D texture, Vector2 position, Color color,
            float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            var spriteScale = scale ?? Vector2.One;
            BloomSprites.Add(new BloomSprite(texture, position, null, color, rotation,
                origin, spriteScale, effects, layerDepth, null));
        }

        public static void DrawBloom(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
            Color color, float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            var spriteScale = scale ?? Vector2.One;
            BloomSprites.Add(new BloomSprite(texture, position, sourceRectangle, color, rotation,
                origin, spriteScale, effects, layerDepth, null));
        }

        public static void DrawBloom(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            BloomSprites.Add(new BloomSprite(texture, Vector2.Zero, null, color, 0f,
                Vector2.Zero, Vector2.One, SpriteEffects.None, 0f, destinationRectangle));
        }

        public static void DrawBloomLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float width)
        {
            var delta = end - start;
            var length = delta.Length();
            var bloomColor = new Color(
                (byte)Math.Min(255, color.R * 1.4f),
                (byte)Math.Min(255, color.G * 1.4f),
                (byte)Math.Min(255, color.B * 1.4f),
                color.A);
            DrawBloom(texture, start, bloomColor, (float)Math.Atan2(delta.Y, delta.X),
                new Vector2(0f, texture.Height / 2f),
                new Vector2(length / texture.Width, width / texture.Height));
        }

        private readonly struct BloomSprite
        {
            public BloomSprite(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color,
                float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth,
                Rectangle? destinationRectangle)
            {
                Texture = texture;
                Position = position;
                SourceRectangle = sourceRectangle;
                Color = color;
                Rotation = rotation;
                Origin = origin;
                Scale = scale;
                Effects = effects;
                LayerDepth = layerDepth;
                DestinationRectangle = destinationRectangle;
            }

            public Texture2D Texture { get; }
            public Vector2 Position { get; }
            public Rectangle? SourceRectangle { get; }
            public Color Color { get; }
            public float Rotation { get; }
            public Vector2 Origin { get; }
            public Vector2 Scale { get; }
            public SpriteEffects Effects { get; }
            public float LayerDepth { get; }
            public Rectangle? DestinationRectangle { get; }
        }

        internal static void SetRenderTarget(RenderTarget2D value)
        {
            Device.SetRenderTarget(value);
        }
    }
}
