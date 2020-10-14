using HiveContracts;
using HiveOnline.GameAssets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Linq;
using System.Text;

namespace HiveGraphics
{
    public class GraphicsEngine
    {
        public HiveContracts.Point ScreenSize { get; set; }

        public GraphicsEngine(ContentManager content, GraphicsDevice graphics)
        {
            Art.Load(content, graphics);
        }

        public bool Draw(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond, IBoard board)
        {
            DrawBoard(graphics, spriteBatch, board);
            DrawPiles(graphics, spriteBatch, board);
            DrawChat(graphics, spriteBatch, board);
            DrawText(graphics, spriteBatch, board);
            DrawFps(graphics, spriteBatch, framesPerSecond);
            return false;
        }

        private void DrawFps(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, int framesPerSecond)
        {
            spriteBatch.DrawString(Art.ChatFont, $"FPS: {framesPerSecond}", new Vector2(1, 1), Color.Red);
        }

        private void DrawText(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            var userNameSize = Art.NameFont.MeasureString(board.UserName);
            var opponentNameSize = Art.NameFont.MeasureString(board.OpponentName ?? "TestOpponent");
            spriteBatch.DrawString(Art.NameFont, board.UserName ?? "TestUser", new Vector2(5, /*window height - font height*/(float)ScreenSize.y - userNameSize.Y), Color.Blue);
            spriteBatch.DrawString(Art.NameFont, board.OpponentName ?? "TestOpponent", new Vector2(/*window width - font width*/(float)ScreenSize.x - opponentNameSize.X - 5, 5), Color.Red);
        }

        private void DrawChat(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            //DRAW BOX
            int textBoxHeight = 250, textBoxWidth = 500;
            spriteBatch.Draw(Art.Pixel, new Rectangle((int)ScreenSize.x - textBoxWidth, (int)ScreenSize.y - textBoxHeight, textBoxWidth - 5, textBoxHeight - 5), new Color(26, 50, 38));

            //DRAW LINE
            var typingText = board.TypingText + "_";
            typingText = typingText == "_" ? "TestText" : typingText;
            var typingTextSize = Art.ChatFont.MeasureString(typingText);
            var lineHeight = (int)ScreenSize.y - typingTextSize.Y - 8;
            spriteBatch.DrawLine(new Vector2((int)ScreenSize.x - textBoxWidth, lineHeight),
                                 new Vector2((int)ScreenSize.x - 5, lineHeight),
                                 Color.MintCream, 2f);

            //DRAW TYPING TEXT
            spriteBatch.DrawString(Art.ChatFont, typingText, new Vector2((int)ScreenSize.x - textBoxWidth + 2, (int)ScreenSize.y - typingTextSize.Y - 5), Color.MintCream);

            int textHeight = 0;
            //DRAW SERVER TEXT
            foreach (var text in board.ChatMessages)
            {
                Vector2 chatTextSize = Art.ChatFont.MeasureString(text);

                if (lineHeight - textHeight - 5 > (int)ScreenSize.y - textBoxHeight)
                {
                    textHeight = DrawChatText(spriteBatch, Art.ChatFont, board, text, textHeight, textBoxWidth, lineHeight);
                }
            }
        }

