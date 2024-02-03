using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
            if (CanHopOntoPiece(board, position) || HexHasNeighborNotMe(board, position))
                return true;

            return false;
        }

        private bool CanHopOntoPiece(PlayingBoard board, Hex position)
        {
            return board.ContainsTile(position);

            //      >-<
            //    >-<A>-<
            //   <C>---<D>
            //    >-<B>-<
            //      >-<
            //If
            //height(A) < height(C) and
            //height(A) < height(D) and
            //height(B) < height(C) and
            //height(B) < height(D)
            //then can't move !
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
            //Todo: implement beetle specific freedom to move rule
            return base.HasFreedomToMove(board);
        }

        public override bool CanMove(PlayingBoard board)
        {
            if (CoveredPiece != null)
                return true;
            else
                return base.CanMove(board);
        }

        public override bool HexHasNeighborNotMe(PlayingBoard board, Hex location)
        {
            for (int i = 0; i < 6; i++)
            {
                if (board.ContainsTile(location.Neighbor(i)))
                {
                    if (CoveredPiece != null && CoveredPiece.Location == location.Neighbor(i))
                        return true;
                    if (this != location.Neighbor(i)  || (CoveredPiece != null))
                        return true;
                }
            }
            return false;
        }
    }
}
