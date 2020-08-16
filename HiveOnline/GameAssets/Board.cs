using System;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.GameAssets
{
    class Board : IBoard
    {

        public void Move(Tile piece, Tile position)
        {
            if (!piece.CanMoveTo(this, position))
                throw new PlayException("Illegal Move!");

            piece.Location = position.Location;

        }

        public bool CanMove(Tile piece)
        {
            return !WillBreakHive(piece) && piece.CanMove(this);
        }

        private bool WillBreakHive(Tile piece)
        { 
            throw new NotImplementedException(); 
            
            //FIRST IDEA IS TO DO A RECURSIVE SEARCH STOPPING AT EMPTY SPACES AND SEE IF WE FOUND ALL 
        }
    }
}
