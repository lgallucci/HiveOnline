using HiveLib.GameAssets;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveOnline;
internal class KeyboardHelper
{
    private static KeyboardState _currentKeyState;
    private static KeyboardState _previousKeyState;

    public static bool IsKeyPressed(Keys key, bool oneShot)
    {
        if (!oneShot) return _currentKeyState.IsKeyDown(key);
        return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
    }

    internal static void HandleRunningKeyboard(PlayingBoard board)
    {
        _currentKeyState = Keyboard.GetState();

        if (IsKeyPressed(Keys.Enter, true))
        {
            if (!string.IsNullOrWhiteSpace(board.ChatWindow.TypingText))
            {
                board.ChatWindow.ChatMessages.Push(new ChatMessage
                {
                    Message = board.ChatWindow.TypingText,
                    PlayerName = board.UserName,
                    PlayerTeam = 1
                });
                board.ChatWindow.TypingText = string.Empty;
                board.ChatWindow.IsTyping = false;
            }
        }
        else if (IsKeyPressed(Keys.Back, true) && board.ChatWindow.TypingText.Length > 0)
        {
            board.ChatWindow.TypingText = board.ChatWindow.TypingText.Substring(0, board.ChatWindow.TypingText.Length - 1);
        }
        else if (IsKeyPressed(Keys.Escape, false))
        {
            board.ChatWindow.TypingText = string.Empty;
            board.ChatWindow.IsTyping = false;
        }
        else
        {
            string input = KeyboardHelper.TryConvertKeyboardInput(_currentKeyState, _previousKeyState);

            if (!string.IsNullOrEmpty(input))
                board.ChatWindow.TypingText += input;
        }

        _previousKeyState = _currentKeyState;
    }

