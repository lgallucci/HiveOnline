
using HiveContracts;
using System;
using System.Collections.Generic;
using FontStashSharp;

namespace HiveGraphics.GameAssetsDraw;
public class BoardGraphics : DrawableObject
{
    public void Draw(string userName, string opponentName, string currentTurn = "")
    {
        var userNameSize = Art.NameFont.MeasureString(userName);
        var opponentNameSize = Art.NameFont.MeasureString(opponentName);
        var turnSize = Art.NameFont.MeasureString(currentTurn);
        
        Context.SpriteBatch.DrawString(Art.NameFont, userName, 
            new Vector2(5, /*window height - font height*/(float)Height - userNameSize.Y - 75), Color.CornflowerBlue);
        Context.SpriteBatch.DrawString(Art.NameFont, opponentName, 
            new Vector2(/*window width - font width*/(float)Width - opponentNameSize.X - 5, 75), Color.Red);
        
        // Display current turn in the center top
        if (!string.IsNullOrEmpty(currentTurn))
        {
            Context.SpriteBatch.DrawString(Art.NameFont, currentTurn,
                new Vector2(10, 10), Color.Yellow);
        }
    }

    public void DrawHexagon(Layout layout, Hex location, Color color)
    {
        DrawHexagon(layout.PolygonCorners(location), color);
    }

    public void DrawHexagon(List<HexPoint> corners, Color color)
    { 
        HexPoint firstPoint = new HexPoint(0, 0);
        Nullable<HexPoint> previousPoint = null;
        foreach (var corner in corners)
        {
            if (previousPoint.HasValue)
            {
                Context.SpriteBatch.DrawLine(Art.Pixel, corner.ToVector2(), previousPoint.Value.ToVector2(), color, 4f);
                Context.Bloom.DrawLine(Art.Pixel, corner.ToVector2(), previousPoint.Value.ToVector2(), color, 4f);
            }
            else
            {
                firstPoint = corner;
            }

            previousPoint = corner;
        }

        Context.SpriteBatch.DrawLine(Art.Pixel, firstPoint.ToVector2(), previousPoint.Value.ToVector2(), color, 4f);
        Context.Bloom.DrawLine(Art.Pixel, firstPoint.ToVector2(), previousPoint.Value.ToVector2(), color, 4f);
    }

    // Thin translucent outline without glow, used for subtle emphasis.
    public void DrawSubtleOutline(List<HexPoint> corners, Color color, float width)
    {
        for (var i = 0; i < corners.Count; i++)
        {
            var next = corners[(i + 1) % corners.Count];
            Context.SpriteBatch.DrawLine(Art.Pixel, corners[i].ToVector2(), next.ToVector2(), color, width);
        }
    }

    public void DrawText(Layout layout, Hex location, string text, Color color)
    {
        var vector2 = layout.HexToPixel(location);

        Context.SpriteBatch.DrawString(Art.ChatFont, text,
            new Vector2((float)vector2.X - 30, (float)vector2.Y - 7), color);

    }
}
