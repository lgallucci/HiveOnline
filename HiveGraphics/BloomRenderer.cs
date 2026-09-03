using System;
using System.Collections.Generic;
using HiveLib;

namespace HiveGraphics
{
    public sealed class BloomRenderer : IDisposable
    {
        private readonly List<BloomSprite> _sprites = new List<BloomSprite>();
        private GraphicsDevice _device;
        private RenderTarget2D _sourceRenderTarget;
        private SpriteBatch _spriteBatch;

        public BloomFilter Filter { get; } = new BloomFilter();

        public void Load(GraphicsDevice device, ContentManager content, int width, int height)
        {
            _device = device;
            _spriteBatch = new SpriteBatch(device);
            Filter.Load(device, content, width, height, SurfaceFormat.Color);
            Filter.BloomPreset = BloomFilter.BloomPresets.Small;
            Resize(width, height);
        }

        public void Resize(int width, int height)
        {
            if (_device == null)
            {
                return;
            }

            _sourceRenderTarget?.Dispose();
            _sourceRenderTarget = new RenderTarget2D(_device, width, height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public void Begin()
        {
            _sprites.Clear();
        }

        public Texture2D Render()
        {
            _device.SetRenderTarget(_sourceRenderTarget);
            _device.Clear(Color.Transparent);
            _spriteBatch.Begin();
            foreach (var sprite in _sprites)
            {
                if (sprite.DestinationRectangle.HasValue)
                {
                    _spriteBatch.Draw(sprite.Texture, sprite.DestinationRectangle.Value, sprite.Color);
                }
                else
                {
                    _spriteBatch.Draw(sprite.Texture, sprite.Position, sprite.SourceRectangle,
                        sprite.Color, sprite.Rotation, sprite.Origin, sprite.Scale,
                        sprite.Effects, sprite.LayerDepth);
                }
            }
            _spriteBatch.End();

            return Filter.Draw(_sourceRenderTarget, _sourceRenderTarget.Width, _sourceRenderTarget.Height);
        }

        public void Draw(Texture2D texture, Vector2 position, Color color,
            float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            Draw(texture, position, null, color, rotation, origin, scale, effects, layerDepth);
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle,
            Color color, float rotation = 0f, Vector2 origin = default, Vector2? scale = null,
            SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
        {
            _sprites.Add(new BloomSprite(texture, position, sourceRectangle, color, rotation,
                origin, scale ?? Vector2.One, effects, layerDepth, null));
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            _sprites.Add(new BloomSprite(texture, Vector2.Zero, null, color, 0f,
                Vector2.Zero, Vector2.One, SpriteEffects.None, 0f, destinationRectangle));
        }

        public void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Color color, float width)
        {
            var delta = end - start;
            var length = delta.Length();
            var bloomColor = new Color(
                (byte)Math.Min(255, color.R * 1.4f),
                (byte)Math.Min(255, color.G * 1.4f),
                (byte)Math.Min(255, color.B * 1.4f),
                color.A);
            Draw(texture, start, bloomColor, (float)Math.Atan2(delta.Y, delta.X),
                new Vector2(0f, texture.Height / 2f),
                new Vector2(length / texture.Width, width / texture.Height));
        }

        public void Dispose()
        {
            Filter.Dispose();
            _sourceRenderTarget?.Dispose();
            _spriteBatch?.Dispose();
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
    }
}
