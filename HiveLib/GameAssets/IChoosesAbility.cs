using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.GameAssets
{
    // A tile that needs an extra click on a target before its destinations are known (e.g. Mosquito, Pillbug).
    public interface IChoosesAbility
    {
        // Choice applied while evaluating or applying a move. Null when none is active.
        BugType? Choice { get; set; }

        // Hexes the player may click first. Empty means no choice is needed.
        List<Hex> GetChoiceTargets(PlayingBoard board);

        BugType? GetChoice(PlayingBoard board, Hex target);

        List<Hex> CalculateAvailableFor(PlayingBoard board, BugType? choice);

        bool IsChoiceValid(PlayingBoard board, BugType? choice);
    }
}
