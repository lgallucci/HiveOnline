using HiveContracts;
using HiveLib.Bugs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HiveOnline.GameAssets
{
    public class Board : IBoard
    {
        //Tile _selectedTile;

        public List<ITile> Tiles { get; set; }
        public Layout Layout { get; set; }
        public Dictionary<int, ITile> HexCoordinates { get; set; }

        public Board ()
        {
            Tiles = new List<ITile>();
            Layout = new Layout(Layout.flat, new Point(30, 30), new Point(500, 380));
            HexCoordinates = new Dictionary<int, ITile>();
        }

        public void Move(Tile piece, Tile position)
        {
            if (!piece.CanMoveTo(this, position.Location))
                throw new PlayException("Illegal Move!");

            piece.Location = position.Location;
        }

        public void AddHex(Hex hex)
        {
            var tile = new BlankTile() { Location = hex };
            Tiles.Add(tile);
            HexCoordinates.Add(hex.GetHashCode(), tile);
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
