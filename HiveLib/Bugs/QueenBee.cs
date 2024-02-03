using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class QueenBee : Tile
    {
        public QueenBee(BugTeam bugTeam)
        {
            Type = BugType.QueenBee;
            Team = bugTeam;
        }

        public override bool CanMoveTo(PlayingBoard board, Hex position)
        {
            if (HexHasNeighborNotMe(board, position))
                return true;

            return false;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                var neighbor = Location.Neighbor(i);
                if (!board.ContainsTile(neighbor) && CanMoveTo(board, neighbor))
                    availableLocations.Add(neighbor);
            }
            return availableLocations;
        }
    }
}
