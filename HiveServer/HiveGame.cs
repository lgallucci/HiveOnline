using System;
using System.Collections.Generic;
using System.Linq;
using HiveContracts;
using HiveLib.Rules;
using HiveOnline.GameAssets;

namespace HiveServer
{
    internal class HiveGame
    {
        public Guid Id { get; } = Guid.NewGuid();

        public ConnectedHiveClient PlayerOne { get; }
        public ConnectedHiveClient PlayerTwo { get; }
        private readonly BugType[] _selectedExpansions;
        public string EnabledExpansionCodes { get; }
        public HiveGameState State { get; private set; }

        public HiveGame(ConnectedHiveClient playerOne, ConnectedHiveClient playerTwo, string expansionCodes = "")
        {
            PlayerOne = playerOne ?? throw new ArgumentNullException(nameof(playerOne));
            PlayerTwo = playerTwo ?? throw new ArgumentNullException(nameof(playerTwo));
            expansionCodes ??= string.Empty;
            EnabledExpansionCodes = new string("LPM".Where(code => expansionCodes.Contains(code)).ToArray());
            _selectedExpansions = ParseExpansionCodes(EnabledExpansionCodes);
            State = CreateState();
        }

        public void Reset()
        {
            State = CreateState();
            PlayerOne.WantsRematch = false;
            PlayerTwo.WantsRematch = false;
        }

        private HiveGameState CreateState() => new(new PlayingBoard(_selectedExpansions));

        private static BugType[] ParseExpansionCodes(string codes)
        {
            var expansions = new List<BugType>();
            if (codes.Contains('L'))
                expansions.Add(BugType.LadyBug);
            if (codes.Contains('P'))
                expansions.Add(BugType.PillBug);
            if (codes.Contains('M'))
                expansions.Add(BugType.Mosquito);
            return expansions.ToArray();
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

        public BugTeam GetTeam(ConnectedHiveClient client)
        {
            if (ReferenceEquals(client, PlayerOne))
                return BugTeam.Light;
            if (ReferenceEquals(client, PlayerTwo))
                return BugTeam.Dark;
            throw new InvalidOperationException("Client is not a member of this game.");
        }
    }
}