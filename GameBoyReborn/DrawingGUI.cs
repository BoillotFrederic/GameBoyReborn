// -----------
// Drawing GUI
// -----------

using Raylib_cs;
using System.Numerics;
using System.Text.RegularExpressions;
using Emulator;

namespace GameBoyReborn
{
    public class DrawingGUI
    {
        #region Draw GUI

        private static Game[] GameList = Array.Empty<Game>();
        private static Font MainFont = Raylib.LoadFont(AppDomain.CurrentDomain.BaseDirectory + "Fonts/ErasBoldITC.ttf");
        private static Texture2D CartridgeGB = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/CartridgeGB.png");
        private static readonly int SizeRef = 2400; // 6 * (item cartridge 400) + (scrollbar 20)
        private static int ScreenWidth;
        private static int ScreenHeight;
        private static int NbGame;
        private static int InputMoveRepeatLimit = 0;
        private static bool InputActionMove = false;
        private static bool ThumbnailClicked = false;
        private static bool scrollClicked = false;
        private static int scrollClickedPos = 0;
        private static int scrollClickedLastPosList = 0;

        public static void InitMetroGB()
        {
            // Game list
            GetGameListPath();

            // Texture cartridge
            Raylib.SetTextureFilter(CartridgeGB, TextureFilter.TEXTURE_FILTER_BILINEAR);
            
            // Texture cartridge text
            for (int i = 0; i < NbGame; i++)
            {
                List<TextSet> TitleTest = TextNlWrap(GameList[i].Name, 30.0f * TextResolution, 3.0f, 300.0f * TextResolution);
                TitleTextures[i] = TitleGameToTexture(TitleTest, 30.0f * TextResolution, 3.0f, Color.GRAY);
            }
        }

        private static int GameListTopShift = 100;

