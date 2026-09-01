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
            var mode = ParseMode(args);
            new HiveOnlineGame(mode).Run();
        }

        private static GameMode ParseMode(string[] args)
        {
            if (args.Length == 0)
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
