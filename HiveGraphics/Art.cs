

using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;

namespace HiveGraphics
{
    internal class Art
    {
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

        //Fonts
        internal static DynamicSpriteFont ChatFont { get; private set; }
        internal static DynamicSpriteFont NameFont { get; private set; }
        internal static DynamicSpriteFont PileFont { get; private set; }

        internal static void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            // Load font system
            var fontSystem = FontSystemFactory.Create(graphicsDevice, 1024, 1024);
            fontSystem.AddFont(File.ReadAllBytes("Content/fonts/supersoft.ttf"));

            var robotoFont = FontSystemFactory.Create(graphicsDevice, 1024, 1024);
            robotoFont.AddFont(File.ReadAllBytes("Content/fonts/roboto.ttf"));

            // Create a dynamic font size
            ChatFont = robotoFont.GetFont(16);
            NameFont = fontSystem.GetFont(28);
            PileFont = fontSystem.GetFont(20);

            DarkBeetle = Texture2D.FromFile(graphicsDevice, "Content/img/beetle_dark.png");
            DarkGrassHopper = Texture2D.FromFile(graphicsDevice, "Content/img/grasshopper_dark.png");
            DarkLadyBug = Texture2D.FromFile(graphicsDevice, "Content/img/ladybug_dark.png");
            DarkMosquito = Texture2D.FromFile(graphicsDevice, "Content/img/mosquito_dark.png");
            DarkPillBug = Texture2D.FromFile(graphicsDevice, "Content/img/pillbug_dark.png");
            DarkQueenBee = Texture2D.FromFile(graphicsDevice, "Content/img/bee_dark.png");
            DarkSoldierAnt = Texture2D.FromFile(graphicsDevice, "Content/img/ant_dark.png");
            DarkSpider = Texture2D.FromFile(graphicsDevice, "Content/img/spider_dark.png");

            LightBeetle = Texture2D.FromFile(graphicsDevice, "Content/img/beetle_light.png");
            LightGrassHopper = Texture2D.FromFile(graphicsDevice, "Content/img/grasshopper_light.png");
            LightLadyBug = Texture2D.FromFile(graphicsDevice, "Content/img/ladybug_light.png");
            LightMosquito = Texture2D.FromFile(graphicsDevice, "Content/img/mosquito_light.png");
            LightPillBug = Texture2D.FromFile(graphicsDevice, "Content/img/pillbug_light.png");
            LightQueenBee = Texture2D.FromFile(graphicsDevice, "Content/img/bee_light.png");
            LightSoldierAnt = Texture2D.FromFile(graphicsDevice, "Content/img/ant_light.png");
            LightSpider = Texture2D.FromFile(graphicsDevice, "Content/img/spider_light.png");

            BlankBug = Texture2D.FromFile(graphicsDevice, "Content/img/blank_dark.png");

            Pixel = new Texture2D(graphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}