        public static void MetroGB()
        {
            // Start draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RAYWHITE);

            // Mouse
            Vector2 mouse = Raylib.GetMousePosition();
            MouseCursor cursor = MouseCursor.MOUSE_CURSOR_DEFAULT;
            ThumbnailClicked = false;

            // Double click
            MouseLeftDoubleClick = false;
            if (Input.MouseLeftClickPressed)
            {
                MouseLeftClickDelay = 10;
                MouseLeftClickPressed++;
            }

            if(MouseLeftClickDelay >= 0)
            MouseLeftClickDelay--;
            else
            MouseLeftClickPressed = 0;

            if(MouseLeftClickPressed >= 2 && MouseLeftClickDelay > 0 && MouseLeftClickLastTarget == MouseLeftClickTarget)
            MouseLeftDoubleClick = true;

            // Move in list
            if (InputMoveRepeatLimit > 0)
            InputMoveRepeatLimit--;

            if(InputMoveRepeatLimit == 0)
            {
                if(Input.AxisLeftPadUp || Input.DPadUp)
                {
                    InputMoveRepeatLimit = 10;
                    int newPos = MouseLeftClickTarget -  6;
                    MouseLeftClickTarget = newPos >= 0 ? newPos : MouseLeftClickTarget;
                    InputActionMove = true;
                }
                if(Input.AxisLeftPadDown || Input.DPadDown)
                {
                    InputMoveRepeatLimit = 10;
                    int newPos = MouseLeftClickTarget +  6;
                    MouseLeftClickTarget = newPos < NbGame ? newPos : MouseLeftClickTarget;
                    InputActionMove = true;
                }
                if(Input.AxisLeftPadLeft || Input.DPadLeft)
                {
                    InputMoveRepeatLimit = 10;
                    MouseLeftClickTarget = MouseLeftClickTarget % 6 == 0 ? MouseLeftClickTarget : MouseLeftClickTarget - 1;
                }
                if(Input.AxisLeftPadRight || Input.DPadRight)
                {
                    InputMoveRepeatLimit = 10;
                    MouseLeftClickTarget = MouseLeftClickTarget % 6 == 5 ? MouseLeftClickTarget : MouseLeftClickTarget + 1;
                }
            }

            // Operating variables
            ScreenWidth = Raylib.GetRenderWidth();
            ScreenHeight = Raylib.GetRenderHeight();
            int cartridgeHeight = ScreenWidth * 300 / SizeRef;
            int yStart = ScreenWidth * 100 / SizeRef;
            int yMargin = ScreenWidth * 65 / SizeRef;
            double nbLine = Math.Ceiling(NbGame / 6.0f);
            int FullThumbnailHeight = cartridgeHeight + yMargin;
            int scrollBarWidth = ScreenWidth * 30 / SizeRef;
            int scrollBarHeight = (int)((float)ScreenHeight / ((nbLine * FullThumbnailHeight) + (yStart * 2)) * ScreenHeight);
            int cartridgeWidth = ScreenWidth * ((scrollBarHeight > ScreenHeight) ? 400 : 395) / SizeRef;
            int gameListPageHeight = (int)(nbLine * FullThumbnailHeight);
            int gameListTop = ScreenWidth * GameListTopShift / SizeRef;
            int gameListTopPrepare = GameListTopShift + (int)(Raylib.GetMouseWheelMove() * 100);
            int gameListBottomLimit = gameListPageHeight - (ScreenHeight - yStart);
            int scrollPos = (int)((float)gameListTop / gameListPageHeight * ScreenHeight) * -1;

            // Draw line selected
            int selectedCartridgeX = MouseLeftClickTarget % 6 * cartridgeWidth;
            int selectedCartridgeY = (int)(gameListTop + (Math.Floor(MouseLeftClickTarget / 6.0f) * FullThumbnailHeight));
            int selectedCartridgeHeight = FullThumbnailHeight + (ScreenWidth * 5 / SizeRef);
            Raylib.DrawRectangle(selectedCartridgeX, selectedCartridgeY, cartridgeWidth, selectedCartridgeHeight, Color.LIGHTGRAY);

            // Draw game list
            CartridgeGB.Width = cartridgeWidth;
            CartridgeGB.Height = cartridgeHeight;

            if(TitleTextures != null && NbGame != 0)
            for(int y = 0; y < nbLine; y++)
            {
                for(int x = 0; x < 6; x++)
                {
                    int index = y * 6 + x;

                    if (index >= NbGame)
                    break;

                    // Where
                    int X = x * cartridgeWidth;
                    int Y = gameListTop + (y * FullThumbnailHeight);

                    // Draw cartridge
                    Raylib.DrawTexture(CartridgeGB, X, Y, Color.WHITE);

                    // Collision area
                    float CartridgeWidthCol = CartridgeGB.Width * 0.55f;
                    float CartridgeHeightCol = CartridgeGB.Height * 0.822f;

                    Rectangle CartridgeRect = new();
                    CartridgeRect.X = X + ((CartridgeGB.Width - CartridgeWidthCol) / 2);
                    CartridgeRect.Y = Y + ((CartridgeGB.Height - CartridgeHeightCol) / 2);
                    CartridgeRect.Width = CartridgeWidthCol;
                    CartridgeRect.Height = CartridgeHeightCol;

                    // Draw title
                    for (int t = 0; t < TitleTextures[index].Length; t++)
                    {
                        TitleTextures[index][t].Texture.Width = ScreenWidth * (int)TitleTextures[index][t].Width / SizeRef;
                        TitleTextures[index][t].Texture.Height = ScreenWidth * (int)TitleTextures[index][t].Height / SizeRef;
                        int centerX = (CartridgeGB.Width - TitleTextures[index][t].Texture.Width) / 2;
                        
                        int shiftY = 0;
                        if(t != 0)
                        for (int s = 1; s < t + 1; s++)
                        shiftY += TitleTextures[index][s].Texture.Height;

                        Raylib.DrawTexture(TitleTextures[index][t].Texture, X + centerX, Y + shiftY + ScreenWidth * 275 / SizeRef, Color.WHITE);

                        // Collision area
                        Rectangle CartridgeTextRect = new();
                        CartridgeTextRect.X = X + centerX;
                        CartridgeTextRect.Y = Y + shiftY + ScreenWidth * 275 / SizeRef;
                        CartridgeTextRect.Width = TitleTextures[index][t].Texture.Width;
                        CartridgeTextRect.Height = TitleTextures[index][t].Texture.Height;

                        // Mouse hover
                        if (Raylib.CheckCollisionPointRec(mouse, CartridgeTextRect))
                        {
                            cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                            if(Input.MouseLeftClickPressed) MouseClickPressed(index);
                            if(Input.MouseLeftClick) ThumbnailClicked = true;
                        }
                    }

                    // Mouse hover
                    if (Raylib.CheckCollisionPointRec(mouse, CartridgeRect))
                    {
                        cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                        if(Input.MouseLeftClickPressed) MouseClickPressed(index);
                        if(Input.MouseLeftClick) ThumbnailClicked = true;
                    }
                }
            }

            // Scrolling game list by click
            Rectangle scrollRect = new();
            scrollRect.X = ScreenWidth - scrollBarWidth;
            scrollRect.Y = scrollPos;
            scrollRect.Width = scrollBarWidth;
            scrollRect.Height = scrollBarHeight;

            if (Raylib.CheckCollisionPointRec(mouse, scrollRect) && Input.MouseLeftClickPressed)
            {
                scrollClicked = true;
                scrollClickedPos = (int)mouse.Y;
                scrollClickedLastPosList = GameListTopShift;
            }

            if (Input.MouseLeftClickUp)
            scrollClicked = false;

            if (scrollClicked)
            {
                int shift = (int)(-1 * (float)((mouse.Y - scrollClickedPos) / ScreenHeight) * gameListPageHeight);
                gameListTopPrepare = ((ScreenWidth * scrollClickedLastPosList / SizeRef) + shift) * SizeRef / ScreenWidth;
            }

            // Scrolling game list by mouse wheel
            if (scrollBarHeight < ScreenHeight)
            {
                if (gameListTopPrepare >= 100)
                GameListTopShift = 100;

                else if(gameListTopPrepare < 0 && ScreenWidth * gameListTopPrepare / SizeRef * -1 > gameListBottomLimit)
                GameListTopShift = gameListBottomLimit * -1 * SizeRef / ScreenWidth;

                else if(gameListTopPrepare < 100)
                GameListTopShift = gameListTopPrepare;
            }

            // Hidden lines selected
            float nbLineDisplayed = (ScreenHeight - yStart * 2.0f) / FullThumbnailHeight;
            int heightDisplayed = (int)(nbLineDisplayed * FullThumbnailHeight);
            int lineRequested = (MouseLeftClickTarget / 6) + 1;
            int lineTopHiddenRequestedY = (yStart + lineRequested * FullThumbnailHeight * -1 + FullThumbnailHeight) * SizeRef / ScreenWidth;
            int lineBottomHiddenRequestedY = (yStart + lineRequested * FullThumbnailHeight * -1 + heightDisplayed) * SizeRef / ScreenWidth;
            bool topHidden = selectedCartridgeY < yStart;
            bool bottomHidden = selectedCartridgeY + FullThumbnailHeight > ScreenHeight - yStart;
            bool topFullHidden = selectedCartridgeY + FullThumbnailHeight < yStart;
            bool bottomFullHidden = selectedCartridgeY > ScreenHeight - yStart;

            if (topHidden && (InputActionMove || (ThumbnailClicked && !topFullHidden)))
            {
                GameListTopShift = lineTopHiddenRequestedY;
                ThumbnailClicked = false;
            }
            if (bottomHidden && (InputActionMove || (ThumbnailClicked && !bottomFullHidden)))
            {
                GameListTopShift = lineBottomHiddenRequestedY;
                ThumbnailClicked = false;
            }

            InputActionMove = false;

            // Launch game
            if (MouseLeftDoubleClick && ThumbnailClicked)
            {
                MouseLeftClickPressed = 0;
                Program.EmulatorRun = true;
                Program.Emulation = new Emulation(GameList[MouseLeftClickTarget].Path);
                Audio.Init();
            }

            // Draw top and bottom rectangle
            Raylib.DrawRectangle(0, 0, ScreenWidth, yStart, Color.RAYWHITE);
            Raylib.DrawRectangle(0, ScreenHeight - yStart, ScreenWidth, yStart, Color.RAYWHITE);

            // Draw scrollbar
            if (scrollBarHeight < ScreenHeight)
            Raylib.DrawRectangle(ScreenWidth - scrollBarWidth, scrollPos, scrollBarWidth, scrollBarHeight, Color.DARKGRAY);

            // Set cursor
            Raylib.SetMouseCursor(cursor);

            // End draw
            Raylib.EndDrawing();
        }

