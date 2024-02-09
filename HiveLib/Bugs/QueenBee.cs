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

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var availableLocations = new List<Hex>();
            var woBoard = GetBoardWithoutMe(board);
            for (int i = 0; i < 6; i++)
            {
                var neighbor = Location.Neighbor(i);
                if (!board.ContainsTile(neighbor) && HexHasNeighborNotMe(woBoard, neighbor) && HasOpenNeighbor(woBoard, Location, i))
                    availableLocations.Add(neighbor);
            }
            return availableLocations;
        }
    }
}
