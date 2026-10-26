using HiveContracts;
using HiveLib.Rules;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;

namespace HiveLibTests;

[TestClass]
public class HiveGameStateTests
{
    [TestMethod]
    public void FirstLightMoveCanBePlacedAtOrigin()
    {
        var state = new HiveGameState();
        var result = state.TryApplyMove(new HiveMove(
            BugTeam.Light, BugType.QueenBee, new Hex(0, 0, 0), new Hex(0, 0, 0)));

        Assert.IsTrue(result.IsValid);
        Assert.IsTrue(result.IsPlacement);
        Assert.AreEqual(BugTeam.Dark, state.CurrentTeam);
        Assert.IsTrue(state.IsQueenPlaced(BugTeam.Light));
    }

    [TestMethod]
    public void MoveFromWrongTeamIsRejectedWithoutChangingState()
    {
        var state = new HiveGameState();
        var result = state.TryApplyMove(new HiveMove(
            BugTeam.Dark, BugType.QueenBee, new Hex(0, 0, 0), new Hex(0, 0, 0)));

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("NOT_YOUR_TURN", result.ErrorCode);
        Assert.AreEqual(0, state.Board.Tiles.Count);
        Assert.AreEqual(BugTeam.Light, state.CurrentTeam);
    }

    [TestMethod]
    public void PlacementTouchingOpponentOnlyIsRejectedAfterOpening()
    {
        var state = new HiveGameState();
        Apply(state, BugTeam.Light, BugType.QueenBee, new Hex(0, 0, 0), new Hex(0, 0, 0));
        Apply(state, BugTeam.Dark, BugType.QueenBee, new Hex(1, -1, 0), new Hex(1, -1, 0));

        var result = state.TryApplyMove(new HiveMove(
            BugTeam.Light, BugType.Beetle, new Hex(-100, 50, 50), new Hex(0, -1, 1)));

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("INVALID_PLACEMENT", result.ErrorCode);
        Assert.AreEqual(2, state.Board.Tiles.Count);
    }

    [TestMethod]
    public void QueenIsRequiredOnFourthTeamTurn()
    {
        var state = new HiveGameState();
        Apply(state, BugTeam.Light, BugType.Beetle, new Hex(0, 0, 0), new Hex(0, 0, 0));
        Apply(state, BugTeam.Dark, BugType.Beetle, new Hex(1, -1, 0), new Hex(1, -1, 0));
        Apply(state, BugTeam.Light, BugType.Grasshopper, new Hex(-1, 0, 1), new Hex(-1, 0, 1));
        Apply(state, BugTeam.Dark, BugType.Grasshopper, new Hex(2, -2, 0), new Hex(2, -2, 0));
        Apply(state, BugTeam.Light, BugType.Spider, new Hex(0, 1, -1), new Hex(0, 1, -1));
        Apply(state, BugTeam.Dark, BugType.Spider, new Hex(3, -3, 0), new Hex(3, -3, 0));

        var result = state.TryApplyMove(new HiveMove(
            BugTeam.Light, BugType.Beetle, new Hex(-100, 50, 50), new Hex(-1, 1, 0)));

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("QUEEN_REQUIRED", result.ErrorCode);
        Assert.AreEqual(6, state.Board.Tiles.Count);
    }

    [TestMethod]
    public void LegalPlacementAdvancesTurnAndConsumesPilePiece()
    {
        var state = new HiveGameState();
        Apply(state, BugTeam.Light, BugType.QueenBee, new Hex(0, 0, 0), new Hex(0, 0, 0));
        var before = state.Board.OpponentPile.GetCount(BugType.QueenBee);

        var result = state.TryApplyMove(new HiveMove(
            BugTeam.Dark, BugType.QueenBee, new Hex(1, -1, 0), new Hex(1, -1, 0)));

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(BugTeam.Light, state.CurrentTeam);
        Assert.AreEqual(before - 1, state.Board.OpponentPile.GetCount(BugType.QueenBee));
        Assert.AreEqual(2, state.Board.Tiles.Count);
    }

