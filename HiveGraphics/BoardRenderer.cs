using HiveContracts;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using HiveGraphics.GameAssetsDraw;
using System;
using System.Collections.Generic;

namespace HiveGraphics;

public sealed class BoardRenderer
{
    private readonly BoardGraphics _boardGraphics = new BoardGraphics();
    private readonly TileGraphics _tileGraphics = new TileGraphics();
    private readonly PileGraphics _userPileGraphics = new PileGraphics(75);
    private readonly PileGraphics _opponentPileGraphics = new PileGraphics(75);
    private readonly ChatBoxGraphics _chatGraphics = new ChatBoxGraphics();
    private readonly List<ITile> _renderTiles = new List<ITile>();
    private bool _renderTilesDirty = true;
    private int _renderedBoardVersion = -1;

    public void Resize(BoardViewState view, PlayingBoard board)
    {
        _boardGraphics.Width = view.ScreenSize.X;
        _boardGraphics.Height = view.ScreenSize.Y;
        _chatGraphics.ChangeScreenSize(new HexPoint(view.ScreenSize.X, view.ScreenSize.Y));
        var userBounds = view.GetPileBounds(board.UserPile, false);
        var opponentBounds = view.GetPileBounds(board.OpponentPile, true);
        _userPileGraphics.SetBounds(userBounds);
        _opponentPileGraphics.SetBounds(opponentBounds);
    }

    public void Draw(PlayingBoard board, BoardViewState view, RenderContext context)
    {
        BindContext(context);
        _boardGraphics.Draw(board.UserName, board.OpponentName, board.CurrentTurn);

        if (_renderTilesDirty || _renderedBoardVersion != board.Version)
        {
            _renderTiles.Clear();
            _renderTiles.AddRange(board.Tiles.Values);
            _renderTiles.Sort((left, right) => left.Type.CompareTo(right.Type));
            _renderTilesDirty = false;
            _renderedBoardVersion = board.Version;
        }

        foreach (var tile in _renderTiles)
            DrawTile(tile, view);

        foreach (var testSpot in board.TestSpots.Values)
            _boardGraphics.DrawHexagon(view.Layout, testSpot, 255, 247, 0);

        foreach (var available in board.AvailableTiles.Values)
            _boardGraphics.DrawHexagon(view.Layout, available, 170, 189, 100);

        DrawPile(board.UserPile, view, false);
        DrawPile(board.OpponentPile, view, true);
        _chatGraphics.Draw(board.ChatWindow.TypingText, board.ChatWindow.IsTyping,
            board.ChatWindow.ChatMessages);

        if (board.SelectedTile != null)
        {
            if (board.ContainsTile(board.SelectedTile))
                _boardGraphics.DrawHexagon(view.Layout, board.SelectedTile.Location, 4, 217, 255);
            else
                DrawPileSelection(board.UserPile, board.SelectedTile.Type, view);
        }
    }

    public void MarkTilesDirty() => _renderTilesDirty = true;

    private void BindContext(RenderContext context)
    {
        _boardGraphics.Bind(context);
        _tileGraphics.Bind(context);
        _userPileGraphics.Bind(context);
        _opponentPileGraphics.Bind(context);
        _chatGraphics.Bind(context);
    }

    private void DrawTile(ITile tile, BoardViewState view)
    {
        var location = view.Layout.HexToPixel(tile.Location);
        var size = view.Layout.size * 2;

        if (tile is Beetle beetle)
        {
            DrawBeetleStack(beetle, view, location, size, 0);
            return;
        }

        _tileGraphics.Draw(tile.Type, tile.Team, location, size);
    }

    private int DrawBeetleStack(Beetle beetle, BoardViewState view, HexPoint location, HexPoint size, int level)
    {
        var overlap = (int)(view.Layout.size.X * (beetle.IsInspecting ? .75 : .1));
        if (beetle.CoveredPiece is Beetle coveredBeetle)
            level = DrawBeetleStack(coveredBeetle, view, location, size, level);
        else if (beetle.CoveredPiece != null)
        {
            var coveredLocation = new HexPoint(location.X + level * overlap, location.Y + level * overlap);
            _tileGraphics.Draw(beetle.CoveredPiece.Type, beetle.CoveredPiece.Team, coveredLocation, size);
            level++;
        }

        var tileLocation = new HexPoint(location.X - level * overlap, location.Y - level * overlap);
        _tileGraphics.Draw(beetle.Type, beetle.Team, tileLocation, size);
        return level + 1;
    }

    private void DrawPile(Pile pile, BoardViewState view, bool opponent)
    {
        var graphics = opponent ? _opponentPileGraphics : _userPileGraphics;
        graphics.DrawBox();
        foreach (var bugType in Enum.GetValues<BugType>())
        {
            var count = pile.GetCount(bugType);
            if (bugType == BugType.Blank || count == 0)
                continue;

            var slot = view.GetPileSlot(pile, bugType, opponent);
            var position = new HexPoint(slot.X, slot.Y);
            var tile = pile.Peek(bugType);
            graphics.DrawBug((location, size) => _tileGraphics.Draw(tile.Type, tile.Team, location, size), position, $"x{count}");
        }
    }

    private void DrawPileSelection(Pile pile, BugType bugType, BoardViewState view)
    {
        var slot = view.GetPileSlot(pile, bugType, false);
        var corners = new List<HexPoint>();
        var center = new HexPoint(slot.X + 37.5, slot.Y + 37.5);
        for (var i = 0; i < 6; i++)
        {
            var angle = 2.0 * Math.PI * -i / 6.0;
            corners.Add(new HexPoint(center.X + 37.5 * Math.Cos(angle), center.Y + 37.5 * Math.Sin(angle)));
        }
        _boardGraphics.DrawHexagon(corners, 4, 217, 255);
    }

}
