using HiveContracts;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using System;
using System.Linq;

namespace HiveLib.Rules;

public static class MoveAvailability
{
    // True if the team can place a piece or move a piece this turn.
    public static bool HasLegalMove(PlayingBoard board, Pile pile, BugTeam team, int turnCount, bool queenPlaced)
    {
        var mustPlayQueen = turnCount >= 3 && !queenPlaced;

        if (pile.CalculateAvailable(board, team).Count > 0)
        {
            foreach (var bugType in Enum.GetValues<BugType>())
            {
                if (pile.Peek(bugType) != null && (!mustPlayQueen || bugType == BugType.QueenBee))
                    return true;
            }
        }

        // Pieces on the board may only move once the queen is down.
        if (!queenPlaced)
            return false;

        return board.Tiles.Values.Any(tile => tile.Team == team && tile.HasMoves(board));
    }
}
