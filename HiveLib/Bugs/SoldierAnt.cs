using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class SoldierAnt : Tile
    {
        public SoldierAnt(BugTeam bugTeam)
        {
            Type = BugType.SoldierAnt;
            Team = bugTeam;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            board.TestSpots.Clear();
            var available = TraverseOutside(GetBoardWithoutMe(board), Location, new List<Hex>());
            return available;
        }

        private List<Hex> TraverseOutside(Dictionary<int, ITile> woBoard, Hex currentLocation, List<Hex> traveledLocations)
        {
            List<Hex> availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                var neighbor = currentLocation.Neighbor(i);

                if (!traveledLocations.Contains(neighbor) &&
                    HexHasNeighborofNeighborNotMe(woBoard, currentLocation, neighbor))
                {
                    traveledLocations.Add(currentLocation);
                    if (!woBoard.ContainsKey(neighbor.GetHashCode()) &&
                        HasOpenNeighbor(woBoard, currentLocation, i))
                    {
                        availableLocations.AddRange(TraverseOutside(woBoard, neighbor, traveledLocations));
                    }
                }

                if (!availableLocations.Contains(currentLocation) && currentLocation != Location)
                {
                    traveledLocations.Add(currentLocation);
                    availableLocations.Add(currentLocation);
                }
            }
            return availableLocations;
        }
    }
}
