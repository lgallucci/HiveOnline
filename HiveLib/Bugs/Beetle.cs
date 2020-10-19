using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace HiveLib.Bugs
{
    public class Beetle : Tile
    {
        public Beetle(BugTeam bugTeam)
        {
            Type = BugType.Beetle;
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
                return Art.LightBeetle;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkBeetle;
            }
            return Art.BlankBug;
        }
    }
}
