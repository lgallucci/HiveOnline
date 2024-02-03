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

        public override bool CanMoveTo(PlayingBoard board, Hex position)
        {
            throw new PlayException("Pillbug expansion hasn't been implemented!");
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            throw new System.NotImplementedException();
        }
    }
}
