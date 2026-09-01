using System;
using System.Collections.Generic;

namespace HiveServer
{
    internal class HiveGame
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ConnectedHiveClient PlayerOne { get; }
        public ConnectedHiveClient PlayerTwo { get; }

        public HiveGame(ConnectedHiveClient playerOne, ConnectedHiveClient playerTwo)
        {
            PlayerOne = playerOne ?? throw new ArgumentNullException(nameof(playerOne));
            PlayerTwo = playerTwo ?? throw new ArgumentNullException(nameof(playerTwo));
        }

        public IReadOnlyCollection<ConnectedHiveClient> Players => new[] { PlayerOne, PlayerTwo };
    }
}