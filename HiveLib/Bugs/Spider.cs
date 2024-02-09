using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HiveLib.Bugs
{
    public class Spider : Tile
    {
        public Spider(BugTeam bugTeam)
        {
            Type = BugType.Spider;
            Team = bugTeam;
        }

        int _travelCount = 0;
        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            _travelCount = 0; 
            board.TestSpots.Clear();
            var available = TraverseOutside(board, GetBoardWithoutMe(board), Location, new List<Hex> (), 3);
            return available;
        }

        private List<Hex> TraverseOutside(PlayingBoard board, Dictionary<int, ITile> woBoard, Hex currentLocation, List<Hex> traveledLocations, int hopsLeft)
        {
            List<Hex> availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                //Can only move if neighbor is my neighbor's neighbor
                var neighbor = currentLocation.Neighbor(i);

                if (!traveledLocations.Contains(neighbor) && 
                    HexHasNeighborofNeighborNotMe(woBoard, currentLocation, neighbor))
                {
                    //Debug.WriteLine($"{currentLocation.q},{currentLocation.r},{currentLocation.s} neighbor {i} ({neighbor.q},{neighbor.r},{neighbor.s}) with {hopsLeft} hops left has neighbor of neighbor");

                    if (!woBoard.ContainsKey(neighbor.GetHashCode()) &&
                        HasOpenNeighbor(woBoard, currentLocation, i))
                    {
                        _travelCount++;
                        if (hopsLeft > 0)
                        {
                            board.TestSpots.Add(_travelCount, currentLocation);
                            traveledLocations.Add(currentLocation);
                            availableLocations.AddRange(TraverseOutside(board, woBoard, neighbor, traveledLocations, hopsLeft - 1));
                        }
                        else
                        {
                            if (!availableLocations.Contains(currentLocation))
                                availableLocations.Add(currentLocation);
                        }
                    }
                }
            }
            return availableLocations;
        }

    }
}
