using HiveContracts;
using HiveOnline.GameAssets;
using System.Collections.Generic;

namespace HiveLib.GameAssets
{
    // A tile whose move takes extra clicks before its destinations are known (Mosquito copying, Pillbug warping).
    // The player clicks choice targets until none remain, then clicks a destination.
    public interface IChoosesAbility
    {
        // Ability copied for the current move. It stays set (and is drawn) until the owner's next turn starts.
        BugType? Choice { get; set; }

        // Piece being acted on (the piece a Pillbug warps). Only used while the move is being built.
        Hex? Target { get; set; }

        void ClearChoice();

        // Hexes the player may click now to advance the choice. Empty once nothing is left to choose.
        List<Hex> GetChoiceTargets(PlayingBoard board);

        // Applies a click on one of the choice targets. Returns false if the hex isn't a valid choice.
        bool TryChoose(PlayingBoard board, Hex hex);

        // Hexes the piece may move to given the choices made so far.
        List<Hex> GetDestinations(PlayingBoard board);

        // True if Choice and Target are consistent with the board. Used to validate moves from the network.
        bool IsSelectionValid(PlayingBoard board);
    }
}
