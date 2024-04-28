// -----------
// Drawing GUI
// -----------

using Raylib_cs;
using System.Numerics;
using System.Text.RegularExpressions;
using Emulator;
using System.Text;
using System.Runtime.InteropServices;

namespace GameBoyReborn
{
    public class DrawingGUI
    {
        #region Draw GUI

        private static Game[] GameList = Array.Empty<Game>();
        private static Font MainFont = LoadFont(AppDomain.CurrentDomain.BaseDirectory + "Fonts/ErasBoldITC.ttf");
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
        private static string WhereIAm = "List";

        public static void InitMetroGB()
        {
            // Game list
            GetGameListPath();

            // Texture cartridge
            Raylib.SetTextureFilter(CartridgeGB, TextureFilter.TEXTURE_FILTER_BILINEAR);
            
            // Texture cartridge text
            for (int i = 0; i < NbGame; i++)
            {
                List<TextSet> TitleTest = TextNlWrap(GameList[i].Name, 30.0f * TextResolution, 3.0f, 300.0f * TextResolution, 3);
                TitleTextures[i] = TitleGameToTexture(TitleTest, 30.0f * TextResolution, 3.0f, Color.GRAY);
            }

            // Buttons infos
            BtnInfoInit(35.0f * TextResolution);
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
            int cartridgeHeight = Res(300);
            int yStart = Res(100);
            int yMargin = Res(65);
            double nbLine = Math.Ceiling(NbGame / 6.0f);
            int FullThumbnailHeight = cartridgeHeight + yMargin;
            int scrollBarWidth = Res(30);
            int scrollBarHeight = (int)((float)ScreenHeight / ((nbLine * FullThumbnailHeight) + (yStart * 2)) * ScreenHeight);
            int cartridgeWidth = Res((scrollBarHeight > ScreenHeight) ? 400 : 395);
            int gameListPageHeight = (int)(nbLine * FullThumbnailHeight);
            int gameListTop = Res(GameListTopShift);
            int gameListTopPrepare = GameListTopShift + (int)(Raylib.GetMouseWheelMove() * 100);
            int gameListBottomLimit = gameListPageHeight - (ScreenHeight - yStart);
            int scrollPos = (int)((float)gameListTop / gameListPageHeight * ScreenHeight) * -1;

            // Draw line selected
            int selectedCartridgeX = MouseLeftClickTarget % 6 * cartridgeWidth;
            int selectedCartridgeY = (int)(gameListTop + (Math.Floor(MouseLeftClickTarget / 6.0f) * FullThumbnailHeight));
            int selectedCartridgeHeight = FullThumbnailHeight + Res(5);
            Raylib.DrawRectangle(selectedCartridgeX, selectedCartridgeY, cartridgeWidth, selectedCartridgeHeight, Color.LIGHTGRAY);
            Raylib.DrawRectangleLines(selectedCartridgeX, selectedCartridgeY, cartridgeWidth, selectedCartridgeHeight, Color.GRAY);

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
                    int CartridgeWidthCol = (int)(CartridgeGB.Width * 0.55f);
                    int CartridgeHeightCol = (int)(CartridgeGB.Height * 0.822f);

                    Rectangle CartridgeRect = new()
                    {
                        X = X + Center(CartridgeGB.Width, CartridgeWidthCol),
                        Y = Y + Center(CartridgeGB.Height, CartridgeHeightCol),
                        Width = CartridgeWidthCol,
                        Height = CartridgeHeightCol
                    };

                    // Draw title
                    for (int t = 0; t < TitleTextures[index].Length; t++)
                    {
                        TitleTextures[index][t].Texture.Width = Res(TitleTextures[index][t].Width);
                        TitleTextures[index][t].Texture.Height = Res(TitleTextures[index][t].Height);
                        int centerX = Center(CartridgeGB.Width, TitleTextures[index][t].Texture.Width);
                        
                        int shiftY = 0;
                        if(t != 0)
                        for (int s = 1; s < t + 1; s++)
                        shiftY += TitleTextures[index][s].Texture.Height;

                        Raylib.DrawTexture(TitleTextures[index][t].Texture, X + centerX, Y + shiftY + Res(275), Color.WHITE);

                        // Collision area
                        Rectangle CartridgeTextRect = new()
                        {
                            X = X + centerX,
                            Y = Y + shiftY + Res(275),
                            Width = TitleTextures[index][t].Texture.Width,
                            Height = TitleTextures[index][t].Texture.Height
                        };

                        // Mouse hover
                        if (Raylib.CheckCollisionPointRec(mouse, CartridgeTextRect) && (mouse.Y < ScreenHeight - yStart + 6 && mouse.Y > yStart - 1))
                        {
                            cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                            if(Input.MouseLeftClickPressed) MouseClickPressed(index);
                            if(Input.MouseLeftClick) ThumbnailClicked = true;
                        }
                    }

                    // Mouse hover
                    if (Raylib.CheckCollisionPointRec(mouse, CartridgeRect) && (mouse.Y < ScreenHeight - yStart + 6 && mouse.Y > yStart - 1))
                    {
                        cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                        if(Input.MouseLeftClickPressed) MouseClickPressed(index);
                        if(Input.MouseLeftClick) ThumbnailClicked = true;
                    }
                }
            }

