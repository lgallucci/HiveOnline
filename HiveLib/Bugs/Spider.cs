using HiveContracts;
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
            var available = TraverseOutside(board, Location, new List<Hex>(), 3);
            return available;
        }

        private List<Hex> TraverseOutside(PlayingBoard board, Hex nextLocation, List<Hex> traveledLocations, int hopsLeft)
        {
            List<Hex> availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                Debug.WriteLine($"{nextLocation.q},{nextLocation.r},{nextLocation.s} neighbor {i} with {hopsLeft} hops left");
                if (HexHasNeighborNotMe(board, nextLocation.Neighbor(i)))
                {
                    var neighbor = nextLocation.Neighbor(i);

                    if (!traveledLocations.Contains(neighbor) &&
                        !board.ContainsTile(neighbor) &&
                        HasOpenNeighbor(board, nextLocation, i))
                    {
                        _travelCount++;
                        if (hopsLeft > 0)
                        {
                            board.TestSpots.Add(_travelCount, nextLocation);
                            traveledLocations.Add(nextLocation);
                            availableLocations.AddRange(TraverseOutside(board, neighbor, traveledLocations, hopsLeft - 1));
                        }
                        else
                        {
                            if (!availableLocations.Contains(nextLocation))
                                availableLocations.Add(nextLocation);
                        }
                    }
                }
            }
            return availableLocations;
        }

        private bool HasOpenNeighbor(PlayingBoard board, Hex nextLocation, int i)
        {
            var nextNeighbor = i == 5 ? 0 : i + 1;
            var previousNeighbor = i == 0 ? 5 : i - 1;
            return !board.ContainsTile(nextLocation.Neighbor(nextNeighbor)) ||
                   !board.ContainsTile(nextLocation.Neighbor(previousNeighbor));
        }
    }
}
