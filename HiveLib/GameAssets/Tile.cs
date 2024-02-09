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

        public bool IsInspecting { get; set; }

        public TileGraphics Graphics { get; set; } = new TileGraphics();

        public virtual void Draw(PlayingBoard board)
        {
            var location = board.Layout.HexToPixel(Location);
            Graphics.Draw(Type, Team, location, board.Layout.size * 2);
            //Graphics.DrawCoordinates(board.Layout, Location);

            var boardGraphics = new BoardGraphics();
            foreach (var testSpot in board.TestSpots)
            {
                boardGraphics.DrawHexagon(board.Layout, testSpot.Value, 255, 247, 0);
                //boardGraphics.DrawText(board.Layout, testSpot.Value, $"{testSpot.Key.ToString()}: {testSpot.Value.q}, {testSpot.Value.r}, {testSpot.Value.s}", 255, 247, 0);
            }
        }

        public virtual void Draw(HexPoint location, HexPoint size)
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

        public virtual bool HexHasNeighborNotMe(Dictionary<int, ITile> board, Hex location)
        {
            for (int i = 0; i < 6; i++)
            {
                if (location.Neighbor(i) != this.Location && board.ContainsKey(location.Neighbor(i).GetHashCode()))
                    return true;
            }
            return false;
        }

        public virtual bool HexHasNeighborofNeighborNotMe(Dictionary<int, ITile> board, Hex currentLocation, Hex nextLocation)
        {
            var neighborList = new List<Hex>();
            for(int i = 0; i < 6; i++)
            {
                if (board.ContainsKey(currentLocation.Neighbor(i).GetHashCode()))
                { 
                    neighborList.Add(currentLocation.Neighbor(i));
                    for (int j = 0; j < 6; j++)
                    {
                        if (board.ContainsKey(currentLocation.Neighbor(i).Neighbor(j).GetHashCode()))
                            neighborList.Add(currentLocation.Neighbor(i).Neighbor(j));
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                if (nextLocation.Neighbor(i) != this.Location && board.ContainsKey(nextLocation.Neighbor(i).GetHashCode()) && neighborList.Contains(nextLocation.Neighbor(i)))
                    return true;
            }
            return false;
        }

        public bool HasOpenNeighbor(Dictionary<int, ITile> board, Hex nextLocation, int i)
        {
            var nextNeighbor = i == 5 ? 0 : i + 1;
            var previousNeighbor = i == 0 ? 5 : i - 1;
            return !board.ContainsKey(nextLocation.Neighbor(nextNeighbor).GetHashCode()) ||
                   !board.ContainsKey(nextLocation.Neighbor(previousNeighbor).GetHashCode());
        }
        public virtual bool CanMove(PlayingBoard board)
        {
            if (board.Tiles.Count == 1)
                return true;

            var boardWithoutPiece = GetBoardWithoutMe(board);

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

        public virtual Dictionary<int, ITile> GetBoardWithoutMe(PlayingBoard board)
        {
            var boardWithoutPiece = board.Tiles.ToDictionary(d => d.Key, d => d.Value);

            boardWithoutPiece.Remove(this.GetHashCode());

            return boardWithoutPiece;
        }

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
