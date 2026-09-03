using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using HiveLib.SearchAlgorithms;
using System.Collections.Generic;
using System;

namespace HiveOnline.GameAssets
{
    public class PlayingBoard : IBoard
    {
        public ChatBox ChatWindow { get; set; }

        public ITile SelectedTile { get; set; }
        public Dictionary<int, ITile> Tiles { get; set; } = new Dictionary<int, ITile>();
        public Dictionary<int, Hex> AvailableTiles { get; set; } = new Dictionary<int, Hex>();

        public string UserName { get; set; } = "TestUser";
        public Pile UserPile { get; set; }
        public string OpponentName { get; set; } = "TestOpponent";
        public Pile OpponentPile { get; set; }
        public string CurrentTurn { get; set; } = "Your Turn";

        public Dictionary<int, Hex> TestSpots { get; set; } = new Dictionary<int, Hex>();
        public int Version { get; private set; }

        public PlayingBoard()
        {
            ChatWindow = new ChatBox();
            UserPile = new Pile(BugTeam.Light);
            OpponentPile = new Pile(BugTeam.Dark);
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
                Version++;
            }
            else
            {
                Tiles.Add(tile.GetHashCode(), tile);
                Version++;
            }
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
            TestSpots.Clear();
            AvailableTiles.Clear();
        }

        public void RemoveTile(ITile tile)
        {
            var replacementTile = tile.RunRemoveRules();

            if (replacementTile != null)
            {
                Tiles[tile.GetHashCode()] = replacementTile;
                Version++;
            }
            else
            {
                Tiles.Remove(tile.GetHashCode());
                Version++;
            }
        }
    }
}
