using HiveContracts;
using HiveGraphics.GameAssetsDraw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HiveLib.GameAssets
{
    public class ChatBox
    {
        private ChatBoxGraphics Graphics { get; set; } = new ChatBoxGraphics();

        public bool IsOpen { get; set; }

        public bool IsTyping { get; set; }
        public string TypingText { get; set; }
        public Stack<ChatMessage> ChatMessages { get; set; }

        public ChatBox()
        {
            ChatMessages = new Stack<ChatMessage>(new List<ChatMessage>
            {
                new ChatMessage { PlayerName = "TestUser", PlayerTeam = 1, Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor"},
                new ChatMessage { PlayerName = "TestOpponent", PlayerTeam = 2, Message = "incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam," },
                new ChatMessage { PlayerName = "TestUser", PlayerTeam = 1, Message = "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." },
                new ChatMessage { PlayerName = "TestUser", PlayerTeam = 1, Message = "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat " },
                new ChatMessage { PlayerName = "TestOpponent", PlayerTeam = 2, Message = "nulla pariatur. Excepteur sint occaecat cupidatat non proident, " },
                new ChatMessage { PlayerName = "TestUser", PlayerTeam =1, Message = "sunt in culpa qui officia deserunt mollit anim id est laborum." }
            });
            TypingText = "";
        }

        public void Draw()
        {
            Graphics.Draw(TypingText, IsTyping, ChatMessages.Select(cm => (cm.PlayerName, cm.PlayerTeam, cm.Message)));
        }

        public void ChangeScreenSize(HexPoint screenSize)
        {
            Graphics.ChangeScreenSize(screenSize);
        }

        public bool Intersects(int x, int y)
        {
            if (Graphics.Location.Contains(x, y)) return true;
            return false;
        }
    }

}
