using System.Net;
using System.Net.Sockets;
using HiveClient;
using HiveServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkingTest;

[TestClass]
public class ConnectionProblemTests
{
    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task<bool> WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return true;
            await Task.Delay(50);
        }
        return false;
    }

    [TestMethod]
    public async Task UnreachableServerReportsProblem()
    {
        var client = new HiveGameClient("localhost", FreePort());
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => client.ConnectAsync(cts.Token));

        Assert.IsTrue(await WaitUntil(() => client.ConnectionProblem != null));
        StringAssert.Contains(client.ConnectionProblem, "Could not reach the server");
        Assert.IsFalse(client.IsConnected);
        cts.Cancel();
    }

    [TestMethod]
    public async Task ServerShutdownReportsLostConnectionAndRecoveryClearsIt()
    {
        var port = FreePort();
        var server = new GameServer(port);
        using var serverCts = new CancellationTokenSource();
        _ = Task.Run(() => server.StartAsync(serverCts.Token));
        await Task.Delay(300);

        var client = new HiveGameClient("localhost", port);
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => client.ConnectAsync(cts.Token));
        Assert.IsTrue(await WaitUntil(() => client.IsConnected));
        Assert.IsNull(client.ConnectionProblem);

        serverCts.Cancel();
        await server.StopAsync(CancellationToken.None);
        Assert.IsTrue(await WaitUntil(() => client.ConnectionProblem != null));
        cts.Cancel();
    }
}
