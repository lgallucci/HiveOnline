

namespace HiveGraphics
{
    internal class Art
    {
        internal static SpriteFont ChatFont { get; private set; }
        internal static SpriteFont NameFont { get; private set; }
        internal static SpriteFont PileFont { get; private set; }

        //Dark Tiles
        internal static Texture2D DarkBeetle { get; private set; }
        internal static Texture2D DarkGrassHopper { get; private set; }
        internal static Texture2D DarkLadyBug { get; private set; }
        internal static Texture2D DarkMosquito { get; private set; }
        internal static Texture2D DarkPillBug { get; private set; }
        internal static Texture2D DarkQueenBee { get; private set; }
        internal static Texture2D DarkSoldierAnt { get; private set; }
        internal static Texture2D DarkSpider { get; private set; }

        //Light Tiles
        internal static Texture2D LightBeetle { get; private set; }
        internal static Texture2D LightGrassHopper { get; private set; }
        internal static Texture2D LightLadyBug { get; private set; }
        internal static Texture2D LightMosquito { get; private set; }
        internal static Texture2D LightPillBug { get; private set; }
        internal static Texture2D LightQueenBee { get; private set; }
        internal static Texture2D LightSoldierAnt { get; private set; }
        internal static Texture2D LightSpider { get; private set; }

        internal static Texture2D BlankBug { get; private set; }

        internal static Texture2D Pixel { get; private set; }

        internal static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            ChatFont = content.Load<SpriteFont>("bin/ChatFont");
            NameFont = content.Load<SpriteFont>("bin/NameFont");
            PileFont = content.Load<SpriteFont>("bin/PileFont");

            DarkBeetle = content.Load<Texture2D>("bin/img/beetle_dark");
            DarkGrassHopper = content.Load<Texture2D>("bin/img/grasshopper_dark");
            DarkLadyBug = content.Load<Texture2D>("bin/img/ladybug_dark");
            DarkMosquito = content.Load<Texture2D>("bin/img/mosquito_dark");
            DarkPillBug = content.Load<Texture2D>("bin/img/pillbug_dark");
            DarkQueenBee = content.Load<Texture2D>("bin/img/bee_dark");
            DarkSoldierAnt = content.Load<Texture2D>("bin/img/ant_dark");
            DarkSpider = content.Load<Texture2D>("bin/img/spider_dark");

            LightBeetle = content.Load<Texture2D>("bin/img/beetle_light");
            LightGrassHopper = content.Load<Texture2D>("bin/img/grasshopper_light");
            LightLadyBug = content.Load<Texture2D>("bin/img/ladybug_light");
            LightMosquito = content.Load<Texture2D>("bin/img/mosquito_light");
            LightPillBug = content.Load<Texture2D>("bin/img/pillbug_light");
            LightQueenBee = content.Load<Texture2D>("bin/img/bee_light");
            LightSoldierAnt = content.Load<Texture2D>("bin/img/ant_light");
            LightSpider = content.Load<Texture2D>("bin/img/spider_light");

            BlankBug = content.Load<Texture2D>("bin/img/blank_dark");

            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}