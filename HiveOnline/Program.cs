using System;

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
        static void Main(string[] args)
        {
            var modeArg = args.Length > 0 ? args : new[] { Environment.GetEnvironmentVariable("HIVE_MODE") ?? "" };
            var mode = ParseMode(modeArg);
            var host = args.Length > 1 ? args[1] : "localhost";
            var port = args.Length > 2 && int.TryParse(args[2], out var parsedPort) ? parsedPort : 7777;
            new HiveOnlineGame(mode, host, port).Run();
        }

        private static GameMode ParseMode(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                return GameMode.LocalAi;

            var value = args[0].Trim();
            if (string.Equals(value, "ai", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "local-ai", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "localai", StringComparison.OrdinalIgnoreCase))
            {
                return GameMode.LocalAi;
            }

            if (string.Equals(value, "multiplayer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "network", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "server", StringComparison.OrdinalIgnoreCase))
            {
                return GameMode.Multiplayer;
            }

            if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "two-player", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "2p", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "local-two-player", StringComparison.OrdinalIgnoreCase))
            {
                return GameMode.LocalTwoPlayer;
            }

            return GameMode.LocalAi;
        }
    }
}
