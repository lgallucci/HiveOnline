using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveLib.Bugs
{
    public class Grasshopper : Tile
    {
        public Grasshopper(BugTeam bugTeam)
        {
            Type = BugType.Grasshopper;
            Team = bugTeam;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var availableLocations = new List<Hex>();
            for (int i = 0; i < 6; i++)
            {
                if (board.ContainsTile(Location.Neighbor(i)))
                    availableLocations.Add(HopUntilBlank(board, Location, i));
            }
            return availableLocations;
        }

        private Hex HopUntilBlank(PlayingBoard board, Hex location, int direction)
        {
            Hex newLocation;
            if (board.ContainsTile(location))
                newLocation = HopUntilBlank(board, location.Neighbor(direction), direction);
            else
                newLocation = location;
            return newLocation;
        }

        public override bool HasFreedomToMove(PlayingBoard board)
        {
            return true;
        }
    }
}
