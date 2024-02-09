
using HiveContracts;
using System;
using System.Collections.Generic;

namespace HiveGraphics.GameAssetsDraw;
public class BoardGraphics : DrawableObject
{
    public void Draw(string userName, string opponentName)
    {
        var userNameSize = Art.NameFont.MeasureString(userName);
        var opponentNameSize = Art.NameFont.MeasureString(opponentName);
        GraphicsEngine.SpriteBatch.DrawString(Art.NameFont, userName, 
            new Vector2(5, /*window height - font height*/(float)Height - userNameSize.Y - 75), Color.CornflowerBlue);
        GraphicsEngine.SpriteBatch.DrawString(Art.NameFont, opponentName, 
            new Vector2(/*window width - font width*/(float)Width - opponentNameSize.X - 5, 75), Color.Red);
    }

    public void DrawHexagon(Layout layout, Hex location, int colorR, int colorG, int colorB)
    {
        DrawHexagon(layout.PolygonCorners(location), colorR, colorG, colorB);
    }

    public void DrawHexagon(List<HexPoint> corners, int colorR, int colorG, int colorB)
    { 
        HexPoint firstPoint = new HexPoint(0, 0);
        Nullable<HexPoint> previousPoint = null;
        foreach (var corner in corners)
        {
            if (previousPoint.HasValue)
            {
                GraphicsEngine.SpriteBatch.DrawLine(Art.Pixel, corner.ToVector2(), previousPoint.Value.ToVector2(), new Color(colorR, colorG, colorB), 4f);
            }
            else
            {
                firstPoint = corner;
            }

            previousPoint = corner;
        }

        GraphicsEngine.SpriteBatch.DrawLine(Art.Pixel, firstPoint.ToVector2(), previousPoint.Value.ToVector2(), new Color(colorR, colorG, colorB), 4f);
    }

    public void DrawText(Layout layout, Hex location, string text, int colorR, int colorG, int colorB)
    {
        var vector2 = layout.HexToPixel(location);

        GraphicsEngine.SpriteBatch.DrawString(Art.ChatFont, text,
            new Vector2((float)vector2.X - 30, (float)vector2.Y - 7), new Color(colorR, colorG, colorB));
    }
}
