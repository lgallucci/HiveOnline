using HiveContracts;
using HiveLib.Bugs;
using HiveLib.SearchAlgorithms;
using HiveOnline.GameAssets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HiveLib.GameAssets
{
    // The Pillbug's special ability, shared by the Pillbug and a Mosquito copying it.
    public static class PillbugWarp
    {
        // Adjacent pieces the warper can lift.
        public static List<Hex> GetTargets(PlayingBoard board, ITile warper)
        {
            var targets = new List<Hex>();
            if (warper.Stunned)
                return targets;

            for (var i = 0; i < 6; i++)
            {
                var hex = warper.Location.Neighbor(i);
                if (board.Tiles.TryGetValue(hex.GetHashCode(), out var piece) && CanWarp(board, warper, piece))
                    targets.Add(hex);
            }
            return targets;
        }

        // Empty hexes next to the warper that the lifted piece can be dropped into.
        public static List<Hex> GetDestinations(PlayingBoard board, ITile warper, Hex target)
        {
            var destinations = new List<Hex>();
            if (!board.Tiles.ContainsKey(target.GetHashCode()))
                return destinations;

            var without = board.Tiles.ToDictionary(t => t.Key, t => t.Value);
            without.Remove(target.GetHashCode());

            var warperLevel = Height(board.Tiles, warper.Location) + 1;
            // The piece climbs from the ground onto the warper, then drops from there into an empty hex.
            if (!PassesGate(board.Tiles, target, warper.Location, 1, warperLevel))
                return destinations;

            for (var i = 0; i < 6; i++)
            {
                var hex = warper.Location.Neighbor(i);
                if (hex == target || without.ContainsKey(hex.GetHashCode()))
                    continue;
                if (PassesGate(without, warper.Location, hex, warperLevel, 1))
                    destinations.Add(hex);
            }
            return destinations;
        }

        // Moves the target piece and stuns it until the next turn has been played.
        public static void Apply(PlayingBoard board, Hex target, Hex destination)
        {
            var piece = board.Tiles[target.GetHashCode()];
            board.RemoveTile(piece);
            piece.Location = destination;
            board.AddTile(piece);
            piece.Stunned = true;
        }

        private static bool CanWarp(PlayingBoard board, ITile warper, ITile piece)
        {
            if (piece.Stunned || (piece as Beetle)?.CoveredPiece is not null)
                return false;
            if (board.LastMovedHex.HasValue && board.LastMovedHex.Value == piece.Location)
                return false;

            var without = board.Tiles.ToDictionary(t => t.Key, t => t.Value);
            without.Remove(piece.GetHashCode());
            if (!without.TryGetValue(warper.Location.GetHashCode(), out var hiveStart) ||
                !BreadthFirstSearch.CheckSingleHive(without, hiveStart))
                return false;

            return GetDestinations(board, warper, piece.Location).Count > 0;
        }

        private static int Height(IReadOnlyDictionary<int, ITile> tiles, Hex hex)
        {
            if (!tiles.TryGetValue(hex.GetHashCode(), out var top))
                return 0;

            var height = 1;
            for (var covered = (top as Beetle)?.CoveredPiece; covered is not null; covered = (covered as Beetle)?.CoveredPiece)
                height++;
            return height;
        }

        // A piece can't squeeze between two stacks that are both at least as tall as the level it moves at.
        private static bool PassesGate(IReadOnlyDictionary<int, ITile> tiles, Hex from, Hex to, int fromLevel, int toLevel)
        {
            var direction = Enumerable.Range(0, 6).First(i => from.Neighbor(i) == to);
            var level = Math.Max(fromLevel, toLevel);
            return Height(tiles, from.Neighbor((direction + 5) % 6)) < level ||
                   Height(tiles, from.Neighbor((direction + 1) % 6)) < level;
        }
    }
}
