using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.Bugs
{
    public class Mosquito : Tile
    {
        public Mosquito(BugTeam bugTeam)
        {
            Type = BugType.Mosquito;
            Team = bugTeam;
        }

        public override bool BugCanMoveTo(Board board, Hex position)
        {
            throw new PlayException("Mosquito expansion hasn't been implemented!");
        }

        public override Texture2D GetTexture()
        {
            if (Team == BugTeam.Light)
            {
                return Art.LightMosquito;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkMosquito;
            }
            return Art.BlankBug;
        }
    }
}
