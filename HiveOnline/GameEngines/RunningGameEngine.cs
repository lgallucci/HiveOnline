using HiveClient;
using HiveContracts;
using HiveGraphics;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using HiveLib.Rules;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HiveOnline
{
    public enum PlayingState
    {
        YourTurn = 0,
        OpponentsTurn = 1,
        Won = 2,
        Lost = 3,
        Draw = 4,
    }

    class RunningGameEngine : GameEngine
    {
        private PlayingBoard _board;
        private int _screenWidth = 0;
        private int _screenHeight = 0;
        private PlayingState _playingState;
        private bool _testing = false;
        private SimpleAI _ai;
        private readonly BoardViewState _view;
        private readonly BoardRenderer _boardRenderer;
        private readonly HiveGameState _rules;
        private readonly bool _useAI;
        private readonly HiveGameClient _networkClient;
        private readonly ConcurrentQueue<string> _pendingNetworkMessages = new ConcurrentQueue<string>();
        private BugTeam _playerTeam;
        private bool _opponentLeft;
        private bool _rematchRequestedByMe;
        private bool _rematchRequestedByOpponent;
        private readonly Action<string> _messageHandler;

        public bool RestartRequested { get; private set; }
        private BugTeam _opponentTeam;
        private bool IsNetworkGame => _networkClient != null;

        // Hot-seat game: both players share the screen and the active player's pile is shown at the bottom.
        private bool IsLocalTwoPlayer => !_useAI && _networkClient == null;

        // Game rule tracking
        private int _playerTurnCount = 0;  // Light player's turn count
        private int _opponentTurnCount = 0; // Dark player's turn count
        private bool _playerQueenPlaced = false;
        private bool _opponentQueenPlaced = false;

        public RunningGameEngine(int screenWidth, int screenHeight, BugTeam team, HiveGameClient networkClient = null, bool useAi = true, string playerName = null, string opponentName = null, IReadOnlyCollection<BugType> selectedExpansions = null)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _networkClient = networkClient;
            _useAI = useAi;
            _playerTeam = team;
            _opponentTeam = team == BugTeam.Light ? BugTeam.Dark : BugTeam.Light;
            _board = new PlayingBoard(selectedExpansions);
            if (team == BugTeam.Dark)
            {
                // The local player's pile is always the one drawn at the bottom of the screen.
                (_board.UserPile, _board.OpponentPile) = (_board.OpponentPile, _board.UserPile);
            }
            _rules = new HiveGameState(_board, BugTeam.Light);
            _view = new BoardViewState(screenWidth, screenHeight);
            _view.ChatVisible = _networkClient != null;
            _boardRenderer = new BoardRenderer();
            _boardRenderer.Resize(_view, _board);
            _ai = new SimpleAI(_board, _opponentTeam);

            if (_networkClient == null && !string.IsNullOrWhiteSpace(playerName))
            {
                _board.UserName = playerName;
                if (!string.IsNullOrWhiteSpace(opponentName))
                    _board.OpponentName = opponentName;
            }

            if (_networkClient != null)
            {
                _board.ChatWindow.ChatMessages.Clear();
                _board.UserName = string.IsNullOrWhiteSpace(playerName) ? $"You ({team})" : playerName;
                _board.OpponentName = string.IsNullOrWhiteSpace(opponentName) ? $"Opponent ({_opponentTeam})" : opponentName;
                _messageHandler = message =>
                {
                    if (!string.IsNullOrWhiteSpace(message))
                        _pendingNetworkMessages.Enqueue(message);
                };
                _networkClient.MessageReceived += _messageHandler;
            }

            if (team == BugTeam.Light)
                _playingState = PlayingState.YourTurn;
            else
                _playingState = PlayingState.OpponentsTurn;

            UpdateTurnDisplay();

            if (_testing)
                //foreach (var testTile in TestBoard.GetTestBoard())
                foreach (var testTile in TestBoard.GetSpiderAntTestBoard())
                {
                    _board.AddTile(testTile);
                }
            else
                _board.AddAvailableHexes(new List<Hex> { new Hex(0, 0, 0) });
        }

        public override void SetScreenSize(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _view.Resize(screenWidth, screenHeight);
            _boardRenderer.Resize(_view, _board);
        }

        public void Detach()
        {
            if (_networkClient != null && _messageHandler != null)
                _networkClient.MessageReceived -= _messageHandler;
        }

        // Directly above the player's name, which sits just above their pile (bottom left).
        private Microsoft.Xna.Framework.Rectangle QuitButton =>
            new Microsoft.Xna.Framework.Rectangle(5, _view.GetPileBounds(_board.UserPile, false).Top - 40 - 6 - 40, 100, 40);

        private Microsoft.Xna.Framework.Rectangle QuitPanel => new Microsoft.Xna.Framework.Rectangle(_view.ScreenSize.X / 2 - 320, _view.ScreenSize.Y / 2 - 90, 640, 190);

        private bool _confirmingQuit;

        private Microsoft.Xna.Framework.Rectangle QuitYesButton => new Microsoft.Xna.Framework.Rectangle(_view.ScreenSize.X / 2 - 160, _view.ScreenSize.Y / 2, 150, 50);

        private Microsoft.Xna.Framework.Rectangle QuitNoButton => new Microsoft.Xna.Framework.Rectangle(_view.ScreenSize.X / 2 + 10, _view.ScreenSize.Y / 2, 150, 50);

        private bool GameOver => _playingState == PlayingState.Won || _playingState == PlayingState.Lost || _playingState == PlayingState.Draw;

        // In multiplayer a rematch is impossible once the opponent has left.
        private bool CanRematch => GameOver && !_opponentLeft;

        private Microsoft.Xna.Framework.Rectangle RematchButton => new Microsoft.Xna.Framework.Rectangle(_view.ScreenSize.X / 2 - 150, 60, 300, 50);

        private string RematchLabel => _rematchRequestedByMe ? "Waiting for opponent..."
            : _rematchRequestedByOpponent ? "Opponent wants a rematch!" : "Rematch";

        public override void Draw(HiveGraphics.GraphicsEngine _graphicsEngine)
        {
            _graphicsEngine.BloomEnabled = !_confirmingQuit;
            _boardRenderer.Draw(_board, _view, _graphicsEngine.Context);

            var quitMouse = Mouse.GetState();
            _graphicsEngine.DrawButton(QuitButton, "Quit", QuitButton.Contains(quitMouse.X, quitMouse.Y));

            if (_confirmingQuit)
            {
                _graphicsEngine.DrawPanel(QuitPanel);
                _graphicsEngine.DrawStringTop(IsNetworkGame && !GameOver ? "Leave the game? Your opponent will be notified." : "Leave the game?",
                    Microsoft.Xna.Framework.Color.White, _view.ScreenSize.Y / 2 - 60);
                _graphicsEngine.DrawButton(QuitYesButton, "Yes, quit", QuitYesButton.Contains(quitMouse.X, quitMouse.Y));
                _graphicsEngine.DrawButton(QuitNoButton, "Keep playing", QuitNoButton.Contains(quitMouse.X, quitMouse.Y));
            }

            if (CanRematch)
            {
                var mouse = Mouse.GetState();
                _graphicsEngine.DrawButton(RematchButton, RematchLabel, !_rematchRequestedByMe && RematchButton.Contains(mouse.X, mouse.Y));
            }
        }

        private void HandleRematchClick()
        {
            if (_rematchRequestedByMe)
                return;

            if (IsNetworkGame)
            {
                _rematchRequestedByMe = true;
                _ = _networkClient.SendMessage("REMATCH");
            }
            else
            {
                RestartRequested = true;
            }
        }

        private Hex leftMost, rightMost, topMost, bottomMost;
        public override void Update(GameTime gameTime, ref GameState _gameState)
        {
            KeyboardHelper.Update();
            var mouseState = Mouse.GetState();
            var leftClicked = _previousMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed;
            var rightClicked = _previousMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed;
            _previousMouseState = mouseState;

            ProcessNetworkMessages();

            if (_confirmingQuit)
            {
                if (leftClicked && QuitYesButton.Contains(mouseState.X, mouseState.Y))
                {
                    Detach();
                    _gameState = GameState.OpeningScreen;
                }
                else if (leftClicked && QuitNoButton.Contains(mouseState.X, mouseState.Y))
                {
                    _confirmingQuit = false;
                }

                return;
            }

            if (leftClicked && QuitButton.Contains(mouseState.X, mouseState.Y))
            {
                _confirmingQuit = true;
                return;
            }

            if (_board.ChatWindow.IsTyping)
            {
                ApplyChatInput(KeyboardHelper.ReadChatInput());
            }

            HexPoint originHexPoint = _view.Layout.origin;
            HexPoint originalSize = _view.Layout.size;

            var fractionalHex = _view.Layout.PixelToHex(new HexPoint(mouseState.X, mouseState.Y));
            var clickedHex = fractionalHex.HexRound();

            // Expanding a stack is view-only, so it works on any turn and after the game ends.
            if (rightClicked && _board.ContainsTile(clickedHex) &&
                !_view.ContainsChat(mouseState.X, mouseState.Y) &&
                !_view.ContainsPile(_board.UserPile, mouseState.X, mouseState.Y, false))
            {
                var inspected = _board.Tiles[clickedHex.GetHashCode()];
                inspected.IsInspecting = !inspected.IsInspecting;
            }

            // Selection never survives the end of your turn or the end of the game.
            if (_playingState != PlayingState.YourTurn && _board.SelectedTile != null)
                DeselectAll();

            // Allow camera movement after the game ends, but block all piece interaction.
            if (HandleCompletedGame(mouseState, ref originHexPoint, ref originalSize))
            {
                // Chat stays usable after the game is over.
                if (leftClicked && _view.ContainsChat(mouseState.X, mouseState.Y))
                    HandleChatClick(mouseState);
                else if (leftClicked && CanRematch && RematchButton.Contains(mouseState.X, mouseState.Y))
                    HandleRematchClick();

                _view.SetLayout(originalSize, originHexPoint);
                return;
            }

            // Handle AI opponent turn
            if (HandleAiTurn(gameTime))
            {
                return; // Don't process player input during AI turn
            }

            //Enter Layout
            if (_view.ContainsPile(_board.UserPile, mouseState.X, mouseState.Y, false))
            {
                if (leftClicked && _playingState == PlayingState.YourTurn)
                {
                    DeselectAll();

                    var bug = _view.GetPileTile(_board.UserPile, mouseState.X, mouseState.Y, false);

                    if (bug == null)
                        return;

                    // Check if Queen must be placed this turn
                    if (MustPlayQueen(bug))
                    {
                        // Can only place Queen
                        return;
                    }

                    if (bug.Team == _playerTeam)
                    {
                        _board.SelectedTile = bug;
                        _board.AddAvailableHexes(_board.UserPile.CalculateAvailable(_board, _playerTeam));
                    }
                }
            }
            //Enter Chat Box
            else if (_view.ContainsChat(mouseState.X, mouseState.Y))
            {
                if (leftClicked)
                    HandleChatClick(mouseState);
            }
            else if (!draggingCamera && _playingState == PlayingState.YourTurn &&
                (_board.AvailableTiles.ContainsKey(clickedHex.GetHashCode()) ||
                 _board.ChoiceTargets.ContainsKey(clickedHex.GetHashCode())))
            {
                if (leftClicked)
                {
                    //Get Selected Tile (from board or pile?)
                    var selectedTile = _board.SelectedTile;

                    if (selectedTile is IChoosesAbility chooser && _board.ChoiceTargets.ContainsKey(clickedHex.GetHashCode()))
                    {
                        // Extra click: the clicked piece narrows down which moves are available.
                        chooser.TryChoose(_board, clickedHex);
                        ShowMoveHighlights(selectedTile);
                    }
                    else if (selectedTile != null && _board.AvailableTiles.ContainsKey(clickedHex.GetHashCode()))
                    {
                        var fromHex = selectedTile.Location;
                        var destinationHex = _board.AvailableTiles[clickedHex.GetHashCode()];
                        var copiedBug = (selectedTile as IChoosesAbility)?.Choice;
                        var warpTarget = (selectedTile as IChoosesAbility)?.Target;
                        if (TryApplyMove(selectedTile, fromHex, destinationHex, _playerTeam, out var appliedTile, copiedBug, warpTarget))
                        {
                            _board.SelectedTile = null;
                            if (_networkClient != null && _networkClient.IsConnected)
                                _ = _networkClient.SendMove(appliedTile.Type.ToString(), fromHex.q, fromHex.r, fromHex.s, destinationHex.q, destinationHex.r, destinationHex.s, appliedTile.Team.ToString(), copiedBug?.ToString(),
                                    warpTarget is { } warped ? $"{warped.q},{warped.r},{warped.s}" : null);

                            if (_useAI)
                            {
                                SwitchTurns();
                                CheckWinCondition();
                            }
                            else if (IsNetworkGame)
                            {
                                _playerTurnCount++;
                                _playingState = PlayingState.OpponentsTurn;
                                UpdateTurnDisplay();
                                CheckNetworkResult();
                            }
                            else
                            {
                                EndLocalTurn();
                            }

                            topMost = default(Hex); bottomMost = default(Hex); leftMost = default(Hex); rightMost = default(Hex);
                            foreach (var hex in _board.Tiles.Select(t => t.Value))
                            {
                                if (hex.Location.s + (-1 * hex.Location.r) > topMost.s + (-1 * topMost.r)) topMost = hex.Location;
                                if (hex.Location.s + (-1 * hex.Location.r) < bottomMost.s + (-1 * bottomMost.r)) bottomMost = hex.Location;
                                if (hex.Location.q < leftMost.q) leftMost = hex.Location;
                                if (hex.Location.q > rightMost.q) rightMost = hex.Location;
                            }
                        }
                    }
                }
            }
            //Enter Hex on Board
            else if (!draggingCamera && _playingState == PlayingState.YourTurn && _board.ContainsTile(clickedHex))
            {
                if (leftClicked)
                {
                    var tile = _board.Tiles[clickedHex.GetHashCode()];
                    ClearAbilityPreview();

                    if (tile.Team == _playerTeam && tile.CanMove(_board) && (_board.SelectedTile == null || tile.GetHashCode() != _board.SelectedTile.GetHashCode()))
                    {
                        //Set Selected
                        _board.SelectedTile = tile;

                        // Check if Queen must be placed this turn
                        if (MustPlayQueen(tile))
                        {
                            // Can only place Queen
                            return;
                        }

                        // Prevent movement until Queen is placed
                        if (!CanMoveAnyPiece(_playerTeam))
                        {
                            return; // Can't move until Queen is placed
                        }

                        //Calculate and set Available 
                        (tile as IChoosesAbility)?.ClearChoice();
                        ShowMoveHighlights(tile);
                    }
                    else
                    {
                        _board.SelectedTile = null;
                        _board.ClearAvailableTiles();
                    }
                }
            }
            else//Drag
            {
                HandleCameraInput(mouseState, ref originHexPoint);
            }

            if (mouseState.ScrollWheelValue != lastScrollWheelValue)
            {
                originalSize = HandleCameraResize(_board, mouseState);
            }

            _view.SetLayout(originalSize, originHexPoint);
        }

        public override void UpdateInactive(GameTime gameTime, ref GameState _gameState)
        {
            ProcessNetworkMessages();
        }

        private bool _connectionLost;

        private void ProcessNetworkMessages()
        {
            if (IsNetworkGame && !_connectionLost && !GameOver && !_networkClient.IsConnected)
            {
                // The server ends the game when we drop, so there is nothing to resume.
                _connectionLost = true;
                _opponentLeft = true;
                _playingState = PlayingState.Lost;
                DeselectAll();
                _board.CurrentTurn = "Connection to the server was lost. The game has ended.";
            }

            while (_networkClient != null && _networkClient.IsConnected && _pendingNetworkMessages.TryDequeue(out var message))
                ProcessIncomingNetworkMessage(message);

            SkipTurnIfStuck();
        }

        private bool _turnChecked;
        private int _aiPassStreak;

        private bool HasLegalMove(BugTeam team)
        {
            if (!_useAI)
                return _rules.HasLegalMove(team);

            var isPlayer = team == _playerTeam;
            return MoveAvailability.HasLegalMove(_board, PileFor(team), team,
                isPlayer ? _playerTurnCount : _opponentTurnCount, isPlayer ? _playerQueenPlaced : _opponentQueenPlaced);
        }

        // Skips your turn when you have nothing to place and nothing that can move.
        private void SkipTurnIfStuck()
        {
            if (_playingState != PlayingState.YourTurn || GameOver)
            {
                _turnChecked = false;
                return;
            }

            if (_turnChecked || _board.Tiles.Count == 0)
                return;
            _turnChecked = true;

            if (HasLegalMove(_playerTeam))
                return;

            DeselectAll();
            if (_useAI)
            {
                if (++_aiPassStreak >= 2)
                {
                    _playingState = PlayingState.Draw;
                    _board.CurrentTurn = "Draw! Neither player can move.";
                    return;
                }
            }
            else
            {
                if (IsNetworkGame)
                    _ = _networkClient.SendMessage("PASS");
                _rules.TryPass(_playerTeam);
                if (_rules.IsFinished)
                {
                    CheckNetworkResult();
                    return;
                }

                if (IsLocalTwoPlayer)
                {
                    SwapSides();
                    _board.CurrentTurn = $"{_board.UserName}'s Turn - {_board.OpponentName} had no legal moves and was skipped";
                    return;
                }
            }

            _playingState = PlayingState.OpponentsTurn;
            UpdateTurnDisplay();
            _board.CurrentTurn = "Opponent's Turn - you had no legal moves and were skipped";
        }

        private void HandleChatClick(MouseState mouseState)
        {
            if (_view.ChatCollapsed || _view.ChatToggleBounds.Contains(mouseState.X, mouseState.Y))
            {
                ToggleChat();
            }
            else if (!_board.ChatWindow.IsTyping)
            {
                _board.ChatWindow.TypingText = "";
                _board.ChatWindow.IsTyping = true;
            }
        }

        private void ToggleChat()
        {
            var chat = _board.ChatWindow;
            chat.IsCollapsed = !chat.IsCollapsed;
            _view.ChatCollapsed = chat.IsCollapsed;
            chat.HasUnread = false;
            if (chat.IsCollapsed)
            {
                chat.IsTyping = false;
                chat.TypingText = string.Empty;
            }
        }

        private void ApplyChatInput(ChatInputResult input)
        {
            if (input.Submit)
            {
                if (!string.IsNullOrWhiteSpace(_board.ChatWindow.TypingText))
                {
                    if (_networkClient != null && _networkClient.IsConnected)
                        _ = _networkClient.SendChat(_board.ChatWindow.TypingText);

                    _board.ChatWindow.ChatMessages.Push(new ChatMessage
                    {
                        Message = _board.ChatWindow.TypingText,
                        PlayerName = _board.UserName,
                        PlayerTeam = 1
                    });
                }

                _board.ChatWindow.TypingText = string.Empty;
                _board.ChatWindow.IsTyping = false;
            }
            else if (input.Backspace && _board.ChatWindow.TypingText.Length > 0)
            {
                _board.ChatWindow.TypingText = _board.ChatWindow.TypingText.Substring(0, _board.ChatWindow.TypingText.Length - 1);
            }
            else if (input.Cancel)
            {
                _board.ChatWindow.TypingText = string.Empty;
                _board.ChatWindow.IsTyping = false;
            }
            else if (!string.IsNullOrEmpty(input.Text))
            {
                _board.ChatWindow.TypingText += input.Text;
            }
        }

        private bool HandleCompletedGame(MouseState mouseState, ref HexPoint originHexPoint, ref HexPoint originalSize)
        {
            if (!GameOver)
                return false;

            if (!draggingCamera && _view.ContainsChat(mouseState.X, mouseState.Y))
                return true;

            HandleCameraInput(mouseState, ref originHexPoint);

            if (mouseState.ScrollWheelValue != lastScrollWheelValue)
                originalSize = HandleCameraResize(_board, mouseState);

            return true;
        }

        private void HandleCameraInput(MouseState mouseState, ref HexPoint originHexPoint)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && mouseState.X > 0 && mouseState.Y > 0 &&
                mouseState.X < _view.ScreenSize.X && mouseState.Y < _view.ScreenSize.Y)
            {
                Mouse.SetCursor(MouseCursor.Crosshair);
                draggingCamera = true;
                if (lastDragPosition == default(HexPoint))
                    lastDragPosition = new HexPoint(mouseState.X, mouseState.Y);

                originHexPoint = HandleCameraDrag(_board, mouseState);
                lastDragPosition = new HexPoint(mouseState.X, mouseState.Y);
            }
            else if (mouseState.LeftButton == ButtonState.Released)
            {
                Mouse.SetCursor(MouseCursor.Arrow);
                lastDragPosition = default(HexPoint);
                draggingCamera = false;
            }
        }

        private bool HandleAiTurn(GameTime gameTime)
        {
            if (!_useAI || _playingState != PlayingState.OpponentsTurn)
                return false;

            if (_board.Tiles.Count > 0 && !HasLegalMove(_opponentTeam))
            {
                if (++_aiPassStreak >= 2)
                {
                    _playingState = PlayingState.Draw;
                    _board.CurrentTurn = "Draw! Neither player can move.";
                }
                else
                {
                    _playingState = PlayingState.YourTurn;
                    UpdateTurnDisplay();
                    _board.CurrentTurn = "Your Turn - the AI had no moves and was skipped";
                }
                return true;
            }

            if (_ai.MakeMove(gameTime.ElapsedGameTime.TotalSeconds, _opponentTurnCount, _opponentQueenPlaced))
            {
                if (!_opponentQueenPlaced && _board.Tiles.Values.Any(t => t.Team == _opponentTeam && t.Type == BugType.QueenBee))
                    _opponentQueenPlaced = true;

                _board.SelectedTile = null;
                _board.ClearAvailableTiles();
                SwitchTurns();
                CheckWinCondition();
            }

            return true;
        }

        private bool MustPlayQueen(ITile tile)
        {
            if (!_useAI)
                return _rules.GetTurnCount(_playerTeam) >= 3 && !_rules.IsQueenPlaced(_playerTeam) && tile.Type != BugType.QueenBee;

            // Check if Queen must be placed this turn
            if (_playerTurnCount >= 3 && !_playerQueenPlaced && 
                tile.Type != BugType.QueenBee)
            {
                // Can only place Queen
                return true;
            }
            return false;
        }

        private void ProcessIncomingNetworkMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message.Equals("REMATCH_START", StringComparison.OrdinalIgnoreCase))
            {
                RestartRequested = true;
                return;
            }

            if (message.Equals("REMATCH_REQUESTED", StringComparison.OrdinalIgnoreCase))
            {
                _rematchRequestedByOpponent = true;
                return;
            }

            if (message.Equals("OPPONENT_DISCONNECTED", StringComparison.OrdinalIgnoreCase))
            {
                _opponentLeft = true;
                if (!GameOver)
                {
                    _playingState = PlayingState.Won;
                    _board.CurrentTurn = "Opponent disconnected. You win!";
                }
                return;
            }

            if (message.Equals("PASS", StringComparison.OrdinalIgnoreCase))
            {
                if (_rules.TryPass(_opponentTeam).IsValid)
                {
                    if (_rules.IsFinished)
                    {
                        CheckNetworkResult();
                    }
                    else
                    {
                        _playingState = PlayingState.YourTurn;
                        UpdateTurnDisplay();
                        _board.CurrentTurn = "Your Turn - opponent had no moves and was skipped";
                    }
                }
                return;
            }

            if (message.StartsWith("CHAT|", StringComparison.OrdinalIgnoreCase))
            {
                var text = message.Substring(5);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    _board.ChatWindow.ChatMessages.Push(new ChatMessage
                    {
                        Message = text,
                        PlayerName = _board.OpponentName,
                        PlayerTeam = 2
                    });
                    _board.ChatWindow.HasUnread = _board.ChatWindow.IsCollapsed;
                }
                return;
            }

            if (message.StartsWith("MOVE_REJECTED", StringComparison.OrdinalIgnoreCase))
            {
                _board.CurrentTurn = message;
                return;
            }

            if (!message.StartsWith("MOVE|", StringComparison.OrdinalIgnoreCase))
                return;

            var parts = message.Split('|');
            if (parts.Length < 5)
                return;

            try
            {
                var teamValue = parts[1];
                var typeValue = parts[2];
                var fromCoords = parts[3].Split(',');
                var toCoords = parts[4].Split(',');

                if (fromCoords.Length != 3 || toCoords.Length != 3)
                    return;

                if (!Enum.TryParse<BugTeam>(teamValue, true, out var team) ||
                    !Enum.TryParse<BugType>(typeValue, true, out var type) ||
                    team == BugTeam.Blank || type == BugType.Blank)
                    return;

                var fromHex = new Hex(int.Parse(fromCoords[0]), int.Parse(fromCoords[1]), int.Parse(fromCoords[2]));
                var toHex = new Hex(int.Parse(toCoords[0]), int.Parse(toCoords[1]), int.Parse(toCoords[2]));

                BugType? copiedBug = null;
                if (parts.Length > 5 && Enum.TryParse<BugType>(parts[5], true, out var parsedCopy))
                    copiedBug = parsedCopy;

                var remoteTile = GetTileForRemoteMove(team, type, fromHex, toHex);
                if (remoteTile == null)
                    return;

                Hex? warpTarget = null;
                if (parts.Length > 6)
                {
                    var warpCoords = parts[6].Split(',');
                    warpTarget = new Hex(int.Parse(warpCoords[0]), int.Parse(warpCoords[1]), int.Parse(warpCoords[2]));
                }

                if (!TryApplyMove(remoteTile, fromHex, toHex, team, out _, copiedBug, warpTarget))
                    return;

                if (team == _opponentTeam)
                {
                    _opponentTurnCount++;
                    _playingState = PlayingState.YourTurn;
                    UpdateTurnDisplay();
                    if (IsNetworkGame)
                        CheckNetworkResult();
                    else
                        CheckWinCondition();
                }
            }
            catch
            {
                // Ignore malformed network move payloads instead of crashing the game.
            }
        }

        private ITile GetTileForRemoteMove(BugTeam team, BugType bugType, Hex fromHex, Hex toHex)
        {
            if (_board.ContainsTile(fromHex))
            {
                var existing = _board.Tiles[fromHex.GetHashCode()];
                if (existing.Team == team && existing.Type == bugType)
                    return existing;
            }

            if (team == BugTeam.Light || team == BugTeam.Dark)
                return PileFor(team).Peek(bugType);

            return null;
        }

        private Pile PileFor(BugTeam team) => _board.UserPile.Team == team ? _board.UserPile : _board.OpponentPile;

        private void CheckNetworkResult()
        {
            if (!_rules.IsFinished)
                return;

            if (_rules.WinningTeam == null)
            {
                _playingState = PlayingState.Draw;
                _board.CurrentTurn = _rules.Board.Tiles.Values.Any(t => t.Type == BugType.QueenBee) && BothQueensSurrounded()
                    ? "Draw! Both Queens are surrounded!"
                    : "Draw! Neither player can move.";
            }
            else if (_rules.WinningTeam == _playerTeam)
            {
                _playingState = PlayingState.Won;
                _board.CurrentTurn = "You won! Opponent's Queen is surrounded!";
            }
            else
            {
                _playingState = PlayingState.Lost;
                _board.CurrentTurn = "You lost! Your Queen is surrounded!";
            }
        }

        private bool TryApplyMove(ITile requestedTile, Hex from, Hex destination,
            BugTeam team, out ITile appliedTile, BugType? copiedBug = null, Hex? warpTarget = null)
        {
            if (!_useAI)
            {
                var result = _rules.TryApplyMove(new HiveMove(team, requestedTile.Type, from, destination, copiedBug, warpTarget));
                appliedTile = result.IsValid ? _board.Tiles[(warpTarget.HasValue ? from : destination).GetHashCode()] : null;
                if (appliedTile != null)
                    OnPiecePlaced(appliedTile);
                return result.IsValid;
            }

            appliedTile = null;
            if (requestedTile == null || requestedTile.Team != team)
                return false;

            var isPlacement = !_board.ContainsTile(requestedTile);

            if (isPlacement)
            {
                var pile = PileFor(team);
                if (MustPlayQueen(requestedTile) || !pile.CalculateAvailable(_board, team).Any(hex => hex == destination))
                    return false;

                appliedTile = pile.GetTile(requestedTile.Type);
            }
            else
            {
                if (!_board.Tiles.TryGetValue(from.GetHashCode(), out var boardTile) || boardTile != requestedTile)
                    return false;

                var chooser = requestedTile as IChoosesAbility;
                var warping = chooser is not null && warpTarget.HasValue;
                if (chooser is not null)
                {
                    chooser.Choice = copiedBug;
                    chooser.Target = warpTarget;
                }

                var allowed = chooser is not null
                    ? chooser.IsSelectionValid(_board) && (warping || requestedTile.CanMove(_board)) &&
                      chooser.GetDestinations(_board).Any(hex => hex == destination)
                    : requestedTile.CanMove(_board) && requestedTile.CanMoveTo(_board, destination);
                if (!allowed)
                {
                    chooser?.ClearChoice();
                    return false;
                }

                _board.ClearStunned();
                if (warping)
                {
                    PillbugWarp.Apply(_board, warpTarget.Value, destination);
                    chooser.Target = null;
                    appliedTile = requestedTile;
                    _board.SelectedTile = null;
                    _board.ClearAvailableTiles();
                    return true;
                }

                _board.RemoveTile(boardTile);
                appliedTile = requestedTile;
            }

            appliedTile.Location = destination;
            _board.AddTile(appliedTile);
            OnPiecePlaced(appliedTile);
            _board.SelectedTile = null;
            _board.ClearAvailableTiles();
            return true;
        }

        private void ClearAbilityChoices(BugTeam team)
        {
            foreach (var chooser in _board.Tiles.Values.Where(t => t.Team == team).OfType<IChoosesAbility>())
                chooser.ClearChoice();
        }

        private void SwitchTurns()
        {
            _aiPassStreak = 0;
            if (_playingState == PlayingState.YourTurn)
            {
                _playerTurnCount++;
                _playingState = PlayingState.OpponentsTurn;
            }
            else if (_playingState == PlayingState.OpponentsTurn)
            {
                _opponentTurnCount++;
                _playingState = PlayingState.YourTurn;
            }

            ClearAbilityChoices(_playingState == PlayingState.YourTurn ? _playerTeam : _opponentTeam);
            UpdateTurnDisplay();
        }

        private bool CheckWinCondition()
        {
            var opponentSurrounded = _board.Tiles.Values.Any(t => t.Team == _opponentTeam && t.Type == BugType.QueenBee && IsQueenSurrounded(t));
            var playerSurrounded = _board.Tiles.Values.Any(t => t.Team == _playerTeam && t.Type == BugType.QueenBee && IsQueenSurrounded(t));

            if (opponentSurrounded && playerSurrounded)
            {
                _playingState = PlayingState.Draw;
                _board.CurrentTurn = "Draw! Both Queens are surrounded!";
                return true;
            }

            if (opponentSurrounded)
            {
                _playingState = PlayingState.Won;
                _board.CurrentTurn = "You won! Opponent's Queen is surrounded!";
                return true;
            }

            if (playerSurrounded)
            {
                _playingState = PlayingState.Lost;
                _board.CurrentTurn = "You lost! Your Queen is surrounded!";
                return true;
            }

            return false;
        }

        private bool BothQueensSurrounded()
        {
            var queens = _board.Tiles.Values.Where(t => t.Type == BugType.QueenBee).ToList();
            return queens.Count == 2 && queens.All(IsQueenSurrounded);
        }

        private bool IsQueenSurrounded(ITile queen)
        {
            // Check if all 6 neighbors contain any pieces (friendly or enemy)
            for (int i = 0; i < 6; i++)
            {
                var neighbor = queen.Location.Neighbor(i);
                if (!_board.ContainsTile(neighbor))
                    return false; // Found an empty space, not surrounded
            }
            return true; // All 6 neighbors contain pieces
        }

        private void EndLocalTurn()
        {
            if (_rules.IsFinished)
            {
                if (_rules.WinningTeam == null)
                {
                    _playingState = PlayingState.Draw;
                    _board.CurrentTurn = "Draw! Both Queens are surrounded!";
                }
                else
                {
                    _playingState = PlayingState.Won;
                    var winner = _rules.WinningTeam == _playerTeam ? _board.UserName : _board.OpponentName;
                    _board.CurrentTurn = $"{winner} wins! The Queen is surrounded!";
                }
                DeselectAll();
                return;
            }

            SwapSides();
        }

        // Hands the board to the other local player: their pile moves to the bottom of the screen.
        private void SwapSides()
        {
            (_playerTeam, _opponentTeam) = (_opponentTeam, _playerTeam);
            (_board.UserPile, _board.OpponentPile) = (_board.OpponentPile, _board.UserPile);
            (_board.UserName, _board.OpponentName) = (_board.OpponentName, _board.UserName);
            _playingState = PlayingState.YourTurn;
            _turnChecked = false;
            DeselectAll();
            UpdateTurnDisplay();
        }

        private void UpdateTurnDisplay()
        {
            string turnText = _playingState == PlayingState.YourTurn ? "Your Turn" : "Opponent's Turn";
            if (IsLocalTwoPlayer)
                turnText = $"{_board.UserName}'s Turn";
            
            // Add warnings if Queen must be played
            if (IsLocalTwoPlayer)
            {
                if (_rules.GetTurnCount(_playerTeam) >= 3 && !_rules.IsQueenPlaced(_playerTeam))
                    turnText += " (Queen required!)";
            }
            else if (_playingState == PlayingState.YourTurn && !_playerQueenPlaced && _playerTurnCount >= 3)
                turnText += " (Queen required!)";
            else if (_playingState == PlayingState.OpponentsTurn && !_opponentQueenPlaced && _opponentTurnCount >= 3)
                turnText += " (Queen required!)";

            _board.CurrentTurn = turnText;

            if (_playingState != PlayingState.YourTurn)
                DeselectAll();
        }

        private void ClearAbilityPreview()
        {
            if (_board.SelectedTile is IChoosesAbility chooser)
                chooser.ClearChoice();
        }

        // Orange for pieces still to be chosen, green for hexes the piece can move to now.
        private void ShowMoveHighlights(ITile tile)
        {
            _board.ClearAvailableTiles();
            if (tile is IChoosesAbility chooser)
            {
                _board.AddChoiceTargets(chooser.GetChoiceTargets(_board));
                _board.AddAvailableHexes(chooser.GetDestinations(_board));
            }
            else
                _board.AddAvailableHexes(tile.CalculateAvailable(_board));
        }

        private void DeselectAll()
        {
            ClearAbilityPreview();
            _board.SelectedTile = null;
            _board.ClearAvailableTiles();
        }

        private bool CanMoveAnyPiece(BugTeam team)
        {
            // Can't move pieces until your Queen is placed
            if (!_useAI)
                return _rules.IsQueenPlaced(team);

            if (team == _playerTeam)
                return _playerQueenPlaced;
            else
                return _opponentQueenPlaced;
        }

        private void OnPiecePlaced(ITile placedPiece)
        {
            // Track if this is a Queen
            if (placedPiece.Type == BugType.QueenBee)
            {
                if (placedPiece.Team == _playerTeam)
                    _playerQueenPlaced = true;
                else
                    _opponentQueenPlaced = true;
            }
        }

        private MouseState _previousMouseState;

        private HexPoint HandleCameraResize(PlayingBoard board, MouseState mouseState)
        {
            var addedSize = (mouseState.ScrollWheelValue - lastScrollWheelValue) / 20;
            lastScrollWheelValue = mouseState.ScrollWheelValue;

            var newSize = addedSize + _view.Layout.size.X;
            if (newSize > 60)
                newSize = 60;
            if (newSize < 30)
                newSize = 30;

            var newLayout = new Layout(_view.Layout.orientation, new HexPoint(newSize, newSize), _view.Layout.origin);

            var topMostPixel = newLayout.HexToPixel(topMost);
            var bottomMostPixel = newLayout.HexToPixel(bottomMost);
            var leftMostPixel = newLayout.HexToPixel(leftMost);
            var rightMostPixel = newLayout.HexToPixel(rightMost);

            double dragbuffer = _view.Layout.size.X;

            if (topMostPixel.Y > _screenHeight - dragbuffer || bottomMostPixel.Y < dragbuffer ||
                rightMostPixel.X < dragbuffer || leftMostPixel.X > _screenWidth - dragbuffer)
                return _view.Layout.size;

            return new HexPoint(newSize, newSize);
        }

        bool draggingCamera = false;
        HexPoint lastDragPosition = default(HexPoint);
        int lastScrollWheelValue = default(int);
        private HexPoint HandleCameraDrag(PlayingBoard board, MouseState mouseState)
        {
            //System.Diagnostics.Debug.WriteLine($"MouseDrag: {lastDragPosition.X}, {lastDragPosition.Y}");
            var mouseDragChange = new HexPoint(-1, -1) * (lastDragPosition - new HexPoint(mouseState.X, mouseState.Y));

            var newLayout = new Layout(_view.Layout.orientation, _view.Layout.size, _view.Layout.origin + mouseDragChange);

            var topMostPixel = newLayout.HexToPixel(topMost);
            var bottomMostPixel = newLayout.HexToPixel(bottomMost);
            var leftMostPixel = newLayout.HexToPixel(leftMost);
            var rightMostPixel = newLayout.HexToPixel(rightMost);

            double dragbuffer = _view.Layout.size.X;

            if (topMostPixel.Y > _screenHeight - dragbuffer || bottomMostPixel.Y < dragbuffer ||
                rightMostPixel.X < dragbuffer || leftMostPixel.X > _screenWidth - dragbuffer)
                return _view.Layout.origin;

            return _view.Layout.origin + mouseDragChange;
        }


        //TODO: Check can't place new tile, can't move any tiles
    }

    class TestBoard
    {
        public static List<ITile> GetTestBoard()
        {
            var board = new List<ITile>
            {
                new QueenBee(BugTeam.Light) { Location = new Hex(0, 0, 0) },
                new QueenBee(BugTeam.Dark) { Location = new Hex(1, -1, 0) },
                new Beetle(BugTeam.Light) { Location = new Hex(-1, 1, 0) },
                new Beetle(BugTeam.Dark) { Location = new Hex(1, -2, 1) },
                new Beetle(BugTeam.Light) { Location = new Hex(0, 1, -1) },
                new Beetle(BugTeam.Dark) { Location = new Hex(2, -2, 0) },
                new Grasshopper(BugTeam.Light) { Location = new Hex(-1, 2, -1) },
                new Grasshopper(BugTeam.Dark) { Location = new Hex(2, -3, 1) },
                new Spider(BugTeam.Light) { Location = new Hex(0, 2, -2) },
                new Spider(BugTeam.Dark) { Location = new Hex(3, -3, 0) },
                new Spider(BugTeam.Light) { Location = new Hex(-2, 1, 1) },
                new Spider(BugTeam.Dark) { Location = new Hex(1, -3, 2) },
                //new QueenBee(BugTeam.Light) { Location = new Hex(0, 0, 0), Type = BugType.SoldierAnt },
                //new QueenBee(BugTeam.Dark) { Location = new Hex(0, 0, 0), Type = BugType.SoldierAnt },
            };
            return board;
        }

        public static List<ITile> GetSpiderAntTestBoard()
        {
            var board = new List<ITile>
            {
                new QueenBee(BugTeam.Light) { Location = new Hex(0, -1, 1) },
                new QueenBee(BugTeam.Dark) { Location = new Hex(-1, 0, 1) },
                new Beetle(BugTeam.Light) { Location = new Hex(-2, 1, 1) },
                new Beetle(BugTeam.Dark) { Location = new Hex(-2, 2, 0) },
                new Beetle(BugTeam.Light) { Location = new Hex(-1, 2, -1) },
                new Beetle(BugTeam.Dark) { Location = new Hex(0, 2, -2) },
                new Grasshopper(BugTeam.Light) { Location = new Hex(1, 1, -2) },
                new Grasshopper(BugTeam.Dark) { Location = new Hex(2, 0, -2) },
                new Spider(BugTeam.Light) { Location = new Hex(2, -1, -1) },
                new Spider(BugTeam.Dark) { Location = new Hex(2, -2, 0) },
                //new Spider(BugTeam.Light) { Location = new Hex(3, -3, 0) },
                //new Spider(BugTeam.Dark) { Location = new Hex(1, -3, 2) },
                new SoldierAnt(BugTeam.Light) { Location = new Hex(0, 0, 0) },
                new SoldierAnt(BugTeam.Dark) { Location = new Hex(0, 0, 0) },
                new SoldierAnt(BugTeam.Light) { Location = new Hex(0, 0, 0) },
                new SoldierAnt(BugTeam.Dark) { Location = new Hex(0, 0, 0) },
            };
            return board;
        }
    }
}
