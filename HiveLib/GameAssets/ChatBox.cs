using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Text;

namespace HiveLib.GameAssets
{
    public class ChatBox
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle Location { get; set; }

        public bool IsOpen { get; set; }

        public string TypingText { get; set; }
        public Stack<ChatMessage> ChatMessages { get; set; }

        private const int _textBoxHeight = 250;
        private const int _textBoxWidth = 500;
        private const int _textBuffer = 2;

        public ChatBox()
        {
            ChatMessages = new Stack<ChatMessage>(new List<ChatMessage>
            {
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "Hey there big spender!"},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You fucking suck."},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "Cheese is smelly and gross"},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "Cheese is smelly and gross asdf asdf asdfas dasdfas dfasdf asdf asdf asdf asdf asdf asdf" +
                " asdf asdf asdf df asdf asdf asdf asdf asdf df asdf asdf asdf asdf asdf "},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestUser", PlayerColor = Color.CornflowerBlue, Message = "You're dumb."},
                new ChatMessage { PlayerName = "TestOpponent", PlayerColor = Color.Red, Message = "You're dumb."}
            });
            TypingText = "I'm typin. yo.";
        }

        public void ChangeScreenSize(HiveContracts.Point ScreenSize)
        {
            Location = new Rectangle((int)ScreenSize.X - _textBoxWidth, (int)ScreenSize.Y - _textBoxHeight, _textBoxWidth - 5, _textBoxHeight - 5);
        }

        public void Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch)
        {
            //DRAW BOX
            spriteBatch.Draw(Art.Pixel, Location, new Color(26, 50, 38));

            //DRAW LINE
            var typingText = TypingText + "_";
            typingText = typingText == "_" ? "TestText" : typingText;
            var typingTextSize = Art.ChatFont.MeasureString(typingText);
            var lineHeight = Location.Bottom - typingTextSize.Y - _textBuffer;
            spriteBatch.DrawLine(Art.Pixel, new Vector2(Location.Left, lineHeight),
                                 new Vector2(Location.Right, lineHeight),
                                 Color.MintCream, 2f);

            //DRAW TYPING TEXT
            spriteBatch.DrawString(Art.ChatFont, typingText, new Vector2(Location.Left + _textBuffer, Location.Bottom - typingTextSize.Y - _textBuffer), Color.MintCream);

            int textHeight = 0;
            //DRAW SERVER TEXT
            foreach (var text in ChatMessages)
            {
                if (lineHeight - textHeight - 5 > Location.Top)
                {
                    textHeight = DrawChatText(spriteBatch, Art.ChatFont, text, textHeight, lineHeight);
                }
            }
        }

        public int DrawChatText(SpriteBatch spriteBatch, SpriteFont font, ChatMessage chatMessage, int textHeight, float lineHeight)
        {
            var nameText = chatMessage.PlayerName;
            var nameOffset = font.MeasureString(nameText).X;

            var chatText = $": {chatMessage.Message}";
            var wrappedText = WrapText(font, chatText, _textBoxWidth - nameOffset - 5);

            textHeight += (int)font.MeasureString(wrappedText).Y;

            spriteBatch.DrawString(font, nameText, new Vector2(Location.Left + _textBuffer, lineHeight - textHeight - 5), chatMessage.PlayerColor);

            spriteBatch.DrawString(font, wrappedText, new Vector2(Location.Left + _textBuffer + font.MeasureString(nameText).X, lineHeight - textHeight - 5), Color.MintCream);

            return textHeight;
        }

        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth)
        {
            string[] words = text.Split(' ');
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(" ").X;

            foreach (string word in words)
            {
                Vector2 size = spriteFont.MeasureString(word);

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(word + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }
    }

}
