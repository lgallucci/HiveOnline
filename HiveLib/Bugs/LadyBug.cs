using HiveContracts;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.Bugs
{
    class LadyBug : Tile
    {
        public LadyBug()
        {
            Type = BugType.LadyBug;
            throw new PlayException("LadyBug expansion hasn't been implemented!");
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
