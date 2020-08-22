using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    class Beetle : Tile
    {
        public Beetle ()
        {
            Type = BugType.Beetle;
        }

        protected override bool CanBreakFree(IBoard board)
        {
            return true;
        }
    }
}
