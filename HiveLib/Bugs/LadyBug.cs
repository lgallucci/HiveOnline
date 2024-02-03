using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class LadyBug : Tile
    {
        public LadyBug(BugTeam bugTeam)
        {
            Type = BugType.LadyBug;
            Team = bugTeam;
        }

        public override bool CanMoveTo(PlayingBoard board, Hex position)
        {
            throw new PlayException("LadyBug expansion hasn't been implemented!");
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            throw new System.NotImplementedException();
        }
    }
}
