using HiveContracts;
using HiveLib.Bugs;
using HiveLib.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace HiveLibTests;

[TestClass]
public class PillbugTests
{
    private static readonly Hex Origin = new(0, 0, 0);
    private static readonly Hex Dark = new(1, -1, 0);
    private static readonly Hex Drop = new(0, -1, 1);

    private static HiveGameState StateWith(params HiveLib.GameAssets.ITile[] tiles)
    {
        var board = new HiveOnline.GameAssets.PlayingBoard(new ReadOnlyCollection<BugType>(new[] { BugType.PillBug }));
        foreach (var tile in tiles)
            board.AddTile(tile);
        return new HiveGameState(board);
    }

    private static HiveGameState Standard() => StateWith(
        new QueenBee(BugTeam.Dark) { Location = Dark },
        new PillBug(BugTeam.Light) { Location = Origin },
        new SoldierAnt(BugTeam.Light) { Location = new Hex(-1, 1, 0) });

    [TestMethod]
    public void WarpMovesAndStunsPieceUntilOpponentMoves()
    {
        var state = Standard();
        var result = state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Drop, null, Dark));
        Assert.IsTrue(result.IsValid, result.ErrorCode);
        Assert.IsTrue(state.Board.Tiles[Drop.GetHashCode()].Stunned);
        Assert.IsFalse(state.Board.Tiles[Drop.GetHashCode()].CanMove(state.Board));
    }

    [TestMethod]
    public void WarpedPieceCannotBeWarpedAgain()
    {
        var state = Standard();
        Assert.IsTrue(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Drop, null, Dark)).IsValid);
        Assert.IsFalse(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Dark, null, Drop)).IsValid);
    }

    [TestMethod]
    public void WarpingBreakingTheHiveIsRejected()
    {
        var state = StateWith(
            new PillBug(BugTeam.Light) { Location = Origin },
            new QueenBee(BugTeam.Dark) { Location = Dark },
            new SoldierAnt(BugTeam.Dark) { Location = new Hex(2, -2, 0) });
        Assert.IsFalse(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Drop, null, Dark)).IsValid);
    }

    [TestMethod]
    public void NonAdjacentPieceCannotBeWarped()
    {
        var state = StateWith(
            new PillBug(BugTeam.Light) { Location = Origin },
            new QueenBee(BugTeam.Dark) { Location = Dark },
            new SoldierAnt(BugTeam.Dark) { Location = new Hex(2, -2, 0) });
        Assert.IsFalse(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Drop, null, new Hex(2, -2, 0))).IsValid);
    }

    [TestMethod]
    public void StunLastsUntilSecondTurnAfterWarp()
    {
        var darkAnt = new SoldierAnt(BugTeam.Dark) { Location = new Hex(1, 0, -1) };
        var lightAnt = new SoldierAnt(BugTeam.Light) { Location = new Hex(-1, 1, 0) };
        var state = StateWith(
            new QueenBee(BugTeam.Dark) { Location = Dark },
            darkAnt,
            new PillBug(BugTeam.Light) { Location = Origin },
            lightAnt);
        Assert.IsTrue(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.PillBug, Origin, Drop, null, Dark)).IsValid);
        var warped = state.Board.Tiles[Drop.GetHashCode()];

        var to = darkAnt.CalculateAvailable(state.Board)[0];
        Assert.IsTrue(state.TryApplyMove(new HiveMove(BugTeam.Dark, BugType.SoldierAnt, darkAnt.Location, to)).IsValid);
        Assert.IsTrue(warped.Stunned);

        to = lightAnt.CalculateAvailable(state.Board)[0];
        Assert.IsTrue(state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.SoldierAnt, lightAnt.Location, to)).IsValid);
        Assert.IsFalse(warped.Stunned);
    }
}
