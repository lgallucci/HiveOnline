using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using HiveClient;
using HiveServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetworkingTest;

[TestClass]
public class TlsTests
{
    private GameServer _server = null!;
    private CancellationTokenSource _cts = null!;
    private int _port;
    private string _fingerprint = null!;

    [TestInitialize]
    public async Task Start()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=localhost", key, HashAlgorithmName.SHA256);
        using var created = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var certificate = X509CertificateLoader.LoadPkcs12(created.Export(X509ContentType.Pfx), null);
        _fingerprint = Convert.ToHexString(SHA256.HashData(certificate.PublicKey.ExportSubjectPublicKeyInfo()));

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        _port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        _server = new GameServer(_port) { Certificate = certificate };
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => _server.StartAsync(_cts.Token));
        await Task.Delay(300);
    }

    [TestCleanup]
    public async Task Stop()
    {
        _cts.Cancel();
        await _server.StopAsync(CancellationToken.None);
    }

    private static async Task<bool> ReceivesWelcome(HiveGameClient client)
    {
        var welcome = new TaskCompletionSource<bool>();
        client.MessageReceived += m =>
        {
            if (m.StartsWith("WELCOME "))
                welcome.TrySetResult(true);
        };
        using var cts = new CancellationTokenSource();
        _ = Task.Run(() => client.ConnectAsync(cts.Token));
        var finished = await Task.WhenAny(welcome.Task, Task.Delay(4000));
        cts.Cancel();
        return finished == welcome.Task;
    }

    [TestMethod]
    public async Task PinnedFingerprintConnects()
    {
        var client = new HiveGameClient("localhost", _port) { UseTls = true, ServerKeyPin = _fingerprint };
        Assert.IsTrue(await ReceivesWelcome(client));
    }

    [TestMethod]
    public async Task WrongFingerprintIsRejected()
    {
        var client = new HiveGameClient("localhost", _port) { UseTls = true, ServerKeyPin = new string('A', 64) };
        Assert.IsFalse(await ReceivesWelcome(client));
    }

    [TestMethod]
    public async Task UntrustedSelfSignedCertificateIsRejectedWithoutPinning()
    {
        var client = new HiveGameClient("localhost", _port) { UseTls = true };
        Assert.IsFalse(await ReceivesWelcome(client));
    }

    [TestMethod]
    public async Task PlaintextClientIsRejected()
    {
        var client = new HiveGameClient("localhost", _port);
        Assert.IsFalse(await ReceivesWelcome(client));
    }
}
