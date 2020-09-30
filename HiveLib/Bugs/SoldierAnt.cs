using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    public class SoldierAnt : Tile
    {
        public SoldierAnt(BugTeam bugTeam)
        {
            Type = BugType.SoldierAnt;
            Team = bugTeam;
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            throw new NotImplementedException();
        }

        protected override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
