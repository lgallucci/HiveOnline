using HiveContracts;
using HiveOnline;
using HiveOnline.GameAssets;
using System;

namespace HiveLib.Bugs
{
    public class BlankTile : Tile
    {
        public BlankTile()
        {
            Type = BugType.Blank;
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            return false;
        }

        protected override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
