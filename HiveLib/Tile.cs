using HiveContracts;
using HiveOnline.GameAssets;
using System;

namespace HiveOnline
{
    public abstract class Tile : ITile
    {
        public BugType Type { get; set; }

        public Hex Location { get; set; }


        public bool CanMoveTo(IBoard board, Hex position)
        {
            return CanMove(board) && BugCanMoveTo(board, position);
        }

        public virtual bool CanMove(IBoard board)
        {
            throw new NotImplementedException();
        }

        protected abstract void Draw();

        protected abstract bool BugCanMoveTo(IBoard board, Hex position);
    }
}
