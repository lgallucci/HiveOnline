using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveOnline
{
    public class SimpleAI
    {
        private PlayingBoard _board;
        private Random _random = new Random();
        private double _thinkTime = 0;
        private const double THINK_DURATION = 1.5; // Seconds to think before making a move

        public SimpleAI(PlayingBoard board)
        {
            _board = board;
        }

        public bool MakeMove(int turnCount, bool queenPlaced)
        {
            _thinkTime += 0.016; // Approximate 60 FPS delta time

            if (_thinkTime < THINK_DURATION)
                return false; // Still thinking, haven't made a move yet

            _thinkTime = 0;

            // If Queen must be placed this turn, only try to place Queen
            if (turnCount >= 3 && !queenPlaced)
            {
                if (_board.OpponentPile.QueenBees?.Count > 0)
                {
                    // Get valid placements using existing Pile logic
                    var availablePlacements = _board.OpponentPile.CalculateAvailable(_board, BugTeam.Dark);
                    if (availablePlacements.Count > 0)
                    {
                        var queen = _board.OpponentPile.GetTile(BugType.QueenBee);
                        var placement = availablePlacements[_random.Next(availablePlacements.Count)];
                        queen.Location = placement;
                        _board.AddTile(queen);
                        return true;
                    }
                }
                return false;
            }

            // Try to move an existing piece on the board (only if Queen is placed)
            if (queenPlaced && _board.Tiles.Count > 0)
            {
                var opponentPieces = _board.Tiles.Where(t => t.Value.Team == BugTeam.Dark).ToList();
                
                // Shuffle to randomize which piece we try to move
                for (int i = opponentPieces.Count - 1; i > 0; i--)
                {
                    int randomIndex = _random.Next(i + 1);
                    var temp = opponentPieces[i];
                    opponentPieces[i] = opponentPieces[randomIndex];
                    opponentPieces[randomIndex] = temp;
                }

                foreach (var piece in opponentPieces)
                {
                    if (piece.Value.CanMove(_board))
                    {
                        var available = piece.Value.CalculateAvailable(_board);
                        if (available.Count > 0)
                        {
                            var moveLocation = available[_random.Next(available.Count)];
                            ExecuteMove(piece.Value, moveLocation);
                            return true;
                        }
                    }
                }
            }

            // If no valid moves on board, place a new piece
            var validPlacements = _board.OpponentPile.CalculateAvailable(_board, BugTeam.Dark);
            if (validPlacements.Count > 0)
            {
                // Get a random bug type from the opponent's pile
                var availableBugTypes = GetAvailableBugTypes(_board.OpponentPile);
                
                if (availableBugTypes.Count > 0)
                {
                    var bugType = availableBugTypes[_random.Next(availableBugTypes.Count)];
                    var newTile = _board.OpponentPile.GetTile(bugType);
                    var placement = validPlacements[_random.Next(validPlacements.Count)];
                    
                    newTile.Location = placement;
                    _board.AddTile(newTile);
                    
                    return true;
                }
            }

            return false; // No valid moves available
        }

        private void ExecuteMove(ITile piece, Hex destination)
        {
            // Remove piece from board if it's already there
            if (_board.ContainsTile(piece))
                _board.RemoveTile(piece);

            piece.Location = destination;
            _board.AddTile(piece);
        }

        private List<BugType> GetAvailableBugTypes(Pile pile)
        {
            var available = new List<BugType>();

            if (pile.Beetles?.Count > 0)
                available.Add(BugType.Beetle);
            if (pile.Grasshoppers?.Count > 0)
                available.Add(BugType.Grasshopper);
            if (pile.LadyBugs?.Count > 0)
                available.Add(BugType.LadyBug);
            if (pile.Mosquitos?.Count > 0)
                available.Add(BugType.Mosquito);
            if (pile.PillBugs?.Count > 0)
                available.Add(BugType.PillBug);
            if (pile.QueenBees?.Count > 0)
                available.Add(BugType.QueenBee);
            if (pile.SoldierAnts?.Count > 0)
                available.Add(BugType.SoldierAnt);
            if (pile.Spiders?.Count > 0)
                available.Add(BugType.Spider);

            return available;
        }

        public void ResetThinkTime()
        {
            _thinkTime = 0;
        }
    }
}
