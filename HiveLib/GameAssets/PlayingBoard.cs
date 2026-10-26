using HiveContracts;
using HiveLib;
using HiveLib.GameAssets;
using System.Collections.Generic;
using System;
using System.Linq;

namespace HiveOnline.GameAssets
{
    public class PlayingBoard : IBoard
    {
        public ChatBox ChatWindow { get; set; }

        public ITile SelectedTile { get; set; }
        private readonly Dictionary<int, ITile> _tiles = new Dictionary<int, ITile>();
        private readonly Dictionary<int, Hex> _availableTiles = new Dictionary<int, Hex>();
        private readonly Dictionary<int, Hex> _choiceTargets = new Dictionary<int, Hex>();
        private readonly Dictionary<int, Hex> _testSpots = new Dictionary<int, Hex>();
        public IReadOnlyDictionary<int, ITile> Tiles => _tiles;
        public IReadOnlyDictionary<int, Hex> AvailableTiles => _availableTiles;
        public IReadOnlyDictionary<int, Hex> ChoiceTargets => _choiceTargets;

        // Where a piece most recently landed. The Pillbug can't lift a piece that was just moved.
        public Hex? LastMovedHex { get; private set; }

        public string UserName { get; set; } = "TestUser";
        public Pile UserPile { get; set; }
        public string OpponentName { get; set; } = "TestOpponent";
        public Pile OpponentPile { get; set; }
        public string CurrentTurn { get; set; } = "Your Turn";

        public IReadOnlyDictionary<int, Hex> TestSpots => _testSpots;
        public int Version { get; private set; }

        public PlayingBoard(IReadOnlyCollection<BugType> selectedExpansions)
        {
            ChatWindow = new ChatBox();
            UserPile = new Pile(BugTeam.Light, 
                ladyBugCount: selectedExpansions?.Contains(BugType.LadyBug) == true ? 1 : 0,
                mosquitoCount: selectedExpansions?.Contains(BugType.Mosquito) == true ? 1 : 0,
                pillBugCount: selectedExpansions?.Contains(BugType.PillBug) == true ? 1 : 0);
            OpponentPile = new Pile(BugTeam.Dark, 
                ladyBugCount: selectedExpansions?.Contains(BugType.LadyBug) == true ? 1 : 0,
                mosquitoCount: selectedExpansions?.Contains(BugType.Mosquito) == true ? 1 : 0,
                pillBugCount: selectedExpansions?.Contains(BugType.PillBug) == true ? 1 : 0);
        }

        public void Move(Tile piece, Tile position)
        {
            if (!piece.CanMove(this) || !piece.CanMoveTo(this, position.Location))
                throw new PlayException("Illegal Move!");

            piece.Location = position.Location;
        }

        // Called once per move or pass to age every stun, including pieces under a stack.
        public void ClearStunned()
        {
            foreach (var top in _tiles.Values)
            {
                for (var tile = top; tile != null; tile = (tile as HiveLib.Bugs.Beetle)?.CoveredPiece)
                    tile.AdvanceStun();
            }
        }

        public void ClearAbilityChoices(BugTeam team)
        {
            foreach (var chooser in _tiles.Values.Where(t => t.Team == team).OfType<IChoosesAbility>())
                chooser.ClearChoice();
        }

        // A queen is surrounded when all six neighbours hold a piece, friendly or not.
        public bool IsQueenSurrounded(ITile queen) =>
            Enumerable.Range(0, 6).All(i => ContainsTile(queen.Location.Neighbor(i)));

        public bool IsQueenSurrounded(BugTeam team) =>
            AllQueens().Any(t => t.Team == team && IsQueenSurrounded(t));

        public bool BothQueensSurrounded()
        {
            var queens = AllQueens().ToList();
            return queens.Count == 2 && queens.All(IsQueenSurrounded);
        }

        // Queens count even when buried under a beetle or a mosquito acting as one.
        private IEnumerable<ITile> AllQueens()
        {
            foreach (var top in _tiles.Values)
            {
                for (var tile = top; tile != null; tile = (tile as HiveLib.Bugs.Beetle)?.CoveredPiece)
                {
                    if (tile.Type == BugType.QueenBee)
                        yield return tile;
                }
            }
        }

        // Validates and applies the move of a piece already on the board, including choice abilities such as a copy or warp.
        public bool TryMovePiece(ITile piece, Hex destination, BugType? copiedBug, Hex? warpTarget, out ITile movedPiece)
        {
            movedPiece = null;
            if (!_tiles.TryGetValue(piece.Location.GetHashCode(), out var boardTile) || boardTile != piece)
                return false;

            var chooser = piece as IChoosesAbility;
            var warping = chooser is not null && warpTarget.HasValue;
            if (chooser is not null)
            {
                chooser.Choice = copiedBug;
                chooser.Target = warpTarget;
            }

            var allowed = chooser is not null
                ? chooser.IsSelectionValid(this) && (warping || piece.CanMove(this)) &&
                  chooser.GetDestinations(this).Any(hex => hex == destination)
                : piece.CanMove(this) && piece.CanMoveTo(this, destination);
            if (!allowed)
            {
                chooser?.ClearChoice();
                return false;
            }

            ClearStunned();
            if (warping)
            {
                PillbugWarp.Apply(this, warpTarget.Value, destination);
                chooser.Target = null;
            }
            else
            {
                RemoveTile(boardTile);
                piece.Location = destination;
                AddTile(piece);
            }

            movedPiece = piece;
            return true;
        }

        public void AddTile(ITile tile)
        {
            LastMovedHex = tile.Location;
            if (ContainsTile(tile)) 
            {
                tile.RunAddRules(_tiles[tile.GetHashCode()]);
                _tiles[tile.GetHashCode()] = tile;
                Version++;
            }
            else
            {
                _tiles.Add(tile.GetHashCode(), tile);
                Version++;
            }
        }

        public bool ContainsTile(ITile tile)
        {
            return _tiles.ContainsKey(tile.GetHashCode());
        }

        public bool ContainsTile(Hex tile)
        {
            return _tiles.ContainsKey(tile.GetHashCode());
        }

        public void AddAvailableHexes(List<Hex> hexes)
        {
            foreach (var hex in hexes)
            { 
                _availableTiles[hex.GetHashCode()] = hex;
            }
        }

        public void ClearAvailableTiles()
        {
            _testSpots.Clear();
            _availableTiles.Clear();
            _choiceTargets.Clear();
        }

        public void AddChoiceTargets(List<Hex> hexes)
        {
            foreach (var hex in hexes)
                _choiceTargets[hex.GetHashCode()] = hex;
        }

        public void ClearTestSpots()
        {
            _testSpots.Clear();
        }

        public void AddTestSpot(int key, Hex location)
        {
            _testSpots[key] = location;
        }

        public void RemoveTile(ITile tile)
        {
            var replacementTile = tile.RunRemoveRules();

            if (replacementTile != null)
            {
                _tiles[tile.GetHashCode()] = replacementTile;
                Version++;
            }
            else
            {
                _tiles.Remove(tile.GetHashCode());
                Version++;
            }
        }
    }
}
