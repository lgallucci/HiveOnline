using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.Bugs
{
    public class PillBug : Tile
    {
        public PillBug(BugTeam bugTeam)
        {
            Type = BugType.PillBug;
            Team = bugTeam;
        }

        public override bool BugCanMoveTo(Board board, Hex position)
        {
            throw new PlayException("Pillbug expansion hasn't been implemented!");
        }

        public override Texture2D GetTexture()
        {
            if (Team == BugTeam.Light)
            {
                return Art.LightPillBug;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkPillBug;
            }
            return Art.BlankBug;
        }
    }
}
