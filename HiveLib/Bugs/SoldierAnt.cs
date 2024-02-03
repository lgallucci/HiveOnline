using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;

namespace HiveLib.Bugs
{
    public class SoldierAnt : Tile
    {
        public SoldierAnt(BugTeam bugTeam)
        {
            Type = BugType.SoldierAnt;
            Team = bugTeam;
        }

        public override bool CanMoveTo(PlayingBoard board, Hex position)
        {
            throw new NotImplementedException();
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            throw new NotImplementedException();
        }
    }
}