        #endregion

        #region Text handle

        private static readonly int TextResolution = 4;
        private static TextureTitleSet[][] TitleTextures = Array.Empty<TextureTitleSet[]>();
        
        private struct TextureTitleSet
        {
            public Texture2D Texture;
            public float Width;
            public float Height;
        }

        private struct TextSet
        {
            public string Text = "";
            public int Width = 0;
            public int Height = 0;
        }

        // Title game
        private static TextureTitleSet[] TitleGameToTexture(List<TextSet> textSet, float size, float space, Color color)
        {
            TextureTitleSet[] Texture2DArr = new TextureTitleSet[textSet.Count];

            for (int i = 0; i < textSet.Count; i++)
            {
                Image titleToImg = Raylib.ImageTextEx(MainFont, textSet[i].Text, size, space, color);
                Texture2D titleTexture = Raylib.LoadTextureFromImage(titleToImg);
                Raylib.UnloadImage(titleToImg);
                Raylib.SetTextureFilter(titleTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                Texture2DArr[i].Texture = titleTexture;
                Texture2DArr[i].Width = titleToImg.Width / TextResolution;
                Texture2DArr[i].Height = titleToImg.Height / TextResolution;
            }

            return Texture2DArr;
        }

        // Text wrap and split line
        // ------------------------

        private static List<TextSet> TextNlWrap(string _text, float size, float space, float sizeMax)
        {
            string text = WordWrap(_text, space, size, sizeMax);
            float spaceWidth = GetCharSize((sbyte)' ', size).Width + space;
            float heightSize = 0;
            string[] words = text.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            int nbWord = words.Length;
            float wordSize;
            int nbWordThisLine = 0;
            float lineSize = 0;
            string lineText = "";
            List<TextSet> lines = new();

            for(int w = 0, c = 0; w < nbWord; w++, c++)
            {
                // Word
                wordSize = 0;
                for(int t = 0; t < words[w].Length; t++, c++)
                {
                    CharSize CharSize = GetCharSize((sbyte)text[t], size);
                    if (t + 1 < words[w].Length) CharSize.Width += space;
                    if (heightSize < CharSize.Height) heightSize = CharSize.Height;

                    wordSize += CharSize.Width;
                }

                // Line
                lineSize += wordSize;
                nbWordThisLine++;

                // Width line max
                if(lineSize + (nbWordThisLine * spaceWidth) > sizeMax)
                {
                    TextSet line = new();
                    line.Text = lineText.Trim();
                    line.Width = (int)(lineSize - wordSize + (nbWordThisLine * spaceWidth));
                    line.Height = (int)heightSize;
                    lines.Add(line);

                    lineText = "";
                    lineSize = wordSize;
                    nbWordThisLine = 0;

                    if (text.Length == c)
                    {
                        TextSet lastLine = new();
                        lastLine.Text = words[w].Trim();
                        lastLine.Width = (int)wordSize;
                        lastLine.Height = (int)heightSize;
                        lines.Add(lastLine);
                    }
                }
                else if(text.Length == c)
                {
                    TextSet line = new();
                    line.Text = (lineText +  words[w]).Trim();
                    line.Width = (int)(lineSize + (nbWordThisLine * spaceWidth));
                    line.Height = (int)heightSize;
                    lines.Add(line);
                }

                lineText += words[w] + " ";
            }

            return lines;
        }


        // Word wrap
        // ---------

        private static string WordWrap(string text, float space, float size, float wordSizeMax)
        {
            string newText = "";
            float wordSize = 0;

            for (int i = 0; i < text.Length; i++)
            {
                // Word detect
                if(text[i] == ' ')
                wordSize = 0;

                // Word size
                CharSize CharSize = GetCharSize((sbyte)text[i], size);

                if (i + 1 < text.Length)
                CharSize.Width += space;

                wordSize += CharSize.Width;

                // Word too long
                if (wordSize > wordSizeMax)
                {
                    wordSize = 0;
                    newText = newText.Remove(newText.Length-1);
                    newText += " " + text[i - 1];
                }

                // New text
                newText += text[i];
            }

            return newText;
        }

        // Get char size
        // -------------

        private struct CharSize
        {
            public float Width = 0;
            public float Height = 0;
        }

        private static unsafe CharSize GetCharSize(sbyte c, float size)
        {
            float scaleFactor = size / MainFont.BaseSize;
            int codepointByteCount = 0;
            int codepoint = Raylib.GetCodepoint(&c, &codepointByteCount);
            int index = Raylib.GetGlyphIndex(MainFont, codepoint);

            CharSize charSize = new();
            charSize.Width = (MainFont.Glyphs[index].AdvanceX == 0) ? MainFont.Recs[index].Width * scaleFactor : MainFont.Glyphs[index].AdvanceX * scaleFactor;
            charSize.Height = MainFont.Recs[index].Height * scaleFactor;

            return charSize;
        }

        #endregion

        #region Game list handle

        private struct Game
        {
            public string Path = "";
            public string Name = "";
            public string Cover = "";
            public string[] Infos = Array.Empty<string>();
            public string[] Tags = Array.Empty<string>();
        }

        private static void GetGameListPath()
        {
            List<string> ext = new(){ "gb", "gbc", "sgb" };
            IEnumerable<string> Games = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "Roms", "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            NbGame = Games.Count();
            GameList = new Game[NbGame];
            TitleTextures = new TextureTitleSet[NbGame][];

            for(int i = 0; i < Games.Count(); i++)
            {
                // Paths
                GameList[i].Path = Games.ElementAt(i);

                // Names
                Match match = Regex.Match(Games.ElementAt(i), @".*\\([^(\[]+).*\.(gb|sgb|gbc)$", RegexOptions.IgnoreCase);
                GameList[i].Name = match.Groups[1].Value.Trim();

                // Infos
                MatchCollection Infos = Regex.Matches(Games.ElementAt(i), @"\(([^)]+)\)");
                GameList[i].Infos = new string[Infos.Count];
                for (int l = 0; l < Infos.Count; l++)
                GameList[i].Infos[l] = Infos[l].Groups[1].Value;

                // Tags
                MatchCollection Tags = Regex.Matches(Games.ElementAt(i), @"\[([^]]+)\]");
                GameList[i].Tags = new string[Tags.Count];
                for (int l = 0; l < Tags.Count; l++)
                GameList[i].Tags[l] = Tags[l].Groups[1].Value;
            }
        }

        #endregion

        #region Other

        // Mouse click and double click
        private static int MouseLeftClickDelay = 0;
        private static int MouseLeftClickPressed = 0;
        private static int MouseLeftClickLastTarget = 0;
        private static int MouseLeftClickTarget = 0;
        private static bool MouseLeftDoubleClick = false;

        private static void MouseClickPressed(int index)
        {
            MouseLeftClickLastTarget = MouseLeftClickTarget;
            MouseLeftClickTarget = index;
        }

        #endregion
    }
}