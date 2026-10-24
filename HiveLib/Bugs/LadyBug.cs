using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveLib.Bugs
{
    public class LadyBug : Tile
    {
        public LadyBug(BugTeam bugTeam)
        {
            Type = BugType.LadyBug;
            Team = bugTeam;
        }

        // Two hops across occupied hexes (any stack height), then one step down onto an empty hex.
        // Each step must pass the freedom-to-move gate rule.
        public override List<Hex> CalculateAvailable(PlayingBoard board)
        {
            var hive = GetBoardWithoutMe(board);
            var results = new Dictionary<int, Hex>();

            for (var first = 0; first < 6; first++)
            {
                var firstHex = Location.Neighbor(first);
                if (!hive.ContainsKey(firstHex.GetHashCode()) || IsGateBlocked(hive, Location, first, 0, Height(hive, firstHex)))
                    continue;

                for (var second = 0; second < 6; second++)
                {
                    var secondHex = firstHex.Neighbor(second);
                    if (secondHex == firstHex || !hive.ContainsKey(secondHex.GetHashCode()) ||
                        IsGateBlocked(hive, firstHex, second, Height(hive, firstHex), Height(hive, secondHex)))
                        continue;

                    for (var third = 0; third < 6; third++)
                    {
                        var landing = secondHex.Neighbor(third);
                        if (landing == Location || landing == firstHex || hive.ContainsKey(landing.GetHashCode()) ||
                            IsGateBlocked(hive, secondHex, third, Height(hive, secondHex), 0))
                            continue;
                        results[landing.GetHashCode()] = landing;
                    }
                }
            }

            return results.Values.ToList();
        }

        private static int Height(Dictionary<int, ITile> hive, Hex hex)
        {
            if (!hive.TryGetValue(hex.GetHashCode(), out var top))
                return 0;

            var height = 1;
            for (var covered = (top as Beetle)?.CoveredPiece; covered != null; covered = (covered as Beetle)?.CoveredPiece)
                height++;
            return height;
        }

        // A gate: both hexes beside the step are taller than both the level we leave and the level we reach,
        // so the ladybug can't squeeze between them.
        private static bool IsGateBlocked(Dictionary<int, ITile> hive, Hex from, int direction, int fromLevel, int toLevel)
        {
            var left = Height(hive, from.Neighbor((direction + 5) % 6));
            var right = Height(hive, from.Neighbor((direction + 1) % 6));
            return Math.Min(left, right) > Math.Max(fromLevel, toLevel);
        }

        // The ladybug travels over the hive, so it doesn't need an open gap to slide through.
        public override bool HasFreedomToMove(PlayingBoard board) => true;
    }
}
