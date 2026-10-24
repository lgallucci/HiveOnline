using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class Mosquito : Tile
    {
        public Mosquito(BugTeam bugTeam)
        {
            Type = BugType.Mosquito;
            Team = bugTeam;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            throw new PlayException("Mosquito expansion hasn't been implemented!");
        }
    }
}
