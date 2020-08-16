using HiveOnline.Bugs;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HiveOnline
{
    abstract class Tile
    {
        public BugType Type { get; set; }

        public TileLocation Location { get; set; }


        public bool CanMoveTo(IBoard board, TileLocation position)
        {
            return CanMove(board) && BugCanMoveTo(board, position);
        }

        public virtual bool CanMove(IBoard board)
        {
            throw new NotImplementedException();
        }

        protected abstract void Draw(IDrawableBug drawableBug);

        protected abstract bool BugCanMoveTo(IBoard board, Tile position);


    }
}
