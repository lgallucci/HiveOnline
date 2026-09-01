using System;
using System.Collections.Generic;
using System.Linq;

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

        public ConnectedHiveClient GetOpponent(ConnectedHiveClient client)
        {
            if (ReferenceEquals(client, PlayerOne))
                return PlayerTwo;

            if (ReferenceEquals(client, PlayerTwo))
                return PlayerOne;

            throw new InvalidOperationException("Client is not a member of this game.");
        }

        public bool Contains(ConnectedHiveClient client)
        {
            return ReferenceEquals(client, PlayerOne) || ReferenceEquals(client, PlayerTwo);
        }
    }
}