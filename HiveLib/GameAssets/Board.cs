using HiveContracts;
using HiveLib.Bugs;
using HiveLib.SearchAlgorithms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HiveOnline.GameAssets
{
    public class Board : IBoard
    {
        //Tile _selectedTile;

        public List<ITile> Tiles { get; set; }
        public Layout Layout { get; set; }
        public Dictionary<int, ITile> HexCoordinates { get; set; }
        public Point WindowSize { get; set; }

        public string UserName { get; set; } = "TestUser";
        public string OpponentName { get; set; } = "TestOpponent";


        public string TypingText { get; set; }
        public Stack<string> ChatMessages { get; set; }


        public Board(Point windowSize)
        {
            WindowSize = windowSize;
            Tiles = new List<ITile>();
            Layout = new Layout(Layout.flat, new Point(45, 45), new Point(windowSize.x / 2, windowSize.y / 2));
            HexCoordinates = new Dictionary<int, ITile>();

            ChatMessages = new Stack<string>(new List<string>
            {
                "TestUser: Hey there big spender!",
                "TestOpponent: You fucking suck.",
                "TestUser: Cheese is smelly and gross",
                "TestUser: Cheese is smelly and gross asdf asdf asdfas dasdfas dfasdf asdf asdf asdf asdf asdf asdf asdf asdf asdf df asdf asdf asdf asdf asdf df asdf asdf asdf asdf asdf ",
                "TestOpponent: You're dumb.",
                "TestUser: You're dumb.",
                "TestOpponent: You're dumb.",
                "TestUser: You're dumb.",
                "TestOpponent: You're dumb.",
                "TestUser: You're dumb.",
                "TestOpponent: You're dumb.",
                "TestUser: You're dumb.",
                "TestOpponent: You're dumb."
            });
        }

        public void Move(Tile piece, Tile position)
        {
            if (!piece.CanMoveTo(this, position.Location))
                throw new PlayException("Illegal Move!");

            piece.Location = position.Location;
        }

        public void AddTile(ITile tile)
        {
            Tiles.Add(tile);
            HexCoordinates.Add(tile.Location.GetHashCode(), tile);
        }

        public void RemoveTile(ITile tile)
        {
            Tiles.Remove(tile);
            HexCoordinates.Remove(tile.Location.GetHashCode());
        }

        public bool CanMove(ITile piece)
        {
            return !WillBreakHive(piece) && piece.CanMove(this);
        }

        private bool WillBreakHive(ITile piece)
        {
            if (HexCoordinates.Count == 1)
                return true;

            var boardWithoutPiece = this.HexCoordinates.ToDictionary(d => d.Key, d => d.Value);

            boardWithoutPiece.Remove(piece.Location.GetHashCode());

            ITile firstNeighbor = null;
            for (int i = 0; i < 6; i++)
            {
                var neighbor = piece.Location.Neighbor(i);
                if (boardWithoutPiece.ContainsKey(neighbor.GetHashCode()))
                {
                    firstNeighbor = boardWithoutPiece[neighbor.GetHashCode()];
                }
            }

            if (firstNeighbor == null)
                return false;

            return !BreadthFirstSearch.CheckSingleHive(boardWithoutPiece, firstNeighbor);


            //SECOND IDEA IS SEEING IF THERE IS A NEIGHBOR SURROUNDED BY TWO EMPTY SPACES AND SEEING IF WE CAN RECURSIVELY GET BACK TO THAT NEIGHBOR FROM A DIFFERENT ONE

            //Get non empty neighbors. add to list and create a bad list.

            //Loop through non empty neighbors.

            ////if neighbor is not surrounded by empties, remove from bad list.

            ////If neighbor is piece surrounded by empties, do a search for other neighbors with piece removed

            //////If other neighbor not found, return true.

            //////If other neighbor found remove from list.

            //if problem list is empty then return false;
        }
    }
}
