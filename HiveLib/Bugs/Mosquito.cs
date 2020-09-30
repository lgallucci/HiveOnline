using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    public class Mosquito : Tile
    {
        public Mosquito(BugTeam bugTeam)
        {
            Type = BugType.Mosquito;
            Team = bugTeam;
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            throw new PlayException("Mosquito expansion hasn't been implemented!");
        }

        protected override void Draw()
        {
            throw new PlayException("Mosquito expansion hasn't been implemented!");
        }
    }
}
