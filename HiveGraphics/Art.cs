using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace HiveGraphics
{
    public class Art
    {
        public static SpriteFont ArialFont { get; private set; }
        public static SpriteFont NameFont { get; private set; }

        //Dark Tiles
        public static Texture2D DarkBeetle { get; private set; }
        public static Texture2D DarkGrassHopper { get; private set; }
        public static Texture2D DarkLadyBug { get; private set; }
        public static Texture2D DarkMosquito { get; private set; }
        public static Texture2D DarkPillBug { get; private set; }
        public static Texture2D DarkQueenBee { get; private set; }
        public static Texture2D DarkSoldierAnt { get; private set; }
        public static Texture2D DarkSpider { get; private set; }

        //Light Tiles
        public static Texture2D LightBeetle { get; private set; }
        public static Texture2D LightGrassHopper { get; private set; }
        public static Texture2D LightLadyBug { get; private set; }
        public static Texture2D LightMosquito { get; private set; }
        public static Texture2D LightPillBug { get; private set; }
        public static Texture2D LightQueenBee { get; private set; }
        public static Texture2D LightSoldierAnt { get; private set; }
        public static Texture2D LightSpider { get; private set; }

        public static Texture2D Pixel { get; private set; }

        public static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            ArialFont = content.Load<SpriteFont>("ArialFont");
            NameFont = content.Load<SpriteFont>("NameFont");

            DarkBeetle = content.Load<Texture2D>("img/beetle_dark");
            DarkGrassHopper = content.Load<Texture2D>("img/grasshopper_dark");
            DarkLadyBug = content.Load<Texture2D>("img/ladybug_dark");
            DarkMosquito = content.Load<Texture2D>("img/mosquito_dark");
            DarkPillBug = content.Load<Texture2D>("img/pillbug_dark");
            DarkQueenBee = content.Load<Texture2D>("img/bee_dark");
            DarkSoldierAnt = content.Load<Texture2D>("img/ant_dark");
            DarkSpider = content.Load<Texture2D>("img/spider_dark");

            LightBeetle = content.Load<Texture2D>("img/beetle_light");
            LightGrassHopper = content.Load<Texture2D>("img/grasshopper_light");
            LightLadyBug = content.Load<Texture2D>("img/ladybug_light");
            LightMosquito = content.Load<Texture2D>("img/mosquito_light");
            LightPillBug = content.Load<Texture2D>("img/pillbug_light");
            LightQueenBee = content.Load<Texture2D>("img/bee_light");
            LightSoldierAnt = content.Load<Texture2D>("img/ant_light");
            LightSpider = content.Load<Texture2D>("img/spider_light");

            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}