            // Scrolling game list by click
            Rectangle scrollRect = new()
            {
                X = ScreenWidth - scrollBarWidth,
                Y = scrollPos,
                Width = scrollBarWidth,
                Height = scrollBarHeight
            };

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
                gameListTopPrepare = (int)ResI((float)Res(scrollClickedLastPosList) + shift);
            }

            // Scrolling game list by mouse wheel
            if (scrollBarHeight < ScreenHeight)
            {
                if (gameListTopPrepare >= 100)
                GameListTopShift = 100;

                else if(gameListTopPrepare < 0 && Res(gameListTopPrepare) * -1 > gameListBottomLimit)
                GameListTopShift = (int)ResI(gameListBottomLimit * -1);

                else if(gameListTopPrepare < 100)
                GameListTopShift = gameListTopPrepare;
            }

            // Hidden lines selected
            selectedCartridgeY = (int)(gameListTop + (Math.Floor(MouseLeftClickTarget / 6.0f) * FullThumbnailHeight));
            float nbLineDisplayed = (ScreenHeight - yStart * 2.0f) / FullThumbnailHeight;
            int heightDisplayed = (int)(nbLineDisplayed * FullThumbnailHeight);
            int lineRequested = (MouseLeftClickTarget / 6) + 1;
            int lineTopHiddenRequestedY = (int)ResI(yStart + lineRequested * FullThumbnailHeight * -1 + FullThumbnailHeight);
            int lineBottomHiddenRequestedY = (int)ResI(yStart + lineRequested * FullThumbnailHeight * -1 + heightDisplayed);
            bool topHidden = selectedCartridgeY < yStart;
            bool bottomHidden = selectedCartridgeY + FullThumbnailHeight > ScreenHeight - yStart;
            bool topFullHidden = selectedCartridgeY + FullThumbnailHeight < yStart;
            bool bottomFullHidden = selectedCartridgeY > ScreenHeight - yStart;

            if (topHidden && (InputActionMove || (ThumbnailClicked && !topFullHidden)) && !scrollClicked)
            {
                GameListTopShift = lineTopHiddenRequestedY;
                ThumbnailClicked = false;
            }
            if (bottomHidden && (InputActionMove || (ThumbnailClicked && !bottomFullHidden)) && !scrollClicked)
            {
                GameListTopShift = lineBottomHiddenRequestedY;
                ThumbnailClicked = false;
            }

            InputActionMove = false;

            // Launch game
            if (MouseLeftDoubleClick && ThumbnailClicked)
            {
                MouseLeftClickPressed = 0;
                Action("Play");
            }

            // Draw top and bottom rectangle
            Raylib.DrawRectangle(0, 0, ScreenWidth, yStart - Res(1), Color.RAYWHITE);
            Raylib.DrawRectangle(0, ScreenHeight - yStart + Res(6), ScreenWidth, yStart, Color.RAYWHITE);

            // Draw info buttons
            if(BtnInfos != null)
            {
                int lineWidth = 0;
                int lineSpacing = Res(20);
                int marginWidth = Res(20);

                for (int b = 0; b < BtnInfos.Length; b++)
                {
                    int iconWidth = Input.IsPad ? BtnInfos[b].IconPadWidth : BtnInfos[b].IconKeyWidth;
                    int nbMargin = iconWidth == 0 ? 2 : 3;
                    lineWidth += Res(iconWidth) + marginWidth * nbMargin + Res(BtnInfos[b].Text.Width);
                }

                int buttonsStartPosX = Center(ScreenWidth, lineWidth + lineSpacing * (BtnInfos.Length - 1));

                for (int b = 0; b < BtnInfos.Length; b++)
                {
                    int iconWidth = Input.IsPad ? BtnInfos[b].IconPadWidth : BtnInfos[b].IconKeyWidth;
                    int iconTextureWidth = Input.IsPad ? BtnInfos[b].IconPad.Width : BtnInfos[b].IconKey.Width;
                    int nbMargin = iconWidth == 0 ? 2 : 3;
                    int textWidth = Res(BtnInfos[b].Text.Width);
                    int btnWidth = Res(iconWidth) + marginWidth * nbMargin + textWidth;
                    int btnHeight = Res(70);
                    Vector2 textPos = new() { X = buttonsStartPosX + Res(iconWidth) + marginWidth * (nbMargin - 1), Y = ScreenHeight - yStart + Res(36) };
                    Vector2 iconPos = new() { X = buttonsStartPosX + marginWidth, Y = ScreenHeight - yStart + Res(25) };
                    float textScale = textWidth / (float)BtnInfos[b].Text.Width;
                    float iconScale = Res(iconWidth) / (float)iconTextureWidth;

                    Raylib.DrawRectangle(buttonsStartPosX, ScreenHeight - yStart + Res(20), btnWidth, btnHeight, Color.DARKGRAY);
                    Raylib.DrawRectangleLines(buttonsStartPosX, ScreenHeight - yStart + Res(20), btnWidth, btnHeight, Color.BLACK);
                    Raylib.DrawTextureEx(BtnInfos[b].Text, textPos, 0, textScale, Color.WHITE);

                    if(iconWidth != 0)
                    Raylib.DrawTextureEx(Input.IsPad ? BtnInfos[b].IconPad : BtnInfos[b].IconKey, iconPos, 0, iconScale, Color.WHITE);

                    buttonsStartPosX += btnWidth + lineSpacing;
                }
            }

            // Draw scrollbar
            if (scrollBarHeight < ScreenHeight)
            Raylib.DrawRectangle(ScreenWidth - scrollBarWidth, scrollPos, scrollBarWidth, scrollBarHeight, Color.DARKGRAY);

            // Set cursor
            Raylib.SetMouseCursor(cursor);

            // End draw
            Raylib.EndDrawing();

            // Actions listenning
            ActionsListenning();
        }

        #endregion

        #region Info buttons handle

        private struct BtnInfo
        {
            public Texture2D IconPad;
            public Texture2D IconKey;
            public Texture2D Text;
            public int IconPadWidth = 0;
            public int IconKeyWidth = 0;
        }

        private static BtnInfo[]? BtnInfos;

        // Buttons init
        private static void BtnInfoInit(float textSize)
        {
            // Buttons infos
            BtnInfos = new BtnInfo[4];

            void btnInfosSet(int index, string text, Color textColor, string IPad, string IKey, int IPadWidth, int IKeyWidth)
            {
                if(BtnInfos != null && index < BtnInfos.Length)
                {
                    BtnInfos[index].Text = SingleToTexture(text, textSize, 3.0f, textColor);
                    BtnInfos[index].IconPad = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IPad);
                    BtnInfos[index].IconKey = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IKey);
                    BtnInfos[index].IconPadWidth = IPadWidth;
                    BtnInfos[index].IconKeyWidth = IKeyWidth;
                }
            }

            btnInfosSet(0, "Menu", Color.WHITE, "TriggerLR.png", "KeyM.png", 140, 60);
            btnInfosSet(1, "Paramètres globaux", Color.WHITE, "ButtonY.png", "KeyG.png", 60, 60);
            btnInfosSet(2, "Paramètres", Color.WHITE, "ButtonX.png", "KeyC.png", 60, 60);
            btnInfosSet(3, "Jouer", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
        }

        #endregion

        #region Actions

        // Action requested
        private static void Action(string name)
        {
            switch (name)
            {
                case "Menu":
                break;

                case "GlobalConfig":
                break;

                case "Config":
                break;

                case "Play":
                    Program.EmulatorRun = true;
                    Program.Emulation = new Emulation(GameList[MouseLeftClickTarget].Path);
                    Audio.Init();
                break;

                default: break;
            }
        }

        // Actions listenning
        private static void ActionsListenning()
        {
            switch (WhereIAm)
            {
                case "List":
                    if(Raylib.IsGamepadButtonPressed(Input.GetGamePad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN) || Raylib.IsKeyPressed(KeyboardKey.KEY_P))
                    Action("Play");
                break;

                default: break;
            }
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

        // Single text
        private static Texture2D SingleToTexture(string textSet, float size, float space, Color color)
        {
            Image textToImg = Raylib.ImageTextEx(MainFont, textSet, size, space, color);
            Texture2D textTexture = Raylib.LoadTextureFromImage(textToImg);
            Raylib.UnloadImage(textToImg);
            Raylib.SetTextureFilter(textTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            textTexture.Width = textToImg.Width / TextResolution;
            textTexture.Height = textToImg.Height / TextResolution;

            return textTexture;
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

        private static List<TextSet> TextNlWrap(string _text, float size, float space, float sizeMax, int limitLine)
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

                // Add line
                void AddLine(string text, int width)
                {
                    TextSet line = new();
                    line.Text = text;
                    line.Width = width;
                    line.Height = (int)heightSize;
                    lines.Add(line);
                };

                // Width line max
                if(lineSize + (nbWordThisLine * spaceWidth) > sizeMax)
                {
                    AddLine(lineText.Trim(), (int)(lineSize - wordSize + (nbWordThisLine * spaceWidth)));

                    lineText = "";
                    lineSize = wordSize;
                    nbWordThisLine = 0;

                    if (text.Length == c)
                    AddLine(words[w].Trim(), (int)wordSize);
                }
                else if(text.Length == c)
                AddLine((lineText + words[w]).Trim(), (int)(lineSize + (nbWordThisLine * spaceWidth)));

                lineText += words[w] + " ";
            }

            // Limit line
            if(lines.Count > limitLine)
            {
                lines.RemoveRange(limitLine, lines.Count - limitLine);
                int lastIndex = lines.Count - 1;
                var lastItem = lines[lastIndex];
                lastItem.Text += "...";
                lines[lastIndex] = lastItem;
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

        // Convert UTF16 to UTF8
        public static unsafe sbyte* StrToSbyte(string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);

            fixed (byte* p = bytes)
            {
                sbyte* sp = (sbyte*)p;
                return sp;
            }               
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

        #region Tools

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

        // Responsive
        private static int Res(float size)
        {
            return (int)(ScreenWidth * size / SizeRef);
        }

        private static float ResI(float size)
        {
            return size * SizeRef / ScreenWidth;
        }

        // Center
        private static int Center(int container, int item)
        {
            return (container - item) / 2;
        }

        // Load font
        private static unsafe Font LoadFont(string path)
        {
            IntPtr ptrUtf8 = Marshal.StringToCoTaskMemUTF8(path);
            sbyte* sbytePtr = (sbyte*)ptrUtf8.ToPointer();
            Font fontTtf = Raylib.LoadFontEx(sbytePtr, 50, null, 250);
            Marshal.FreeCoTaskMem(ptrUtf8);

            return fontTtf;
        }

        #endregion
    }
}