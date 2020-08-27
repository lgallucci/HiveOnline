using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    class PillBug : Tile
    {
        public PillBug()
        {
            Type = BugType.PillBug;
            throw new PlayException("Pillbug expansion hasn't been implemented!");
        }

        protected override bool BugCanMoveTo(IBoard board, Hex position)
        {
            throw new NotImplementedException();
        }

        protected override void Draw()
        {
            throw new NotImplementedException();
        }
    }
}
