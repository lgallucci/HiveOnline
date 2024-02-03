


using HiveContracts;
using System;

namespace HiveGraphics.GameAssetsDraw;
public class PileGraphics : DrawableObject
{
    private int _tileSize;

    public PileGraphics(int tileSize)
    {
        _tileSize = tileSize;
    }

    public void ChangeScreenSize(int width, int height, int stackCount, int placementWidth, bool isOpponent)
    {
        var pileWidth = stackCount * placementWidth;
        if (!isOpponent)
            Location = new Rectangle(5, height - 75, pileWidth, 75);
        else
            Location = new Rectangle(width - pileWidth - 5, 5, pileWidth, 75);
    }

    public void DrawBox()
    {
        //DRAW BOX
        GraphicsEngine.SpriteBatch.Draw(Art.Pixel, Location, new Color(48, 90, 70));
    }

    public void DrawBug(Action<HexPoint, HexPoint> tileDraw, string numberString, int bufferSize)
    {
        int halfTileSize = _tileSize / 2;

        tileDraw(new HexPoint(Location.Left + bufferSize + halfTileSize, Location.Top + halfTileSize), new HexPoint(_tileSize, _tileSize));

        DrawString(numberString, bufferSize);
    }

    private void DrawString(string numberString, int bufferSize)
    {
        var fontSize = Art.PileFont.MeasureString(numberString);
        GraphicsEngine.SpriteBatch.DrawString(Art.PileFont, numberString, new Vector2(Location.Left + bufferSize + _tileSize - 5, Location.Bottom - fontSize.Y), Color.MintCream);
    }
}
