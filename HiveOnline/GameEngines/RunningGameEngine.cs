using HiveClient;
using HiveContracts;
using HiveLib.Bugs;
using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;

namespace HiveOnline
{
    public enum PlayingState
    {
        YourTurn = 0,
        OpponentsTurn = 1,
        Won = 2,
        Lost = 3,
    }

    class RunningGameEngine : GameEngine
    {
        private PlayingBoard _board;
        private int _screenWidth = 0;
        private int _screenHeight = 0;
        private PlayingState _playingState;
        private bool _testing = false;
        private SimpleAI _ai;
        private readonly bool _useAI;
        private readonly HiveGameClient _networkClient;
        private readonly Queue<string> _pendingNetworkMessages = new Queue<string>();
        private BugTeam _playerTeam;

        // Game rule tracking
        private int _playerTurnCount = 0;  // Light player's turn count
        private int _opponentTurnCount = 0; // Dark player's turn count
        private bool _playerQueenPlaced = false;
        private bool _opponentQueenPlaced = false;

        public RunningGameEngine(int screenWidth, int screenHeight, BugTeam team, HiveGameClient networkClient = null, bool useAi = true)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _networkClient = networkClient;
            _useAI = useAi;
            _playerTeam = team;
            _board = new PlayingBoard(screenWidth, screenHeight);
            _ai = new SimpleAI(_board);

            if (_networkClient != null)
            {
                _networkClient.MessageReceived += message =>
                {
                    if (!string.IsNullOrWhiteSpace(message))
                        _pendingNetworkMessages.Enqueue(message);
                };
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
            _board.SetScreenSize(screenWidth, screenHeight);
        }

        public override void Draw(HiveGraphics.GraphicsEngine _graphicsEngine)
        {
            _board.Draw();
        }

        private Hex leftMost, rightMost, topMost, bottomMost;
        public override void Update(ref GameState _gameState)
        {
            ProcessNetworkMessages();

            if (_board.ChatWindow.IsTyping)
            {
                KeyboardHelper.HandleRunningKeyboard(_board);
            }

            HexPoint originHexPoint = _board.Layout.origin;
            HexPoint originalSize = _board.Layout.size;

            var mouseState = Mouse.GetState();
            var fractionalHex = _board.Layout.PixelToHex(new HexPoint(mouseState.X, mouseState.Y));
            var clickedHex = fractionalHex.HexRound();

            // Allow camera movement after the game ends, but block all piece interaction.
            if (HandleCompletedGame(mouseState, ref originHexPoint, ref originalSize))
            {
                _board.Layout = new Layout(Layout.flat, originalSize, originHexPoint);
                return;
            }

            // Handle AI opponent turn
            if (HandleAiTurn())
            {
                return; // Don't process player input during AI turn
            }

            //Enter Layout
            if (_board.UserPile.Intersects(mouseState.X, mouseState.Y))
            {
                if (MouseLeftClickedOnce(mouseState.LeftButton) && _playingState == PlayingState.YourTurn)
                {
                    _board.SelectedTile = null;
                    _board.ClearAvailableTiles();

                    var bug = _board.UserPile.GetIntersectBug(mouseState.X, mouseState.Y);

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
                        _board.AddAvailableHexes(_board.UserPile.CalculateAvailable(_board));
                    }
                }
            }
            //Enter Chat Box
            else if (_board.ChatWindow.Intersects(mouseState.X, mouseState.Y))
            {
                if (MouseLeftClickedOnce(mouseState.LeftButton) && !_board.ChatWindow.IsTyping)
                {
                    _board.ChatWindow.TypingText = "";
                    _board.ChatWindow.IsTyping = true;
                }
            }
            else if (!draggingCamera && _playingState == PlayingState.YourTurn && _board.AvailableTiles.ContainsKey(clickedHex.GetHashCode()))
            {
                if (MouseLeftClickedOnce(mouseState.LeftButton))
                {
                    //Get Selected Tile (from board or pile?)
                    var selectedTile = _board.SelectedTile;

                    if (selectedTile != null)
                    {
                        bool isNewPlacement = !_board.ContainsTile(selectedTile);

                        if (isNewPlacement && MustPlayQueen(selectedTile))
                        {
                            // Can only place Queen
                            return;
                        }

                        // Prevent movement until Queen is placed
                        if (!isNewPlacement && !CanMoveAnyPiece(BugTeam.Light))
                        {
                            return; // Can't move until Queen is placed
                        }

                        //TODO: Figure out a better way to handle pile selections
                        Hex fromHex = selectedTile.Location;
                        if (_board.ContainsTile(selectedTile))
                            _board.RemoveTile(_board.Tiles[selectedTile.GetHashCode()]);
                        else
                            selectedTile = _board.UserPile.GetTile(selectedTile.Type);

                        var destinationHex = _board.AvailableTiles[clickedHex.GetHashCode()];
                        selectedTile.Location = destinationHex;

                        //Add tile of selected type to available spot
                        _board.AddTile(selectedTile);
                        
                        // Track Queen placement
                        OnPiecePlaced(selectedTile);

                        _board.SelectedTile = null;
                        _board.ClearAvailableTiles();

                        if (_networkClient != null && _networkClient.IsConnected)
                        {
                            _ = _networkClient.SendMove(selectedTile.Type.ToString(), fromHex.q, fromHex.r, fromHex.s, destinationHex.q, destinationHex.r, destinationHex.s, selectedTile.Team.ToString());
                        }

                        // Switch to opponent's turn after successful move
                        SwitchTurns();
                        
                        // Check if anyone won
                        CheckWinCondition();

                        //reset drag area
                        topMost = default(Hex); bottomMost = default(Hex); leftMost = default(Hex); rightMost = default(Hex);
                        foreach (var hex in _board.Tiles.Select(t => t.Value))
                        {
                            if (hex.Location.s + (-1 * hex.Location.r) > topMost.s + (-1 * topMost.r))
                                topMost = hex.Location;
                            if (hex.Location.s + (-1 * hex.Location.r) < bottomMost.s + (-1 * bottomMost.r))
                                bottomMost = hex.Location;
                            if (hex.Location.q < leftMost.q)
                                leftMost = hex.Location;
                            if (hex.Location.q > rightMost.q)
                                rightMost = hex.Location;
                        }
                    }
                }
            }
            //Enter Hex on Board
            else if (!draggingCamera && _playingState == PlayingState.YourTurn && _board.ContainsTile(clickedHex))
            {
                if (MouseLeftClickedOnce(mouseState.LeftButton))
                {
                    var tile = _board.Tiles[clickedHex.GetHashCode()];

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
                        if (!CanMoveAnyPiece(BugTeam.Light))
                        {
                            return; // Can't move until Queen is placed
                        }

                        //Calculate and set Available 
                        _board.ClearAvailableTiles();
                        var available = tile.CalculateAvailable(_board);
                        _board.AddAvailableHexes(available);
                    }
                    else
                    {
                        _board.SelectedTile = null;
                        _board.ClearAvailableTiles();
                    }
                }
                else if (MouseRightClickedOnce(mouseState.RightButton))
                {
                    var tile = _board.Tiles[clickedHex.GetHashCode()];
                    tile.IsInspecting = !tile.IsInspecting;
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

            _board.Layout = new Layout(Layout.flat, originalSize, originHexPoint);
        }

