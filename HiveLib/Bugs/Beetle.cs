using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HiveLib.Bugs
{
    public class Beetle : Tile
    {
        public Beetle(BugTeam bugTeam)
        {
            Type = BugType.Beetle;
            Team = bugTeam;
        }

        public ITile CoveredPiece { get; set; }

        public override bool CanMoveTo(PlayingBoard board, Hex position)
        {
            // A beetle can climb or drop at most one level per move.
            if (Math.Abs(LandingLevel(board, position) - StackLevel()) > 1)
                return false;

            if (CanHopOntoPiece(board, position) || HexHasNeighborNotMe(GetBoardWithoutMe(board), position))
                return true;

            return false;
        }

        // 1 when on the ground, 2 when on top of one piece, and so on.
        private int StackLevel()
        {
            var level = 1;
            for (var covered = CoveredPiece; covered != null; covered = (covered as Beetle)?.CoveredPiece)
                level++;
            return level;
        }

        private static int LandingLevel(PlayingBoard board, Hex position)
        {
            var level = 1;
            if (board.Tiles.TryGetValue(position.GetHashCode(), out var top))
            {
                level++;
                for (var covered = (top as Beetle)?.CoveredPiece; covered != null; covered = (covered as Beetle)?.CoveredPiece)
                    level++;
            }
            return level;
        }

        public override Dictionary<int, ITile> GetBoardWithoutMe(PlayingBoard board)
        {
            var boardWithoutPiece = board.Tiles.ToDictionary(d => d.Key, d => d.Value);

            boardWithoutPiece.Remove(this.GetHashCode());
            if (this.CoveredPiece != null)
                boardWithoutPiece.Add(this.CoveredPiece.GetHashCode(), this.CoveredPiece);

            return boardWithoutPiece;
        }

        private bool CanHopOntoPiece(PlayingBoard board, Hex position)
        {            
            return board.ContainsTile(position);
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                if (CanMoveTo(board, Location.Neighbor(i)))
                    availableLocations.Add(Location.Neighbor(i));
            }
            return availableLocations;
        }

        public override void RunAddRules(ITile tile)
        {
            CoveredPiece = tile;
        }

        public override ITile RunRemoveRules()
        {
            var piece = CoveredPiece;
            CoveredPiece = null;
            return piece;
        }

        public override bool HasFreedomToMove(PlayingBoard board)
        {
            for (int i = 0; i < 6; i++)
            {
                if (CanMoveTo(board, this.Location.Neighbor(i)))
                    return true;
            }
            return false;
        }

        public override bool CanMove(PlayingBoard board)
        {
            if (CoveredPiece != null)
                return true;
            else
                return base.CanMove(board);
        }

        public override bool HexHasNeighborNotMe(Dictionary<int, ITile> board, Hex location)
        {
            for (int i = 0; i < 6; i++)
            {
                if (board.ContainsKey(location.Neighbor(i).GetHashCode()))
                {
                    if (CoveredPiece != null && CoveredPiece.Location == location.Neighbor(i))
                        return true;
                    if (this != location.Neighbor(i) || (CoveredPiece != null))
                        return true;
                }
            }
            return false;
        }
    }
}
