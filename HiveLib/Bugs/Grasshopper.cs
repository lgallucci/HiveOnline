using HiveContracts;
using HiveLib;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.Bugs
{
    public class Grasshopper : Tile
    {
        public Grasshopper(BugTeam bugTeam)
        {
            Type = BugType.Grasshopper;
            Team = bugTeam;
        }

        public override bool BugCanMoveTo(Board board, Hex position)
        {
            throw new NotImplementedException();
        }

        public override Texture2D GetTexture()
        {
            if (Team == BugTeam.Light)
            {
                return Art.LightGrassHopper;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkGrassHopper;
            }
            return Art.BlankBug;
        }
    }
}
