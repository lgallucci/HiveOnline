using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using HiveLib.SearchAlgorithms;
using HiveGraphics.GameAssetsDraw;
using System.Collections.Generic;
using System.Linq;
using System;

namespace HiveOnline.GameAssets
{
    public class PlayingBoard : IBoard
    {
        public Layout Layout { get; set; }

        public ChatBox ChatWindow { get; set; }

        public ITile SelectedTile { get; set; }
        public Dictionary<int, ITile> Tiles { get; set; } = new Dictionary<int, ITile>();
        public Dictionary<int, Hex> AvailableTiles { get; set; } = new Dictionary<int, Hex>();

        public string UserName { get; set; } = "TestUser";
        public Pile UserPile { get; set; }

        public string OpponentName { get; set; } = "TestOpponent";
        public Pile OpponentPile { get; set; }

        public BoardGraphics Graphics { get; set; } = new BoardGraphics();
        public Dictionary<int, Hex> TestSpots { get; set; } = new Dictionary<int, Hex>();

        public PlayingBoard(int width, int height)
        {
            ChatWindow = new ChatBox();
            UserPile = new Pile(BugTeam.Light);
            OpponentPile = new Pile(BugTeam.Dark);

            SetScreenSize(width, height);
        }

        public void Draw()
        {
            Graphics.Draw(UserName, OpponentName);

            //Draw Grid
            foreach (var tile in Tiles)
            {
                tile.Value.Draw(this);
            }

            foreach (var tile in AvailableTiles)
            {
                Graphics.DrawHexagon(Layout, tile.Value, 170, 189, 100);
            }

            if (SelectedTile != null && !SelectedTile.Equals(default(Hex)))
                Graphics.DrawHexagon(Layout, SelectedTile.Location, 4, 217, 255); 

            DrawPiles();

            ChatWindow.Draw();
        }

        private void DrawPiles()
        {
            UserPile.Draw(this);
            OpponentPile.Draw(this);
        }

        public void SetScreenSize(int width, int height)
        {
            Graphics.Width = width;
            Graphics.Height = height;

            ChatWindow.ChangeScreenSize(new HexPoint(width, height));
            var size = Layout.size == default ? new HexPoint(45, 45) : Layout.size;
            var origin = Layout.origin == default ? new HexPoint(width / 2, height / 2) : Layout.origin;

            UserPile.ChangeScreenSize(width, height, false);
            OpponentPile.ChangeScreenSize(width, height, true);

            Layout = new Layout(Layout.flat, size, origin);
        }

        public void Move(Tile piece, Tile position)
        {
            if (!piece.CanMove(this) || !piece.CanMoveTo(this, position.Location))
                throw new PlayException("Illegal Move!");

            piece.Location = position.Location;
        }

        public void AddTile(ITile tile)
        {
            if (ContainsTile(tile)) 
            {
                tile.RunAddRules(Tiles[tile.GetHashCode()]);
                Tiles[tile.GetHashCode()] = tile;
            }
            else
                Tiles.Add(tile.GetHashCode(), tile);
        }

        public bool ContainsTile(ITile tile)
        {
            return Tiles.ContainsKey(tile.GetHashCode());
        }

        public bool ContainsTile(Hex tile)
        {
            return Tiles.ContainsKey(tile.GetHashCode());
        }

        public void AddAvailableHexes(List<Hex> hexes)
        {
            foreach (var hex in hexes)
            { 
                AvailableTiles.Add(hex.GetHashCode(), hex);
            }
        }

        public void ClearAvailableTiles()
        {
            AvailableTiles.Clear();
        }

        public void RemoveTile(ITile tile)
        {
            var replacementTile = tile.RunRemoveRules();

            if (replacementTile != null)
                Tiles[tile.GetHashCode()] = replacementTile;
            else
                Tiles.Remove(tile.GetHashCode());
        }
    }
}
