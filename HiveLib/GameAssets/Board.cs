using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using HiveLib.SearchAlgorithms;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace HiveOnline.GameAssets
{
    public class Board
    {
        public List<ITile> Tiles { get; set; }
        public Layout Layout { get; set; }

        public ChatBox ChatWindow { get; set; }
        public Dictionary<int, ITile> HexCoordinates { get; set; }

        public string UserName { get; set; } = "TestUser";
        public Pile UserPile { get; set; }

        public string OpponentName { get; set; } = "TestOpponent";
        public Pile OpponentPile { get; set; }

        public Board(int width, int height)
        {
            ChatWindow = new ChatBox();
            Tiles = new List<ITile>();
            HexCoordinates = new Dictionary<int, ITile>();
            UserPile = new Pile(BugTeam.Light);
            OpponentPile = new Pile(BugTeam.Dark);

            SetScreenSize(width, height);
        }

        public void SetScreenSize(int width, int height)
        {
            ChatWindow.ChangeScreenSize(new HiveContracts.Point(width, height));
            var size = Layout.size == default ? new HiveContracts.Point(45, 45) : Layout.size;
            var origin = Layout.origin == default ? new HiveContracts.Point(width / 2, height / 2) : Layout.origin;

            UserPile.ChangeScreenSize(width, height, false);
            OpponentPile.ChangeScreenSize(width, height, true);

            Layout = new Layout(Layout.flat, size, origin);
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
