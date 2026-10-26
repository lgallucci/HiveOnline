using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System.Collections.Generic;
using System.Linq;

namespace HiveLib.Bugs
{
    // Extends Beetle so it can cover pieces when it climbs onto the hive.
    public class Mosquito : Beetle, IChoosesAbility
    {
        public Mosquito(BugTeam bugTeam) : base(bugTeam)
        {
            Type = BugType.Mosquito;
        }

        // The ability being used for the current move. Only set while a move is being evaluated or applied.
        private BugType? _choice;

        // While elevated it is always a Beetle, so that choice can't be cleared until it climbs down.
        public BugType? Choice
        {
            get => IsElevated ? BugType.Beetle : _choice;
            set => _choice = value;
        }

        public bool IsElevated => CoveredPiece != null;

        // Adjacent top-of-stack pieces that can be copied. Other mosquitos can't be copied.
        public List<ITile> GetCopySources(PlayingBoard board)
        {
            var sources = new List<ITile>();
            if (IsElevated)
                return sources;

            for (var i = 0; i < 6; i++)
            {
                if (board.Tiles.TryGetValue(Location.Neighbor(i).GetHashCode(), out var neighbor) && neighbor.Type != BugType.Mosquito)
                    sources.Add(neighbor);
            }
            return sources;
        }

        public List<BugType> GetCopyOptions(PlayingBoard board) =>
            IsElevated ? new List<BugType> { BugType.Beetle } : GetCopySources(board).Select(s => s.Type).Distinct().ToList();

        public Hex? Target { get; set; }

        public void ClearChoice()
        {
            _choice = null;
            Target = null;
        }

        public List<Hex> GetChoiceTargets(PlayingBoard board)
        {
            if (IsElevated)
                return new List<Hex>();

            if (_choice is null)
            {
                var targets = GetCopySources(board).Where(s => CanMoveAs(board, s.Type)).Select(s => s.Location).ToList();
                // Evaluating the copied abilities leaves their path previews behind; none should show before a choice.
                board.ClearTestSpots();
                return targets;
            }

            // A copied Pillbug still has to pick the piece it warps.
            return _choice == BugType.PillBug && Target is null ? PillbugWarp.GetTargets(board, this) : new List<Hex>();
        }

        public bool TryChoose(PlayingBoard board, Hex hex)
        {
            if (!GetChoiceTargets(board).Any(t => t == hex))
                return false;

            if (_choice is null)
                _choice = board.Tiles[hex.GetHashCode()].Type;
            else
                Target = hex;
            return true;
        }

        public List<Hex> GetDestinations(PlayingBoard board)
        {
            if (Choice is not { } type)
                return new List<Hex>();
            if (Target.HasValue)
                return PillbugWarp.GetDestinations(board, this, Target.Value);
            return CanMoveAs(board, type) ? CalculateAvailableAs(board, type) : new List<Hex>();
        }

        public bool IsSelectionValid(PlayingBoard board)
        {
            if (IsElevated)
                return Target is null;
            if (_choice is null || !GetCopyOptions(board).Contains(_choice.Value))
                return false;
            return Target is null || (_choice == BugType.PillBug && PillbugWarp.GetTargets(board, this).Any(t => t == Target.Value));
        }

        public List<Hex> CalculateAvailableAs(PlayingBoard board, BugType type)
        {
            var proxy = CreateProxy(IsElevated ? BugType.Beetle : type);
            return proxy is null ? new List<Hex>() : proxy.CalculateAvailable(board);
        }

        public bool CanMoveAs(PlayingBoard board, BugType type)
        {
            var proxy = CreateProxy(IsElevated ? BugType.Beetle : type);
            return proxy is not null && proxy.HasMoves(board);
        }

        private IEnumerable<BugType> ActiveTypes(PlayingBoard board)
        {
            if (IsElevated)
                return new[] { BugType.Beetle };
            return Choice.HasValue ? new[] { Choice.Value } : GetCopyOptions(board);
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board) =>
            ActiveTypes(board).SelectMany(t => CalculateAvailableAs(board, t)).GroupBy(h => h.GetHashCode()).Select(g => g.First()).ToList();

        public override bool CanMoveTo(PlayingBoard board, Hex position) =>
            CalculateAvailable(board).Any(h => h.GetHashCode() == position.GetHashCode());

        public override bool CanMove(PlayingBoard board) =>
            !Stunned && ActiveTypes(board).Any(t => CanMoveAs(board, t));

        public override bool HasMoves(PlayingBoard board) => CanMove(board);

        public override bool HasFreedomToMove(PlayingBoard board) =>
            ActiveTypes(board).Any(t => CalculateAvailableAs(board, t).Count > 0);

        private Tile CreateProxy(BugType type)
        {
            Tile proxy = type switch
            {
                BugType.QueenBee => new QueenBee(Team),
                BugType.Spider => new Spider(Team),
                BugType.Grasshopper => new Grasshopper(Team),
                BugType.SoldierAnt => new SoldierAnt(Team),
                BugType.LadyBug => new LadyBug(Team),
                BugType.PillBug => new PillBug(Team),
                BugType.Beetle => new Beetle(Team) { CoveredPiece = CoveredPiece },
                _ => null
            };
            if (proxy is not null)
                proxy.Location = Location;
            return proxy;
        }
    }
}
