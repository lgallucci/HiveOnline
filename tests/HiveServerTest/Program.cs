using HiveServer;

var _serverCancellationToken = new CancellationTokenSource();

await new GameServer().StartAsync(_serverCancellationToken.Token);