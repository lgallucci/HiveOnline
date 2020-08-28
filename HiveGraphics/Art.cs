using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HiveGraphics
{
    public class Art
    {
        public static SpriteFont ArialFont { get; private set; }
        public static Texture2D Pixel { get; private set; }

        public static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            ArialFont = content.Load<SpriteFont>("ArialFont");

            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}