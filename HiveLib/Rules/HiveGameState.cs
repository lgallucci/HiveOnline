using HiveContracts;
using HiveOnline.GameAssets;
using HiveLib.GameAssets;
using System;
using System.Linq;

namespace HiveLib.Rules;

public sealed class HiveGameState
{
    private readonly bool[] _queenPlaced = new bool[2];
    private readonly int[] _turnCounts = new int[2];

    public HiveGameState(PlayingBoard board = null, BugTeam startingTeam = BugTeam.Light)
    {
        Board = board ?? new PlayingBoard();
        CurrentTeam = startingTeam;
    }

    public PlayingBoard Board { get; }
    public BugTeam CurrentTeam { get; private set; }
    public bool IsFinished { get; private set; }
    public BugTeam? WinningTeam { get; private set; }

    public int GetTurnCount(BugTeam team) => _turnCounts[GetTeamIndex(team)];

    public bool IsQueenPlaced(BugTeam team) => _queenPlaced[GetTeamIndex(team)];

    public MoveResult TryApplyMove(HiveMove move)
    {
        if (IsFinished)
            return MoveResult.Rejected("GAME_FINISHED");
        if (move.Team != BugTeam.Light && move.Team != BugTeam.Dark)
            return MoveResult.Rejected("INVALID_TEAM");
        if (move.Team != CurrentTeam)
            return MoveResult.Rejected("NOT_YOUR_TURN");
        if (move.BugType == BugType.Blank || move.BugType == BugType.Selection || move.BugType == BugType.Available)
            return MoveResult.Rejected("INVALID_PIECE");

        var isPlacement = !Board.Tiles.ContainsKey(move.From.GetHashCode());
        ITile tile;
        Pile pile = null;

        if (isPlacement)
        {
            pile = GetPile(move.Team);
            tile = pile.Peek(move.BugType);
            if (tile == null)
                return MoveResult.Rejected("PIECE_UNAVAILABLE");
            if (MustPlayQueen(move.Team) && move.BugType != BugType.QueenBee)
                return MoveResult.Rejected("QUEEN_REQUIRED");
            if (!pile.CalculateAvailable(Board, move.Team).Any(hex => hex == move.To))
                return MoveResult.Rejected("INVALID_PLACEMENT");
        }
        else
        {
            tile = Board.Tiles[move.From.GetHashCode()];
            if (tile.Team != move.Team || tile.Type != move.BugType)
                return MoveResult.Rejected("INVALID_SOURCE");
            if (tile is Tile modelTile && !modelTile.CanMove(Board))
                return MoveResult.Rejected("PIECE_CANNOT_MOVE");
            if (!tile.CanMoveTo(Board, move.To))
                return MoveResult.Rejected("INVALID_DESTINATION");
        }

        if (isPlacement)
            tile = pile.GetTile(move.BugType);
        else
            Board.RemoveTile(tile);

        tile.Location = move.To;
        Board.AddTile(tile);
        if (tile.Type == BugType.QueenBee)
            _queenPlaced[GetTeamIndex(move.Team)] = true;

        _turnCounts[GetTeamIndex(move.Team)]++;
        IsFinished = IsQueenSurrounded(BugTeam.Light) || IsQueenSurrounded(BugTeam.Dark);
        if (IsFinished)
            WinningTeam = IsQueenSurrounded(CurrentTeam == BugTeam.Light ? BugTeam.Dark : BugTeam.Light)
                ? CurrentTeam
                : Opponent(CurrentTeam);
        else
            CurrentTeam = Opponent(CurrentTeam);

        Board.ClearAvailableTiles();
        return MoveResult.Accepted(isPlacement, IsFinished);
    }

    private bool MustPlayQueen(BugTeam team) =>
        _turnCounts[GetTeamIndex(team)] >= 3 && !_queenPlaced[GetTeamIndex(team)];

    private bool IsQueenSurrounded(BugTeam team)
    {
        var queen = Board.Tiles.Values.FirstOrDefault(tile => tile.Team == team && tile.Type == BugType.QueenBee);
        if (queen == null)
            return false;

        for (var index = 0; index < 6; index++)
        {
            if (!Board.ContainsTile(queen.Location.Neighbor(index)))
                return false;
        }
        return true;
    }

    private Pile GetPile(BugTeam team) => team == BugTeam.Light ? Board.UserPile : Board.OpponentPile;

    private static BugTeam Opponent(BugTeam team) => team == BugTeam.Light ? BugTeam.Dark : BugTeam.Light;

    private static int GetTeamIndex(BugTeam team) => team == BugTeam.Light ? 0 : 1;
}
