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
        private bool _testing = true;

        public RunningGameEngine(int screenWidth, int screenHeight, BugTeam team)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _board = new PlayingBoard(screenWidth, screenHeight);


            if (team == BugTeam.Light)
                _playingState = PlayingState.YourTurn;
            else
                _playingState = PlayingState.OpponentsTurn;

            if (_testing)
                foreach (var testTile in TestBoard.GetTestBoard())
                {
                    _board.AddTile(testTile);
                }
            else
                _board.AddTile(new QueenBee(BugTeam.Light) { Location = new Hex(0, 0, 0) });
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
            if (_board.ChatWindow.IsTyping)
            {
                KeyboardHelper.HandleRunningKeyboard(_board);
            }

            HexPoint originHexPoint = _board.Layout.origin;
            HexPoint originalSize = _board.Layout.size;

            var mouseState = Mouse.GetState();
            var fractionalHex = _board.Layout.PixelToHex(new HexPoint(mouseState.X, mouseState.Y));
            var clickedHex = fractionalHex.HexRound();

            //Enter Layout
            if (false)
            {
                if (MouseLeftClickedOnce(mouseState.LeftButton))
                {

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
                        if (_board.ContainsTile(selectedTile))
                            _board.RemoveTile(_board.Tiles[selectedTile.GetHashCode()]);
                        else
                            selectedTile = _board.UserPile.GetTile(selectedTile.Type);

                        selectedTile.Location = _board.AvailableTiles[clickedHex.GetHashCode()];

                        //Add tile of selected type to available spot
                        _board.AddTile(selectedTile);

                        _board.SelectedTile = null;
                        _board.ClearAvailableTiles();

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

                    if (tile.CanMove(_board) && (_board.SelectedTile == null || tile.GetHashCode() != _board.SelectedTile.GetHashCode()))
                    {
                        //Set Selected
                        _board.SelectedTile = tile;

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
            }
            else//Drag
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (mouseState.X > 0 && mouseState.Y > 0 && mouseState.X < _board.Graphics.Width && mouseState.Y < _board.Graphics.Height)
                    {
                        Mouse.SetCursor(MouseCursor.Crosshair);
                        draggingCamera = true;
                        if (lastDragPosition == default(HexPoint))
                            lastDragPosition = new HexPoint(mouseState.X, mouseState.Y);

                        originHexPoint = HandleCameraDrag(_board, mouseState);

                        lastDragPosition = new HexPoint(mouseState.X, mouseState.Y);
                    }
                }
                else
                {
                    Mouse.SetCursor(MouseCursor.Arrow);
                    lastDragPosition = default(HexPoint);
                    draggingCamera = false;
                }
            }

            if (mouseState.ScrollWheelValue != lastScrollWheelValue)
            {
                originalSize = HandleCameraResize(_board, mouseState);
            }

            _board.Layout = new Layout(Layout.flat, originalSize, originHexPoint);


            //TODO: Update game state
        }

        private ButtonState _leftButtonPreviousState;
        private bool MouseLeftClickedOnce(ButtonState leftButton)
        {
            var clicked = _leftButtonPreviousState == ButtonState.Released && leftButton == ButtonState.Pressed;
            _leftButtonPreviousState = leftButton;
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

            //TODO: Don't allow move if no tiles on screen

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
    }
}
