using HiveServer;

var _serverCancellationToken = new CancellationTokenSource();
var _port = 7777;

await new GameServer().StartAsync(_serverCancellationToken.Token);