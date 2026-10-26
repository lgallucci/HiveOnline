using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System.Collections.Generic;
using System.Linq;

namespace HiveLib.Bugs
{
    public class PillBug : Tile, IChoosesAbility
    {
        public PillBug(BugTeam bugTeam)
        {
            Type = BugType.PillBug;
            Team = bugTeam;
        }

        // A Pillbug never copies anything; this only exists to satisfy the interface.
        public BugType? Choice { get; set; }

        public Hex? Target { get; set; }

        public void ClearChoice()
        {
            Choice = null;
            Target = null;
        }

        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            if (Target.HasValue)
                return PillbugWarp.GetDestinations(board, this, Target.Value);
            return Steps(board);
        }

        // Walks like a Queen, but only while it is free to leave its spot.
        private List<Hex> Steps(PlayingBoard board) =>
            base.CanMove(board) ? new QueenBee(Team) { Location = Location }.CalculateAvailable(board) : new List<Hex>();

        public override bool CanMove(PlayingBoard board) =>
            !Stunned && (Steps(board).Count > 0 || PillbugWarp.GetTargets(board, this).Count > 0);

        public override bool HasMoves(PlayingBoard board) => CanMove(board);

        public List<Hex> GetChoiceTargets(PlayingBoard board) =>
            Target.HasValue ? new List<Hex>() : PillbugWarp.GetTargets(board, this);

        public bool TryChoose(PlayingBoard board, Hex hex)
        {
            if (Target.HasValue || !PillbugWarp.GetTargets(board, this).Any(t => t == hex))
                return false;

            Target = hex;
            return true;
        }

        public List<Hex> GetDestinations(PlayingBoard board) => CalculateAvailable(board);

        public bool IsSelectionValid(PlayingBoard board) =>
            Choice is null && (Target is null || PillbugWarp.GetTargets(board, this).Any(t => t == Target.Value));
    }
}
