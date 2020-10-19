using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.Bugs
{
    public class Spider : Tile
    {
        public Spider(BugTeam bugTeam)
        {
            Type = BugType.Spider;
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
                return Art.LightSpider;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkSpider;
            }
            return Art.BlankBug;
        }
    }
}
