using HiveContracts;
using HiveLib;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HiveGraphics.GameAssetsDraw;
public class TileGraphics : DrawableObject
{
    public void Draw(BugType bugType, BugTeam team, HexPoint location, HexPoint tileSize)
    {
        var texture = GetTexture(bugType, team);

        //var bloom = GraphicsEngine.BloomFilter.Draw(texture, texture.Width, texture.Height);
        //GraphicsEngine.SetRenderTarget(null);

        GraphicsEngine.SpriteBatch.Draw(
            texture,
            location.ToVector2(),
            new Rectangle(0, 0, texture.Width, texture.Height),
            Color.White,
            0,
            new Vector2((float)texture.Width / 2, (float)texture.Width / 2),
            new Vector2((float)tileSize.X / texture.Width, (float)tileSize.Y / texture.Height), //Scale
            SpriteEffects.None, 0f);


        //GraphicsEngine.SpriteBatch.Draw(bloom, new Rectangle(0, 0, texture.Width, texture.Height), Color.Red);
    }

    public void Draw(BugType bugType, BugTeam team, Layout layout, Hex location, HexPoint tileSize)
    {
        var vector2 = layout.HexToPixel(location);
        Draw(bugType, team, vector2, tileSize);

        GraphicsEngine.SpriteBatch.DrawString(Art.PileFont, $"{location.q}, {location.r}, {location.s}",
            new Vector2((float)vector2.X - 30, (float)vector2.Y - 7), Color.Red);
    }

    private Texture2D GetTexture(BugType bugType, BugTeam team)
    {
        switch (bugType)
        {
            case BugType.Beetle:
                return team == BugTeam.Light ? Art.LightBeetle : Art.DarkBeetle;
            case BugType.Grasshopper:
                return team == BugTeam.Light ? Art.LightGrassHopper : Art.DarkGrassHopper;
            case BugType.LadyBug:
                return team == BugTeam.Light ? Art.LightLadyBug : Art.DarkLadyBug;
            case BugType.Mosquito:
                return team == BugTeam.Light ? Art.LightMosquito : Art.DarkMosquito;
            case BugType.PillBug:
                return team == BugTeam.Light ? Art.LightPillBug : Art.DarkPillBug;
            case BugType.QueenBee:
                return team == BugTeam.Light ? Art.LightQueenBee : Art.DarkQueenBee;
            case BugType.SoldierAnt:
                return team == BugTeam.Light ? Art.LightSoldierAnt : Art.DarkSoldierAnt;
            case BugType.Spider:
                return team == BugTeam.Light ? Art.LightSpider : Art.DarkSpider;
            default:
                return Art.Pixel;
        }
    }
}
