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
    private int _consecutivePasses;

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

    public bool HasLegalMove(BugTeam team) =>
        MoveAvailability.HasLegalMove(Board, GetPile(team), team, GetTurnCount(team), IsQueenPlaced(team));

    // A team with no legal move must skip its turn. If both teams are stuck in a row the game is a draw.
    public MoveResult TryPass(BugTeam team)
    {
        if (IsFinished)
            return MoveResult.Rejected("GAME_FINISHED");
        if (team != CurrentTeam)
            return MoveResult.Rejected("NOT_YOUR_TURN");
        if (HasLegalMove(team))
            return MoveResult.Rejected("HAS_LEGAL_MOVE");

        _consecutivePasses++;
        if (_consecutivePasses >= 2)
            IsFinished = true;
        else
            CurrentTeam = Opponent(team);

        Board.ClearAvailableTiles();
        return MoveResult.Accepted(false, IsFinished);
    }

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
        _consecutivePasses = 0;
        var lightSurrounded = IsQueenSurrounded(BugTeam.Light);
        var darkSurrounded = IsQueenSurrounded(BugTeam.Dark);
        IsFinished = lightSurrounded || darkSurrounded;
        if (IsFinished)
            WinningTeam = lightSurrounded && darkSurrounded ? null : lightSurrounded ? BugTeam.Dark : BugTeam.Light;
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

    private Pile GetPile(BugTeam team) => Board.UserPile.Team == team ? Board.UserPile : Board.OpponentPile;

    private static BugTeam Opponent(BugTeam team) => team == BugTeam.Light ? BugTeam.Dark : BugTeam.Light;

    private static int GetTeamIndex(BugTeam team) => team == BugTeam.Light ? 0 : 1;
}