    private static void Apply(HiveGameState state, BugTeam team, BugType type, Hex from, Hex to)
    {
        var result = state.TryApplyMove(new HiveMove(team, type, from, to));
        Assert.IsTrue(result.IsValid, result.ErrorCode);
    }

    private static HiveGameState StateWithPiles(int lightPieces, int darkPieces)
    {
        var board = new HiveOnline.GameAssets.PlayingBoard(new ReadOnlyCollection<BugType>(Array.Empty<BugType>()))
        {
            UserPile = new HiveOnline.GameAssets.Pile(BugTeam.Light, 0, 0, 0, 0, 0, lightPieces, 0, 0),
            OpponentPile = new HiveOnline.GameAssets.Pile(BugTeam.Dark, 0, 0, 0, 0, 0, darkPieces, 0, 0)
        };
        return new HiveGameState(board);
    }

    [TestMethod]
    public void PassIsRejectedWhenTeamHasLegalMove()
    {
        var state = StateWithPiles(1, 1);
        var result = state.TryPass(BugTeam.Light);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("HAS_LEGAL_MOVE", result.ErrorCode);
        Assert.AreEqual(BugTeam.Light, state.CurrentTeam);
    }

    [TestMethod]
    public void TeamWithNothingToPlayPassesTurn()
    {
        var state = StateWithPiles(0, 1);
        Assert.IsFalse(state.HasLegalMove(BugTeam.Light));

        var result = state.TryPass(BugTeam.Light);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(BugTeam.Dark, state.CurrentTeam);
        Assert.IsFalse(state.IsFinished);
    }

    [TestMethod]
    public void BothTeamsStuckEndsInDraw()
    {
        var state = StateWithPiles(0, 0);
        state.TryPass(BugTeam.Light);
        var result = state.TryPass(BugTeam.Dark);

        Assert.IsTrue(result.IsValid);
        Assert.IsTrue(state.IsFinished);
        Assert.IsNull(state.WinningTeam);
    }

