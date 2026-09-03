using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using System;

namespace HiveGraphics;

public sealed class BoardViewState
{
    private const int TileSize = 75;
    private const int PlacementWidth = 100;
    private const int ChatWidth = 500;
    private const int ChatHeight = 250;

    public Point ScreenSize { get; private set; }
    public Layout Layout { get; private set; }
    public Rectangle ChatBounds => new Rectangle(ScreenSize.X - ChatWidth, ScreenSize.Y - ChatHeight, ChatWidth - 5, ChatHeight - 5);

    public BoardViewState(int width, int height)
    {
        Resize(width, height);
    }

    public void Resize(int width, int height)
    {
        ScreenSize = new Point(width, height);
        var size = Layout.size == default ? new HexPoint(45, 45) : Layout.size;
        var origin = Layout.origin == default ? new HexPoint(width / 2, height / 2) : Layout.origin;
        Layout = new Layout(Layout.flat, size, origin);
    }

    public void SetLayout(HexPoint size, HexPoint origin)
    {
        Layout = new Layout(Layout.flat, size, origin);
    }

    public bool ContainsChat(int x, int y) => ChatBounds.Contains(x, y);

    public bool ContainsPile(Pile pile, int x, int y, bool opponent)
    {
        return GetPileBounds(pile, opponent).Contains(x, y);
    }

    public ITile GetPileTile(Pile pile, int x, int y, bool opponent)
    {
        var bounds = GetPileBounds(pile, opponent);
        if (!bounds.Contains(x, y))
            return null;

        foreach (var bugType in Enum.GetValues<BugType>())
        {
            if (bugType == BugType.Blank || pile.GetCount(bugType) == 0)
                continue;

            var slot = GetPileSlot(pile, bugType, opponent);
            if (slot.Contains(x, y))
                return pile.Peek(bugType);
        }

        return null;
    }

    public Rectangle GetPileSlot(Pile pile, BugType bugType, bool opponent)
    {
        var offset = 0;
        foreach (var currentType in Enum.GetValues<BugType>())
        {
            if (currentType == BugType.Blank || pile.GetCount(currentType) == 0)
                continue;

            if (currentType == bugType)
            {
                var x = opponent
                    ? ScreenSize.X - GetPileWidth(pile) - 5 + offset
                    : 5 + offset;
                var y = opponent ? 5 : ScreenSize.Y - 75;
                return new Rectangle(x, y, TileSize, TileSize);
            }

            offset += PlacementWidth;
        }

        return Rectangle.Empty;
    }

    public Rectangle GetPileBounds(Pile pile, bool opponent)
    {
        var width = GetPileWidth(pile);
        var x = opponent ? ScreenSize.X - width - 5 : 5;
        var y = opponent ? 5 : ScreenSize.Y - 75;
        return new Rectangle(x, y, width, 75);
    }

    private int GetPileWidth(Pile pile)
    {
        var count = 0;
        foreach (var bugType in Enum.GetValues<BugType>())
        {
            if (bugType != BugType.Blank && pile.GetCount(bugType) > 0)
                count++;
        }
        return count * PlacementWidth;
    }
}
