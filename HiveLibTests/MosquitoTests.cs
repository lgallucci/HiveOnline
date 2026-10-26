using HiveContracts;
using HiveLib.Bugs;
using HiveLib.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq;

namespace HiveLibTests;

[TestClass]
public class MosquitoTests
{
    private static readonly Hex Origin = new(0, 0, 0);

    private static HiveGameState StateWith(params HiveLib.GameAssets.ITile[] tiles)
    {
        var board = new HiveOnline.GameAssets.PlayingBoard(new ReadOnlyCollection<BugType>(new[] { BugType.Mosquito }));
        foreach (var tile in tiles)
            board.AddTile(tile);
        return new HiveGameState(board);
    }

    [TestMethod]
    public void MosquitoCopiesAdjacentGrasshopper()
    {
        var mosquito = new Mosquito(BugTeam.Light) { Location = Origin };
        var state = StateWith(mosquito,
            new Grasshopper(BugTeam.Dark) { Location = new Hex(1, -1, 0) },
            new QueenBee(BugTeam.Dark) { Location = new Hex(1, 0, -1) });

        Assert.AreEqual("INVALID_COPY", state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.Mosquito, Origin, new Hex(2, -2, 0))).ErrorCode);
        Assert.AreEqual("INVALID_COPY", state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.Mosquito, Origin, new Hex(2, -2, 0), BugType.Spider)).ErrorCode);

        var result = state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.Mosquito, Origin, new Hex(2, -2, 0), BugType.Grasshopper));
        Assert.IsTrue(result.IsValid, result.ErrorCode);
    }

    [TestMethod]
    public void MosquitoCannotUseCopiedAbilityOutsideItsRange()
    {
        var mosquito = new Mosquito(BugTeam.Light) { Location = Origin };
        var state = StateWith(mosquito,
            new Grasshopper(BugTeam.Dark) { Location = new Hex(1, -1, 0) },
            new QueenBee(BugTeam.Dark) { Location = new Hex(1, 0, -1) });

        var result = state.TryApplyMove(new HiveMove(BugTeam.Light, BugType.Mosquito, Origin, new Hex(-1, 0, 1), BugType.Grasshopper));
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void MosquitoTouchingOnlyMosquitoIsStranded()
    {
        var mosquito = new Mosquito(BugTeam.Light) { Location = Origin };
        var board = StateWith(mosquito,
            new Mosquito(BugTeam.Dark) { Location = new Hex(1, -1, 0) },
            new QueenBee(BugTeam.Dark) { Location = new Hex(2, -2, 0) }).Board;

        Assert.IsFalse(mosquito.CanMove(board));
        Assert.AreEqual(0, mosquito.CalculateAvailable(board).Count);
    }

    [TestMethod]
    public void MosquitoCopiesBeetleOnTopOfStack()
    {
        var mosquito = new Mosquito(BugTeam.Light) { Location = Origin };
        var queen = new QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var beetle = new Beetle(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var board = StateWith(mosquito, new QueenBee(BugTeam.Light) { Location = new Hex(-1, 0, 1) }, queen, beetle).Board;

        var sources = mosquito.GetCopySources(board);
        Assert.IsTrue(sources.Any(s => s.Type == BugType.Beetle));
        Assert.IsFalse(sources.Any(s => s.Type == BugType.QueenBee && s.Location == beetle.Location));
    }

    [TestMethod]
    public void ElevatedMosquitoActsAsBeetle()
    {
        var mosquito = new Mosquito(BugTeam.Light) { Location = Origin };
        var queen = new QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) };
        var spider = new Spider(BugTeam.Dark) { Location = new Hex(2, -2, 0) };
        var state = StateWith(queen, spider, mosquito);
        // Climb onto the queen as a beetle, then move down again without copying anything.
        mosquito.Location = queen.Location;
        state.Board.AddTile(mosquito);
        Assert.IsTrue(mosquito.IsElevated);
        Assert.AreEqual(0, mosquito.GetCopySources(state.Board).Count);
        Assert.IsTrue(mosquito.CalculateAvailable(state.Board).Any(h => h == spider.Location));
    }
}