    [TestMethod]
    public void SecondPlayersFirstPieceMayTouchTheOpponent()
    {
        var state = StateWithPiles(1, 1);
        state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.QueenBee, new Hex(0, 0, 0), new Hex(0, 0, 0)));

        Assert.AreEqual(6, state.Board.OpponentPile.CalculateAvailable(state.Board, BugTeam.Dark).Count);
    }

    [TestMethod]
    public void BuriedTeamHasNoPlacements()
    {
        var state = StateWithPiles(1, 1);
        var board = state.Board;
        var darkQueen = new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(0, 0, 0) };
        var lightBeetle = new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = new Hex(1, -1, 0) };
        var darkBeetle = new HiveLib.Bugs.Beetle(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        board.AddTile(darkQueen);
        board.AddTile(lightBeetle);
        board.RemoveTile(lightBeetle);
        darkBeetle.CoveredPiece = lightBeetle;
        board.AddTile(darkBeetle);

        Assert.AreEqual(0, board.UserPile.CalculateAvailable(board, BugTeam.Light).Count);
    }

    [TestMethod]
    public void BeetleCannotClimbMoreThanOneLevel()
    {
        var board = StateWithPiles(1, 1).Board;
        var queen = new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var mover = new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = new Hex(0, 0, 0) };
        board.AddTile(queen);
        board.AddTile(mover);

        Assert.IsTrue(mover.CalculateAvailable(board).Contains(new Hex(1, -1, 0)));

        var top = new HiveLib.Bugs.Beetle(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        board.AddTile(top);

        Assert.IsFalse(mover.CalculateAvailable(board).Contains(new Hex(1, -1, 0)));
    }

    [TestMethod]
    public void BeetleCannotDropMoreThanOneLevel()
    {
        var board = StateWithPiles(1, 1).Board;
        var queen = new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var under = new HiveLib.Bugs.Beetle(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var mover = new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = new Hex(1, -1, 0) };
        board.AddTile(queen);
        board.AddTile(under);
        board.AddTile(mover);

        // Level 3 beside empty ground (level 1) is a two-level drop.
        Assert.IsFalse(mover.CalculateAvailable(board).Contains(new Hex(2, -2, 0)));
    }

    [TestMethod]
    public void LadyBugHopsTwiceOverPiecesThenDropsToGround()
    {
        var board = StateWithPiles(1, 1).Board;
        // A straight line of three pieces: ladybug on the left end, so it must cross the other two.
        var ladybug = new HiveLib.Bugs.LadyBug(BugTeam.Light) { Location = new Hex(0, 0, 0) };
        board.AddTile(ladybug);
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) });
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Light) { Location = new Hex(2, -2, 0) });

        var available = ladybug.CalculateAvailable(board);

        Assert.IsTrue(available.Contains(new Hex(3, -3, 0)));
        Assert.IsFalse(available.Contains(new Hex(0, 0, 0)), "cannot end where it started");
        Assert.IsFalse(available.Contains(new Hex(1, -1, 0)), "cannot stop on a piece");
        foreach (var hex in available)
            Assert.IsFalse(board.ContainsTile(hex));
    }

    [TestMethod]
    public void LadyBugIsNotBlockedByTallStacks()
    {
        var board = StateWithPiles(1, 1).Board;
        var ladybug = new HiveLib.Bugs.LadyBug(BugTeam.Light) { Location = new Hex(0, 0, 0) };
        board.AddTile(ladybug);
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) });
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Light) { Location = new Hex(2, -2, 0) });
        var low = new HiveLib.Bugs.Beetle(BugTeam.Dark) { Location = new Hex(2, -2, 0) };
        var mid = new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = new Hex(2, -2, 0) };
        var top = new HiveLib.Bugs.Beetle(BugTeam.Dark) { Location = new Hex(2, -2, 0) };
        foreach (var beetle in new[] { low, mid, top })
            board.AddTile(beetle);

        Assert.IsTrue(ladybug.CalculateAvailable(board).Contains(new Hex(3, -3, 0)));
    }

    [TestMethod]
    public void LadyBugCannotClimbBetweenTwoTallStacks()
    {
        // (3,-3,0) is only reachable by climbing onto (1,-1,0) and then (2,-2,0).
        // That first climb squeezes between the two gate hexes beside it.
        bool CanReachFarSide(bool tallGates)
        {
            var board = StateWithPiles(1, 1).Board;
            var ladybug = new HiveLib.Bugs.LadyBug(BugTeam.Light) { Location = new Hex(0, 0, 0) };
            board.AddTile(ladybug);
            board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) });
            board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Light) { Location = new Hex(2, -2, 0) });
            foreach (var gate in new[] { new Hex(0, -1, 1), new Hex(1, 0, -1) })
            {
                board.AddTile(new HiveLib.Bugs.Spider(BugTeam.Dark) { Location = gate });
                if (tallGates)
                    board.AddTile(new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = gate });
            }
            return ladybug.CalculateAvailable(board).Contains(new Hex(3, -3, 0));
        }

        Assert.IsTrue(CanReachFarSide(tallGates: false));
        Assert.IsFalse(CanReachFarSide(tallGates: true));
    }

    [TestMethod]
    public void LadyBugCanClimbWhenOnlyOneSideIsTall()
    {
        var board = StateWithPiles(1, 1).Board;
        var ladybug = new HiveLib.Bugs.LadyBug(BugTeam.Light) { Location = new Hex(0, 0, 0) };
        board.AddTile(ladybug);
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) });
        board.AddTile(new HiveLib.Bugs.QueenBee(BugTeam.Light) { Location = new Hex(2, -2, 0) });
        board.AddTile(new HiveLib.Bugs.Spider(BugTeam.Dark) { Location = new Hex(0, -1, 1) });
        board.AddTile(new HiveLib.Bugs.Beetle(BugTeam.Light) { Location = new Hex(0, -1, 1) });

        Assert.IsTrue(ladybug.CalculateAvailable(board).Contains(new Hex(3, -3, 0)));
    }
}
