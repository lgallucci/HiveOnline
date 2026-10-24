using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class PillBug : Tile
    {
        public PillBug(BugTeam bugTeam)
        {
            Type = BugType.PillBug;
            Team = bugTeam;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            throw new PlayException("PillBug expansion hasn't been implemented!");
        }
    }
}