        private void ProcessNetworkMessages()
        {
            while (_networkClient != null && _networkClient.IsConnected && _networkClient.TryDequeueMessage(out var message))
                ProcessIncomingNetworkMessage(message);
        }

        private bool HandleCompletedGame(MouseState mouseState, ref HexPoint originHexPoint, ref HexPoint originalSize)
        {
            if (_playingState != PlayingState.Won && _playingState != PlayingState.Lost)
                return false;

            HandleCameraInput(mouseState, ref originHexPoint);

            if (mouseState.ScrollWheelValue != lastScrollWheelValue)
                originalSize = HandleCameraResize(_board, mouseState);

            return true;
        }

        private void HandleCameraInput(MouseState mouseState, ref HexPoint originHexPoint)
        {
            if (mouseState.LeftButton == ButtonState.Pressed && mouseState.X > 0 && mouseState.Y > 0 &&
                mouseState.X < _board.Graphics.Width && mouseState.Y < _board.Graphics.Height)
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

        private bool HandleAiTurn()
        {
            if (!_useAI || _playingState != PlayingState.OpponentsTurn)
                return false;

            if (_ai.MakeMove(_opponentTurnCount, _opponentQueenPlaced))
            {
                if (!_opponentQueenPlaced && _board.Tiles.Values.Any(t => t.Team == BugTeam.Dark && t.Type == BugType.QueenBee))
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

                var team = Enum.TryParse<BugTeam>(teamValue, true, out var parsedTeam) ? parsedTeam : BugTeam.Blank;
                var type = Enum.TryParse<BugType>(typeValue, true, out var parsedType) ? parsedType : BugType.QueenBee;

                var fromHex = new Hex(int.Parse(fromCoords[0]), int.Parse(fromCoords[1]), int.Parse(fromCoords[2]));
                var toHex = new Hex(int.Parse(toCoords[0]), int.Parse(toCoords[1]), int.Parse(toCoords[2]));

                var remoteTile = GetTileForRemoteMove(team, type, fromHex, toHex);
                if (remoteTile == null)
                    return;

                remoteTile.Location = toHex;

                if (_board.ContainsTile(toHex))
                {
                    _board.RemoveTile(_board.Tiles[toHex.GetHashCode()]);
                }

                _board.AddTile(remoteTile);

                if (team == BugTeam.Dark)
                {
                    _opponentTurnCount++;
                    _playingState = PlayingState.YourTurn;
                    UpdateTurnDisplay();
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

            if (team == BugTeam.Light)
            {
                return _board.UserPile.GetTile(bugType);
            }

            if (team == BugTeam.Dark)
            {
                return _board.OpponentPile.GetTile(bugType);
            }

            return null;
        }

        private void SwitchTurns()
        {
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

            UpdateTurnDisplay();
        }

        private bool CheckWinCondition()
        {
            // Check if opponent's Queen is surrounded
            var opponentQueens = _board.Tiles.Values.Where(t => t.Team == BugTeam.Dark && t.Type == BugType.QueenBee).ToList();
            if (opponentQueens.Count > 0 && IsQueenSurrounded(opponentQueens[0]))
            {
                _playingState = PlayingState.Won;
                _board.CurrentTurn = "You won! Opponent's Queen is surrounded!";
                return true;
            }

            // Check if player's Queen is surrounded
            var playerQueens = _board.Tiles.Values.Where(t => t.Team == BugTeam.Light && t.Type == BugType.QueenBee).ToList();
            if (playerQueens.Count > 0 && IsQueenSurrounded(playerQueens[0]))
            {
                _playingState = PlayingState.Lost;
                _board.CurrentTurn = "You lost! Your Queen is surrounded!";
                return true;
            }

            return false;
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

        private void UpdateTurnDisplay()
        {
            string turnText = _playingState == PlayingState.YourTurn ? "Your Turn" : "Opponent's Turn";
            
            // Add warnings if Queen must be played
            if (_playingState == PlayingState.YourTurn && !_playerQueenPlaced && _playerTurnCount >= 3)
                turnText += " (Queen required!)";
            else if (_playingState == PlayingState.OpponentsTurn && !_opponentQueenPlaced && _opponentTurnCount >= 3)
                turnText += " (Queen required!)";

            _board.CurrentTurn = turnText;
        }

        private bool CanMoveAnyPiece(BugTeam team)
        {
            // Can't move pieces until your Queen is placed
            if (team == BugTeam.Light)
                return _playerQueenPlaced;
            else
                return _opponentQueenPlaced;
        }

        private void OnPiecePlaced(ITile placedPiece)
        {
            // Track if this is a Queen
            if (placedPiece.Type == BugType.QueenBee)
            {
                if (placedPiece.Team == BugTeam.Light)
                    _playerQueenPlaced = true;
                else
                    _opponentQueenPlaced = true;
            }
        }

        private ButtonState _leftButtonPreviousState;
        private bool MouseLeftClickedOnce(ButtonState leftButton)
        {
            var clicked = _leftButtonPreviousState == ButtonState.Released && leftButton == ButtonState.Pressed;
            _leftButtonPreviousState = leftButton;
            return clicked;
        }
        private ButtonState _rightButtonPreviousState;
        private bool MouseRightClickedOnce(ButtonState rightButton)
        {
            var clicked = _rightButtonPreviousState == ButtonState.Released && rightButton == ButtonState.Pressed;
            _rightButtonPreviousState = rightButton;
            return clicked;
        }

        private HexPoint HandleCameraResize(PlayingBoard board, MouseState mouseState)
        {
            var addedSize = (mouseState.ScrollWheelValue - lastScrollWheelValue) / 20;
            lastScrollWheelValue = mouseState.ScrollWheelValue;

            var newSize = addedSize + board.Layout.size.X;
            if (newSize > 60)
                newSize = 60;
            if (newSize < 30)
                newSize = 30;

            var newLayout = new Layout(board.Layout.orientation, new HexPoint(newSize, newSize), board.Layout.origin);

            var topMostPixel = newLayout.HexToPixel(topMost);
            var bottomMostPixel = newLayout.HexToPixel(bottomMost);
            var leftMostPixel = newLayout.HexToPixel(leftMost);
            var rightMostPixel = newLayout.HexToPixel(rightMost);

            double dragbuffer = board.Layout.size.X;

            if (topMostPixel.Y > _screenHeight - dragbuffer || bottomMostPixel.Y < dragbuffer ||
                rightMostPixel.X < dragbuffer || leftMostPixel.X > _screenWidth - dragbuffer)
                return board.Layout.size;

            return new HexPoint(newSize, newSize);
        }

        bool draggingCamera = false;
        HexPoint lastDragPosition = default(HexPoint);
        int lastScrollWheelValue = default(int);
        private HexPoint HandleCameraDrag(PlayingBoard board, MouseState mouseState)
        {
            //System.Diagnostics.Debug.WriteLine($"MouseDrag: {lastDragPosition.X}, {lastDragPosition.Y}");
            var mouseDragChange = new HexPoint(-1, -1) * (lastDragPosition - new HexPoint(mouseState.X, mouseState.Y));

            var newLayout = new Layout(board.Layout.orientation, board.Layout.size, board.Layout.origin + mouseDragChange);

            var topMostPixel = newLayout.HexToPixel(topMost);
            var bottomMostPixel = newLayout.HexToPixel(bottomMost);
            var leftMostPixel = newLayout.HexToPixel(leftMost);
            var rightMostPixel = newLayout.HexToPixel(rightMost);

            double dragbuffer = board.Layout.size.X;

            if (topMostPixel.Y > _screenHeight - dragbuffer || bottomMostPixel.Y < dragbuffer ||
                rightMostPixel.X < dragbuffer || leftMostPixel.X > _screenWidth - dragbuffer)
                return board.Layout.origin;

            return board.Layout.origin + mouseDragChange;
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
