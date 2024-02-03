using HiveContracts;
using HiveLib.GameAssets;
using HiveGraphics.GameAssetsDraw;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using HiveLib.SearchAlgorithms;
using System.Linq;
using System;

namespace HiveOnline.GameAssets
{
    public abstract class Tile : ITile, IEquatable<Tile>
    {
        public BugType Type { get; set; }
        public Hex Location { get; set; }
        public BugTeam Team { get; set; }

        private TileGraphics Graphics { get; set; } = new TileGraphics();

        public void Draw(PlayingBoard board)
        {
            Graphics.Draw(Type, Team, board.Layout, Location, board.Layout.size * 2);

            var boardGraphics = new BoardGraphics();
            foreach (var testSpot in board.TestSpots)
            {
                boardGraphics.DrawHexagon(board.Layout, testSpot.Value, 255, 247, 0);
                boardGraphics.DrawText(board.Layout, testSpot.Value, testSpot.Key.ToString(), 255, 247, 0);
            }
        }

        public void Draw(HexPoint location, HexPoint size)
        {
            Graphics.Draw(Type, Team, location, size);
        }

        public override int GetHashCode()
        {
            return Location.GetHashCode();
        }

        public virtual bool CanMoveTo(PlayingBoard board, Hex position)
        {
            var available = CalculateAvailable(board);

            return available.Any(a => a.GetHashCode() == position.GetHashCode());
        }

        public virtual void RunAddRules(ITile tile)
        {
        }

        public virtual ITile RunRemoveRules()
        {
            return null;
        }

        public virtual bool HexHasNeighborNotMe(PlayingBoard board, Hex location)
        {
            for (int i = 0; i < 6; i++)
            {
                if (location.Neighbor(i) != this.Location && board.ContainsTile(location.Neighbor(i)))
                    return true;
            }
            return false;
        }

        public virtual bool CanMove(PlayingBoard board)
        {
            if (board.Tiles.Count == 1)
                return true;

            var boardWithoutPiece = board.Tiles.ToDictionary(d => d.Key, d => d.Value);

            boardWithoutPiece.Remove(this.GetHashCode());

            ITile firstNeighbor = null;
            for (int i = 0; i < 6; i++)
            {
                var neighbor = this.Location.Neighbor(i);
                if (boardWithoutPiece.ContainsKey(neighbor.GetHashCode()))
                {
                    firstNeighbor = boardWithoutPiece[neighbor.GetHashCode()];
                }
            }

            if (firstNeighbor == null)
                return false;

            return BreadthFirstSearch.CheckSingleHive(boardWithoutPiece, firstNeighbor) && HasFreedomToMove(board);


            //SECOND IDEA IS SEEING IF THERE IS A NEIGHBOR SURROUNDED BY TWO EMPTY SPACES AND SEEING IF WE CAN RECURSIVELY GET BACK TO THAT NEIGHBOR FROM A DIFFERENT ONE

            //Get non empty neighbors. add to list and create a bad list.

            //Loop through non empty neighbors.

            ////if neighbor is not surrounded by empties, remove from bad list.

            ////If neighbor is piece surrounded by empties, do a search for other neighbors with piece removed

            //////If other neighbor not found, return true.

            //////If other neighbor found remove from list.

            //if problem list is empty then return false;
        }

        public virtual bool HasFreedomToMove(PlayingBoard board)
        {
            int consecutiveOpens = 0;
            int maxConsecutiveOpens = 0;
            for (int i = 0; i < 7; i++)
            {
                int neighborNumber = i > 5 ? 0 : i;

                if (board.ContainsTile(Location.Neighbor(neighborNumber)))
                    consecutiveOpens = 0;
                else
                    consecutiveOpens++;

                if (maxConsecutiveOpens < consecutiveOpens)
                    maxConsecutiveOpens = consecutiveOpens;
            }
            return maxConsecutiveOpens > 1;
        }

        public abstract List<Hex> CalculateAvailable(PlayingBoard board);

        public bool Equals(Tile other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(Tile t1, Tile t2)
        {
            if (t1 is null)
            {
                if (t2 is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return t1.Equals(t2);
        }

        public static bool operator !=(Tile t1, Tile t2) => !(t1 == t2);

        public bool Equals(Hex other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(Tile t1, Hex h2)
        {
            if (t1 is null)
            {
                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return t1.Equals(h2);
        }

        public static bool operator !=(Tile t1, Hex h2) => !(t1 == h2);

        public override bool Equals(object obj)
        {
            return Equals(obj as Tile);
        }
    }
}
