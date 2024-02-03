using HiveContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace HiveGraphics.GameAssetsDraw;
public class ChatBoxGraphics : DrawableObject
{
    private const int _textBoxHeight = 250;
    private const int _textBoxWidth = 500;
    private const int _textBuffer = 2;

    public void ChangeScreenSize(HexPoint ScreenSize)
    {
        Location = new Rectangle((int)ScreenSize.X - _textBoxWidth, (int)ScreenSize.Y - _textBoxHeight, _textBoxWidth - 5, _textBoxHeight - 5);
    }

    public void Draw(string typingText, bool isTyping, IEnumerable<(string, int, string)> messages)
    {
        //DRAW BOX
        GraphicsEngine.SpriteBatch.Draw(Art.Pixel, Location, new Color(26, 50, 38));

        //DRAW LINE
        if (!string.IsNullOrWhiteSpace(typingText) || isTyping)
            typingText = $"> {typingText}_";
        else
            typingText = " ";

        var typingTextSize = Art.ChatFont.MeasureString(typingText);
        var lineHeight = Location.Bottom - typingTextSize.Y - _textBuffer;
        GraphicsEngine.SpriteBatch.DrawLine(Art.Pixel, new Vector2(Location.Left, lineHeight),
                             new Vector2(Location.Right, lineHeight),
                             Color.MintCream, 2f);

        //DRAW TYPING TEXT
        GraphicsEngine.SpriteBatch.DrawString(Art.ChatFont, typingText, new Vector2(Location.Left + _textBuffer, Location.Bottom - typingTextSize.Y - _textBuffer), Color.MintCream);

        int textHeight = 0;
        //DRAW SERVER TEXT
        foreach (var text in messages)
        {
            if (lineHeight - textHeight - 5 > Location.Top)
            {
                textHeight = DrawChatText(Art.ChatFont, text.Item1, text.Item2, text.Item3, textHeight, lineHeight);
            }
        }
    }

    public int DrawChatText(SpriteFont font, string playerName, int playerTeam, string message, int textHeight, float lineHeight)
    {
        var nameOffset = font.MeasureString(playerName).X;

        var chatText = $": {message}";
        var wrappedText = WrapText(font, chatText, _textBoxWidth - nameOffset - 5);

        textHeight += (int)font.MeasureString(wrappedText).Y;

        GraphicsEngine.SpriteBatch.DrawString(font, playerName, new Vector2(Location.Left + _textBuffer, lineHeight - textHeight - 5), GetPlayerColor(playerTeam));

        GraphicsEngine.SpriteBatch.DrawString(font, wrappedText, new Vector2(Location.Left + _textBuffer + font.MeasureString(playerName).X, lineHeight - textHeight - 5), Color.MintCream);

        return textHeight;
    }

    private Color GetPlayerColor(int playerTeam)
    {
        if (playerTeam == 1)
            return Color.CornflowerBlue;
        return Color.Red;
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
