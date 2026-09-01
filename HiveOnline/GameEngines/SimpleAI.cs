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

            var validPlacements = _board.OpponentPile.CalculateAvailable(_board, BugTeam.Dark);
            var availableBugTypes = GetAvailableBugTypes(_board.OpponentPile);

            // Sometimes prefer a fresh pile placement, not just moving already-placed pieces.
            var shouldPlaceNewTile = validPlacements.Count > 0 && availableBugTypes.Count > 0 &&
                (!queenPlaced || _random.NextDouble() < 0.35 || _board.Tiles.Count == 0);

            if (shouldPlaceNewTile)
            {
                var bugType = SelectBestBugType(availableBugTypes, queenPlaced);
                var newTile = _board.OpponentPile.GetTile(bugType);
                var placement = SelectBestPlacement(validPlacements, BugTeam.Dark);

                newTile.Location = placement;
                _board.AddTile(newTile);
                return true;
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

            // If no valid board moves remain, place a new piece from the pile.
            if (validPlacements.Count > 0 && availableBugTypes.Count > 0)
            {
                var bugType = SelectBestBugType(availableBugTypes, queenPlaced);
                var newTile = _board.OpponentPile.GetTile(bugType);
                var placement = SelectBestPlacement(validPlacements, BugTeam.Dark);

                newTile.Location = placement;
                _board.AddTile(newTile);
                return true;
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

        private Hex SelectBestPlacement(List<Hex> validPlacements, BugTeam team)
        {
            if (validPlacements.Count == 0)
                return new Hex(0, 0, 0);

            return validPlacements
                .OrderByDescending(position => ScorePlacement(position, team))
                .First();
        }

        private int ScorePlacement(Hex position, BugTeam team)
        {
            var score = 0;

            foreach (var neighbor in Enumerable.Range(0, 6).Select(i => position.Neighbor(i)))
            {
                if (_board.Tiles.TryGetValue(neighbor.GetHashCode(), out var tile))
                {
                    score += tile.Team == team ? 4 : -2;
                }
            }

            var centerDistance = Math.Abs(position.q) + Math.Abs(position.r) + Math.Abs(position.s);
            score += Math.Max(0, 3 - centerDistance);

            return score;
        }

        private BugType SelectBestBugType(List<BugType> availableBugTypes, bool queenPlaced)
        {
            if (availableBugTypes.Count == 0)
                return BugType.QueenBee;

            if (!queenPlaced && availableBugTypes.Contains(BugType.QueenBee))
                return BugType.QueenBee;

            var priority = new[]
            {
                BugType.SoldierAnt,
                BugType.Spider,
                BugType.Grasshopper,
                BugType.Beetle,
                BugType.Mosquito,
                BugType.PillBug,
                BugType.LadyBug,
                BugType.QueenBee
            };

            foreach (var bugType in priority)
            {
                if (availableBugTypes.Contains(bugType))
                    return bugType;
            }

            return availableBugTypes[_random.Next(availableBugTypes.Count)];
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
