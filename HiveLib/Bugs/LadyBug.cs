using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    public class LadyBug : Tile
    {
        public LadyBug(BugTeam bugTeam)
        {
            Type = BugType.LadyBug;
            Team = bugTeam;
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            throw new PlayException("LadyBug expansion hasn't been implemented!");
        }

        protected override void Draw()
        {
            throw new PlayException("LadyBug expansion hasn't been implemented!");
        }
    }
}
