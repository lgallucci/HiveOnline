using HiveContracts;
using HiveGraphics.GameAssetsDraw;
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
            if (CanHopOntoPiece(board, position) || HexHasNeighborNotMe(GetBoardWithoutMe(board), position))
                return true;

            return false;
        }

        public override Dictionary<int, ITile> GetBoardWithoutMe(PlayingBoard board)
        {
            var boardWithoutPiece = board.Tiles.ToDictionary(d => d.Key, d => d.Value);

            boardWithoutPiece.Remove(this.GetHashCode());
            if (this.CoveredPiece != null)
                boardWithoutPiece.Add(this.CoveredPiece.GetHashCode(), this.CoveredPiece);

            return boardWithoutPiece;
        }

        public override void Draw(PlayingBoard board)
        {
            DrawRecursive(this, board, board.Layout.HexToPixel(Location), board.Layout.size * 2);
        }

        private int DrawRecursive(ITile tile, PlayingBoard board, HexPoint location, HexPoint size)
        {
            int _overlapLevel = (int)(board.Layout.size.X * .1);
            if (IsInspecting)
                _overlapLevel = (int)(board.Layout.size.X * .75);
            ITile coveredPiece = null;

            int level = 0;
            if (tile is Beetle) 
            {
                coveredPiece = ((Beetle)tile)?.CoveredPiece;
            }

            if (coveredPiece is Beetle)
            {
                level = DrawRecursive(coveredPiece, board, location, size);
            }
            else if (coveredPiece != null)
            {
                Graphics.Draw(coveredPiece.Type, coveredPiece.Team, new HexPoint(location.X + (level * _overlapLevel), location.Y + (level * _overlapLevel)), size);
                level += 1;
            }

            Graphics.Draw(tile.Type, tile.Team, new HexPoint(location.X - (level * _overlapLevel), location.Y - (level * _overlapLevel)), size);

            return level + 1;
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