    /// <summary>
    /// Tries to convert keyboard input to characters and prevents repeatedly returning the 
    /// same character if a key was pressed last frame, but not yet unpressed this frame.
    /// </summary>
    /// <param name="keyboard">The current KeyboardState</param>
    /// <param name="oldKeyboard">The KeyboardState of the previous frame</param>
    /// <param name="key">When this method returns, contains the correct character if conversion succeeded.
    /// Else contains the null, (000), character.</param>
    /// <returns>True if conversion was successful</returns>
    public static string TryConvertKeyboardInput(KeyboardState keyboard, KeyboardState oldKeyboard)
    {
        Keys[] keys = keyboard.GetPressedKeys();
        bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

        string inputBuilder = string.Empty;


        for (int i = 0; i < keys.Length; i++)
        {
            if (keys.Length > 0 && !oldKeyboard.IsKeyDown(keys[i]))
            {
                switch (keys[i])
                {
                    //Alphabet keys
                    case Keys.A: if (shift) { inputBuilder += 'A'; } else { inputBuilder += 'a'; } break;
                    case Keys.B: if (shift) { inputBuilder += 'B'; } else { inputBuilder += 'b'; } break;
                    case Keys.C: if (shift) { inputBuilder += 'C'; } else { inputBuilder += 'c'; } break;
                    case Keys.D: if (shift) { inputBuilder += 'D'; } else { inputBuilder += 'd'; } break;
                    case Keys.E: if (shift) { inputBuilder += 'E'; } else { inputBuilder += 'e'; } break;
                    case Keys.F: if (shift) { inputBuilder += 'F'; } else { inputBuilder += 'f'; } break;
                    case Keys.G: if (shift) { inputBuilder += 'G'; } else { inputBuilder += 'g'; } break;
                    case Keys.H: if (shift) { inputBuilder += 'H'; } else { inputBuilder += 'h'; } break;
                    case Keys.I: if (shift) { inputBuilder += 'I'; } else { inputBuilder += 'i'; } break;
                    case Keys.J: if (shift) { inputBuilder += 'J'; } else { inputBuilder += 'j'; } break;
                    case Keys.K: if (shift) { inputBuilder += 'K'; } else { inputBuilder += 'k'; } break;
                    case Keys.L: if (shift) { inputBuilder += 'L'; } else { inputBuilder += 'l'; } break;
                    case Keys.M: if (shift) { inputBuilder += 'M'; } else { inputBuilder += 'm'; } break;
                    case Keys.N: if (shift) { inputBuilder += 'N'; } else { inputBuilder += 'n'; } break;
                    case Keys.O: if (shift) { inputBuilder += 'O'; } else { inputBuilder += 'o'; } break;
                    case Keys.P: if (shift) { inputBuilder += 'P'; } else { inputBuilder += 'p'; } break;
                    case Keys.Q: if (shift) { inputBuilder += 'Q'; } else { inputBuilder += 'q'; } break;
                    case Keys.R: if (shift) { inputBuilder += 'R'; } else { inputBuilder += 'r'; } break;
                    case Keys.S: if (shift) { inputBuilder += 'S'; } else { inputBuilder += 's'; } break;
                    case Keys.T: if (shift) { inputBuilder += 'T'; } else { inputBuilder += 't'; } break;
                    case Keys.U: if (shift) { inputBuilder += 'U'; } else { inputBuilder += 'u'; } break;
                    case Keys.V: if (shift) { inputBuilder += 'V'; } else { inputBuilder += 'v'; } break;
                    case Keys.W: if (shift) { inputBuilder += 'W'; } else { inputBuilder += 'w'; } break;
                    case Keys.X: if (shift) { inputBuilder += 'X'; } else { inputBuilder += 'x'; } break;
                    case Keys.Y: if (shift) { inputBuilder += 'Y'; } else { inputBuilder += 'y'; } break;
                    case Keys.Z: if (shift) { inputBuilder += 'Z'; } else { inputBuilder += 'z'; } break;

                    //Decimal keys
                    case Keys.D0: if (shift) { inputBuilder += ')'; } else { inputBuilder += '0'; } break;
                    case Keys.D1: if (shift) { inputBuilder += '!'; } else { inputBuilder += '1'; } break;
                    case Keys.D2: if (shift) { inputBuilder += '@'; } else { inputBuilder += '2'; } break;
                    case Keys.D3: if (shift) { inputBuilder += '#'; } else { inputBuilder += '3'; } break;
                    case Keys.D4: if (shift) { inputBuilder += '$'; } else { inputBuilder += '4'; } break;
                    case Keys.D5: if (shift) { inputBuilder += '%'; } else { inputBuilder += '5'; } break;
                    case Keys.D6: if (shift) { inputBuilder += '^'; } else { inputBuilder += '6'; } break;
                    case Keys.D7: if (shift) { inputBuilder += '&'; } else { inputBuilder += '7'; } break;
                    case Keys.D8: if (shift) { inputBuilder += '*'; } else { inputBuilder += '8'; } break;
                    case Keys.D9: if (shift) { inputBuilder += '('; } else { inputBuilder += '9'; } break;

                    //Decimal numpad keys
                    case Keys.NumPad0: inputBuilder += '0'; break;
                    case Keys.NumPad1: inputBuilder += '1'; break;
                    case Keys.NumPad2: inputBuilder += '2'; break;
                    case Keys.NumPad3: inputBuilder += '3'; break;
                    case Keys.NumPad4: inputBuilder += '4'; break;
                    case Keys.NumPad5: inputBuilder += '5'; break;
                    case Keys.NumPad6: inputBuilder += '6'; break;
                    case Keys.NumPad7: inputBuilder += '7'; break;
                    case Keys.NumPad8: inputBuilder += '8'; break;
                    case Keys.NumPad9: inputBuilder += '9'; break;

                    //Special keys
                    case Keys.OemTilde: if (shift) { inputBuilder += '~'; } else { inputBuilder += '`'; } break;
                    case Keys.OemSemicolon: if (shift) { inputBuilder += ':'; } else { inputBuilder += ';'; } break;
                    case Keys.OemQuotes: if (shift) { inputBuilder += '"'; } else { inputBuilder += '\''; } break;
                    case Keys.OemQuestion: if (shift) { inputBuilder += '?'; } else { inputBuilder += '/'; } break;
                    case Keys.OemPlus: if (shift) { inputBuilder += '+'; } else { inputBuilder += '='; } break;
                    case Keys.OemPipe: if (shift) { inputBuilder += '|'; } else { inputBuilder += '\\'; } break;
                    case Keys.OemPeriod: if (shift) { inputBuilder += '>'; } else { inputBuilder += '.'; } break;
                    case Keys.OemOpenBrackets: if (shift) { inputBuilder += '{'; } else { inputBuilder += '['; } break;
                    case Keys.OemCloseBrackets: if (shift) { inputBuilder += '}'; } else { inputBuilder += ']'; } break;
                    case Keys.OemMinus: if (shift) { inputBuilder += '_'; } else { inputBuilder += '-'; } break;
                    case Keys.OemComma: if (shift) { inputBuilder += '<'; } else { inputBuilder += ','; } break;
                    case Keys.Space: inputBuilder += ' '; break;
                }
            }
        }

        return inputBuilder;
    }

}
