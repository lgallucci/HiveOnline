namespace HiveOnline
{
    public enum GameMode
    {
        LocalAi,
        Multiplayer,
        LocalTwoPlayer
    }

    class Program
    {
        // The public game server, used when no host argument is given.
        private const string DefaultServerHost = "34.29.94.144";

        // Usage: HiveOnline [host] [port]. The game mode is chosen on the opening screen.
        static void Main(string[] args)
        {
            var host = args.Length > 0 ? args[0] : DefaultServerHost;
            var port = args.Length > 1 && int.TryParse(args[1], out var parsedPort) ? parsedPort : 7777;
            new HiveOnlineGame(host, port).Run();
        }
    }
}