        public int DrawChatText(SpriteBatch spriteBatch, SpriteFont font, IBoard board, string text, int textHeight, int textBoxWidth, float lineHeight)
        {
            var textPieces = text.Split(":");
            var nameText = textPieces[0];
            var nameOffset = font.MeasureString(nameText).X;

            var chatText = $":{string.Join(":", textPieces.Skip(1))}";
            var wrappedText = WrapText(Art.ChatFont, chatText, textBoxWidth - nameOffset - 5);

            textHeight += (int)font.MeasureString(wrappedText).Y;

            Color nameColor = nameText == board.UserName ? Color.Blue : Color.Red;

            spriteBatch.DrawString(Art.ChatFont, nameText, new Vector2((int)ScreenSize.x - textBoxWidth + 2, lineHeight - textHeight - 5), nameColor);

            spriteBatch.DrawString(Art.ChatFont, wrappedText, new Vector2((int)ScreenSize.x - textBoxWidth + 2 + font.MeasureString(nameText).X, lineHeight - textHeight - 5), Color.MintCream);

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

        public static bool SetupUser()
        {
            throw new NotImplementedException();
        }

        private void DrawBoard(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            //Draw Grid
            foreach (var tile in board.Tiles)
            {
                switch (tile.Type)
                {
                    case BugType.Blank:
                        DrawBlankTile(spriteBatch, board, tile);
                        break;
                    default:
                        DrawBugTile(spriteBatch, board, tile);
                        break;
                }
            }
        }

        private void DrawBugTile(SpriteBatch spriteBatch, IBoard board, ITile tile)
        {
            var tileSize = board.Layout.size;
            var texture = GetTextureFromTile(tile);
            spriteBatch.Draw(
                texture,
                board.Layout.HexToPixel(tile.Location).ToVector2(),
                new Rectangle(0, 0, texture.Width, texture.Height),
                Color.White,
                0,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Vector2((float)tileSize.x / (texture.Width - 100), (float)tileSize.y / (texture.Height - 100)), //Scale
                SpriteEffects.None, 0f);
        }

        private Texture2D GetTextureFromTile(ITile tile)
        {
            switch (tile.Team)
            {
                case BugTeam.Dark:
                    switch (tile.Type)
                    {
                        case BugType.Beetle:
                            return Art.DarkBeetle;
                        case BugType.Grasshopper:
                            return Art.DarkGrassHopper;
                        case BugType.LadyBug:
                            return Art.DarkLadyBug;
                        case BugType.Mosquito:
                            return Art.DarkMosquito;
                        case BugType.PillBug:
                            return Art.DarkPillBug;
                        case BugType.QueenBee:
                            return Art.DarkQueenBee;
                        case BugType.SoldierAnt:
                            return Art.DarkSoldierAnt;
                        case BugType.Spider:
                            return Art.DarkSpider;
                        default:
                            break;
                    }
                    break;
                case BugTeam.Light:
                    switch (tile.Type)
                    {
                        case BugType.Beetle:
                            return Art.LightBeetle;
                        case BugType.Grasshopper:
                            return Art.LightGrassHopper;
                        case BugType.LadyBug:
                            return Art.LightLadyBug;
                        case BugType.Mosquito:
                            return Art.LightMosquito;
                        case BugType.PillBug:
                            return Art.LightPillBug;
                        case BugType.QueenBee:
                            return Art.LightQueenBee;
                        case BugType.SoldierAnt:
                            return Art.LightSoldierAnt;
                        case BugType.Spider:
                            return Art.LightSpider;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            return Art.Pixel;
        }

        private void DrawBlankTile(SpriteBatch spriteBatch, IBoard board, ITile hex)
        {
            HiveContracts.Point firstPoint = new HiveContracts.Point(0, 0);
            Nullable<HiveContracts.Point> previousPoint = null;
            foreach (var corner in board.Layout.PolygonCorners(hex.Location))
            {
                if (previousPoint.HasValue)
                {
                    spriteBatch.DrawLine(corner.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
                }
                else
                {
                    firstPoint = corner;
                }

                previousPoint = corner;
            }

            spriteBatch.DrawLine(firstPoint.ToVector2(), previousPoint.Value.ToVector2(), Color.Red, 3f);
            var vector2 = board.Layout.HexToPixel(hex.Location);
            spriteBatch.DrawString(Art.ChatFont, $"{hex.Location.q}, {hex.Location.r}, {hex.Location.s}",
                new Vector2((float)vector2.x - 20, (float)vector2.y - 7), Color.Red);
        }

        private void DrawPiles(GraphicsDeviceManager graphics, SpriteBatch spriteBatch, IBoard board)
        {
            //Draw Your Pile            
            //Draw Opponents Pile
        }
    }
}
