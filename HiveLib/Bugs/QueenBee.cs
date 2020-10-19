using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.Bugs
{
    public class QueenBee : Tile
    {
        public QueenBee(BugTeam bugTeam)
        {
            Type = BugType.QueenBee;
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
                return Art.LightQueenBee;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkQueenBee;
            }
            return Art.BlankBug;
        }
    }
}
