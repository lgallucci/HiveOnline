using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Graphics;

namespace HiveLib.Bugs
{
    public class LadyBug : Tile
    {
        public LadyBug(BugTeam bugTeam)
        {
            Type = BugType.LadyBug;
            Team = bugTeam;
        }

        public override bool BugCanMoveTo(Board board, Hex position)
        {
            throw new PlayException("LadyBug expansion hasn't been implemented!");
        }

        public override Texture2D GetTexture()
        {
            if (Team == BugTeam.Light)
            {
                return Art.LightLadyBug;
            }
            else if (Team == BugTeam.Dark)
            {
                return Art.DarkLadyBug;
            }
            return Art.BlankBug;
        }
    }
}
