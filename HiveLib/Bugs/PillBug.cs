using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    public class PillBug : Tile
    {
        public PillBug(BugTeam bugTeam)
        {
            Type = BugType.PillBug;
            Team = bugTeam;
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            throw new PlayException("Pillbug expansion hasn't been implemented!");
        }

        protected override void Draw()
        {
            throw new PlayException("Pillbug expansion hasn't been implemented!");
        }
    }
}
