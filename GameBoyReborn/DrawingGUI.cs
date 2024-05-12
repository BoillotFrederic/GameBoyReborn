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
        private static string WhereIAm = "List";
        private static Vector2 Mouse;
        private static MouseCursor Cursor;

        public static void InitScreenSize()
        {
            ScreenWidth = Raylib.GetRenderWidth();
            ScreenHeight = Raylib.GetRenderHeight();
        }

        public static void InitMouse()
        {
            Mouse = Raylib.GetMousePosition();
            Cursor = MouseCursor.MOUSE_CURSOR_DEFAULT;
        }

        public static void UpdateMouse()
        {
            Raylib.SetMouseCursor(Cursor);
        }

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

        public static void MetroGB()
        {
            // Start draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RAYWHITE);

            // Mouse
            InitMouse();

            // Draw game list
            DrawListGame();

            // Draw info buttons
            DrawBtnInfos();

            // Actions listenning
            ActionsListenning();

            // Modals listenning
            ModalsListenning();

            // Update cursor
            UpdateMouse();

            // End draw
            Raylib.EndDrawing();
        }

        #endregion

        #region List handle

        private static bool InputActionMove = false;
        private static bool ThumbnailClicked = false;
        private static bool scrollClicked = false;
        private static int scrollClickedPos = 0;
        private static int scrollClickedLastPosList = 0;
        private static int GameListTopShift = 100;

        private static void DrawListGame()
        {
            // Operating variables
            ThumbnailClicked = false;
            bool focus = WhereIAm == "List";
            float mouseWheelMove = focus ? Raylib.GetMouseWheelMove() : 0;
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
            int gameListTopPrepare = GameListTopShift + (int)(mouseWheelMove * 100);
            int gameListBottomLimit = gameListPageHeight - (ScreenHeight - yStart);
            int scrollPos = (int)((float)gameListTop / gameListPageHeight * ScreenHeight) * -1;
            ThumbnailClicked = false;

            // Delemiter area
            InitScreenSize();
            Raylib.BeginScissorMode(0, yStart - 1, ScreenWidth - scrollBarWidth, ScreenHeight - yStart * 2);

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
                        if (focus && Raylib.CheckCollisionPointRec(Mouse, CartridgeTextRect) && (Mouse.Y < ScreenHeight - yStart + 6 && Mouse.Y > yStart - 1))
                        {
                            Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                            if(Input.Pressed("Click", Input.MouseLeftClick)) MouseClickTarget(index);
                            ThumbnailClicked = Input.MouseLeftClick;
                        }
                    }

                    // Mouse hover
                    if (focus && Raylib.CheckCollisionPointRec(Mouse, CartridgeRect) && (Mouse.Y < ScreenHeight - yStart + 6 && Mouse.Y > yStart - 1))
                    {
                        Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;
                        if(Input.Pressed("Click", Input.MouseLeftClick)) MouseClickTarget(index);
                        ThumbnailClicked = Input.MouseLeftClick;
                    }
                }
            }

            // Stop delimiter
            Raylib.EndScissorMode();

            // Draw scrollbar
            if (scrollBarHeight < ScreenHeight)
            Raylib.DrawRectangle(ScreenWidth - scrollBarWidth, scrollPos, scrollBarWidth, scrollBarHeight, Color.DARKGRAY);

            // Scrolling game list by click
            Rectangle scrollRect = new()
            {
                X = ScreenWidth - scrollBarWidth,
                Y = scrollPos,
                Width = scrollBarWidth,
                Height = scrollBarHeight
            };

            if (focus && Raylib.CheckCollisionPointRec(Mouse, scrollRect) && Input.Pressed("Click", Input.MouseLeftClick))
            {
                scrollClicked = true;
                scrollClickedPos = (int)Mouse.Y;
                scrollClickedLastPosList = GameListTopShift;
            }

            if (!Input.MouseLeftClick)
            scrollClicked = false;

            if (scrollClicked)
            {
                int shift = (int)(-1 * (float)((Mouse.Y - scrollClickedPos) / ScreenHeight) * gameListPageHeight);
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
            bool bottomHidden = selectedCartridgeY + FullThumbnailHeight > ScreenHeight - yStart + 1;
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
            if (Input.MouseLeftDoubleClick && ThumbnailClicked && MouseLeftClickLastTarget == MouseLeftClickTarget)
            Action("ListPlay");
        }

        #endregion

        #region Info buttons handle

        private struct BtnInfo
        {
            public string Action = "";
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
            // Clear
            if(BtnInfos != null)
            {
                Array.Clear(BtnInfos);
                BtnInfos = Array.Empty<BtnInfo>();
            }

            // Setting
            void btnInfosSet(string action, int index, string text, Color textColor, string IPad, string IKey, int IPadWidth, int IKeyWidth)
            {
                if(BtnInfos != null && index < BtnInfos.Length)
                {
                    BtnInfos[index].Action = action;
                    BtnInfos[index].Text = SingleToTexture(text, textSize, 3.0f, textColor);
                    BtnInfos[index].IconPad = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IPad);
                    BtnInfos[index].IconKey = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IKey);
                    BtnInfos[index].IconPadWidth = IPadWidth;
                    BtnInfos[index].IconKeyWidth = IKeyWidth;
                }
            }

            // Load btns
            switch (WhereIAm)
            {
                case "List":
                    BtnInfos = new BtnInfo[4];
                    btnInfosSet("ListOpenMenu", 0, "Menu", Color.WHITE, "TriggerLR.png", "KeyM.png", 140, 60);
                    btnInfosSet("ListGlobalConfig", 1, "Paramètres globaux", Color.WHITE, "ButtonY.png", "KeyG.png", 60, 60);
                    btnInfosSet("ListConfig", 2, "Paramètres", Color.WHITE, "ButtonX.png", "KeyC.png", 60, 60);
                    btnInfosSet("ListPlay", 3, "Jouer", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;

                case "MenuList":
                    BtnInfos = new BtnInfo[2];
                    btnInfosSet("CloseAllModals", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet(ModalShift.Count > 0 ? ModalShift[ModalShiftPos.Y][ModalShiftPos.X].Action : "-", 1, "Valider", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;

                case "MenuGame":
                    BtnInfos = new BtnInfo[2];
                    btnInfosSet("MenuGameRestore", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet(ModalShift.Count > 0 ? ModalShift[ModalShiftPos.Y][ModalShiftPos.X].Action : "-", 1, "Valider", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;
            }
        }

        public static void DrawBtnInfos()
        {
            if(BtnInfos != null)
            {
                int yStart = Res(100);
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

                    Rectangle rect = new() { X = buttonsStartPosX, Y = ScreenHeight - yStart + Res(20), Height = btnHeight, Width = btnWidth };
                    Raylib.DrawRectangleRec(rect, Color.DARKGRAY);
                    Raylib.DrawRectangleLinesEx(rect, 1.0f, Color.BLACK);
                    Raylib.DrawTextureEx(BtnInfos[b].Text, textPos, 0, textScale, Color.WHITE);

                    if(iconWidth != 0)
                    Raylib.DrawTextureEx(Input.IsPad ? BtnInfos[b].IconPad : BtnInfos[b].IconKey, iconPos, 0, iconScale, Color.WHITE);

                    buttonsStartPosX += btnWidth + lineSpacing;

                    // Hover and click
                    if (Raylib.CheckCollisionPointRec(Mouse, rect))
                    {
                        Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;

                        if (Input.Pressed("Click", Input.MouseLeftClick))
                        Action(BtnInfos[b].Action);
                    }
                }
            }
        }

        #endregion

        #region Actions

        // Action requested
        public static void Action(string name)
        {
            switch (name)
            {
                // Metro GB list action
                // --------------------

                // Move in list
                case "ListMoveUp":
                case "ListMoveDown":
                case "ListMoveLeft":
                case "ListMoveRight":
                    static void upDown(bool direction)
                    {
                        int newPos = MouseLeftClickTarget + (!direction ? -6 : 6);
                        MouseLeftClickTarget = (!direction && newPos >= 0) || (direction && newPos < NbGame) ? newPos : MouseLeftClickTarget;
                        InputActionMove = true;
                    }

                    static void leftRight(bool direction)
                    {
                        if (!direction) MouseLeftClickTarget = MouseLeftClickTarget % 6 == 0 ? MouseLeftClickTarget : MouseLeftClickTarget - 1;
                        else MouseLeftClickTarget = MouseLeftClickTarget % 6 == 5 ? MouseLeftClickTarget : MouseLeftClickTarget + 1;
                    }

                    switch (name)
                    {
                        case "ListMoveUp": upDown(false); break;
                        case "ListMoveDown": upDown(true); break;
                        case "ListMoveLeft": leftRight(false); break;
                        case "ListMoveRight": leftRight(true); break;
                    }
                break;

                // Move in modal
                case "ModalMoveUp":
                case "ModalMoveDown":
                case "ModalMoveLeft":
                case "ModalMoveRight":
                    if(ModalShift.Count > 0)
                    {
                        void modalShitY(List<List<ShiftElm>> elm, int newPos)
                        {
                            ModalShiftPos.X = 0;
                            ModalShiftPos.Y = elm.ElementAtOrDefault(newPos) != null ? newPos : ModalShiftPos.Y;
                        }

                        void modalShitX(List<ShiftElm> elm, int newPos)
                        {
                            ModalShiftPos.X = newPos >= 0 && newPos < elm.Count ? newPos : ModalShiftPos.X;
                        }

                        switch (name)
                        {
                            case "ModalMoveUp": modalShitY(ModalShift, ModalShiftPos.Y - 1); break;
                            case "ModalMoveDown": modalShitY(ModalShift, ModalShiftPos.Y + 1); break;
                            case "ModalMoveLeft": modalShitX(ModalShift[ModalShiftPos.Y], ModalShiftPos.X - 1); break;
                            case "ModalMoveRight": modalShitX(ModalShift[ModalShiftPos.Y], ModalShiftPos.X + 1); break;
                        }
                    }
                break;

                // Open menu
                case "ListOpenMenu":
                    WhereIAm = "MenuList";
                    ModalsOpen.Add("MenuList");
                    ModalsListenning();
                    BtnInfoInit(35.0f * TextResolution);
                break;

                // Open global config
                case "ListGlobalConfig":
                break;

                // Open config
                case "ListConfig":
                break;

                // Play game
                case "ListPlay":
                    Emulation.Start(GameList[MouseLeftClickTarget].Path);
                break;

                // Close all modals
                case "CloseAllModals":
                    ModalDestruct();
                    ModalsOpen.Clear();
                    WhereIAm = "List";
                    BtnInfoInit(35.0f * TextResolution);
                break;

                // Select directory for scan
                case "SelectDirForScan":
                    WhereIAm = "SelectDirForScan";
                    ModalsOpen.Add("SelectDirForScan");
                    ModalsListenning();
                break;

                // Menu in game
                case "MenuGame":
                    WhereIAm = "MenuGame";
                    ModalsOpen.Clear();
                    ModalsOpen.Add("MenuGame");
                    ModalsListenning();
                    BtnInfoInit(35.0f * TextResolution);
                break;

                // Return to the game
                case "MenuGameRestore":
                    if(Program.Emulation != null)
                    {
                        Program.Emulation.MenuIsOpen = false;
                        Program.Emulation.UnPause();
                    }
                break;

                case "CloseGame":
                    ModalDestruct();
                    ModalsOpen.Clear();
                    WhereIAm = "List";
                    BtnInfoInit(35.0f * TextResolution);

                    if (Program.Emulation != null)
                    Program.Emulation.Stop();
                break;

                default: break;
            }
        }

        // Actions listenning
        public static void ActionsListenning()
        {
            switch (WhereIAm)
            {
                // Metro GB list
                // -------------
                case "List":
                {
                    // Play
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP))
                    Action("ListPlay");

                    // Open menu
                    if(Input.Pressed("Press M", Input.KeyM || (Input.AxisLS && Input.AxisRS)))
                    Action("ListOpenMenu");

                    // Move up
                    if (Input.Repeat("Up", Input.DPadUp || Input.AxisLeftPadUp, 0.2f))
                    Action("ListMoveUp");

                    // Move down
                    if (Input.Repeat("Down", Input.DPadDown || Input.AxisLeftPadDown, 0.2f))
                    Action("ListMoveDown");

                    // Move left
                    if (Input.Repeat("Left", Input.DPadLeft || Input.AxisLeftPadLeft, 0.2f))
                    Action("ListMoveLeft");

                    // Move right
                    if (Input.Repeat("Right", Input.DPadRight || Input.AxisLeftPadRight, 0.2f))
                    Action("ListMoveRight");
                }
                break;

                case "SelectDirForScan":
                case "MenuList":
                {
                    // Confirm
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP) && ModalShift.Count > 0)
                    Action(ModalShift[ModalShiftPos.Y][ModalShiftPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("CloseAllModals");

                    // Move up
                    if (Input.Repeat("Up", Input.DPadUp || Input.AxisLeftPadUp, 0.2f))
                    Action("ModalMoveUp");

                    // Move down
                    if (Input.Repeat("Down", Input.DPadDown || Input.AxisLeftPadDown, 0.2f))
                    Action("ModalMoveDown");

                    // Move left
                    if (Input.Repeat("Left", Input.DPadLeft || Input.AxisLeftPadLeft, 0.2f))
                    Action("ModalMoveLeft");

                    // Move right
                    if (Input.Repeat("Right", Input.DPadRight || Input.AxisLeftPadRight, 0.2f))
                    Action("ModalMoveRight");
                }
                break;

                case "MenuGame": 
                {
                    // Confirm
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP) && ModalShift.Count > 0)
                    Action(ModalShift[ModalShiftPos.Y][ModalShiftPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("MenuGameRestore");

                    // Move up
                    if (Input.Repeat("Up", Input.DPadUp || Input.AxisLeftPadUp, 0.2f))
                    Action("ModalMoveUp");

                    // Move down
                    if (Input.Repeat("Down", Input.DPadDown || Input.AxisLeftPadDown, 0.2f))
                    Action("ModalMoveDown");

                    // Move left
                    if (Input.Repeat("Left", Input.DPadLeft || Input.AxisLeftPadLeft, 0.2f))
                    Action("ModalMoveLeft");

                    // Move right
                    if (Input.Repeat("Right", Input.DPadRight || Input.AxisLeftPadRight, 0.2f))
                    Action("ModalMoveRight");
                }
                break;

                default: break;
            }
        }

        #endregion

        #region Modals

        // Operating variables
        private static readonly List<string> ModalsOpen = new();
        private static readonly Dictionary<string, Dictionary<string, Texture2D>> ModalTextures = new();
        private static readonly Dictionary<string, bool> ModalIsInit = new();
        private static VecInt2 ModalShiftPos = new() { X = 0, Y = 0 };
        private static readonly List<List<ShiftElm>> ModalShift = new();
        private struct ShiftElm
        {
            public string Action;
            public Rectangle ElmRect;
        }

        // Modals listenning
        public static void ModalsListenning()
        {
            // Draw modal
            bool ShiftClicked = false;

            void DrawModal(string modal, int width, int height)
            {
                // Modal
                Rectangle modalRect = new() { Width = width, Height = height, X = Center(ScreenWidth, width), Y = Center(ScreenHeight, height) };
                Raylib.DrawRectangleRec(modalRect, Color.WHITE);
                Raylib.DrawRectangleLinesEx(modalRect, Res(10), Color.DARKGRAY);

                // Init
                InitModal(modal, modalRect);

                // Select line by hover and click handle
                int x = 0;
                int y = 0;

                if(ModalShift.Count > 0)
                foreach (List<ShiftElm> selectLines in ModalShift)
                {
                    foreach (ShiftElm selectLine in selectLines)
                    {
                        if(Raylib.CheckCollisionPointRec(Mouse, selectLine.ElmRect))
                        {
                            ModalShiftPos.X = x;
                            ModalShiftPos.Y = y;

                            if (Input.Pressed("Click", Input.MouseLeftClick))
                            ShiftClicked = true;
                        }
                        x++;
                    }
                    x = 0;
                    y++;
                }

                // Draw select line
                if(ModalShift.Count > 0)
                Raylib.DrawRectangleRec(ModalShift[ModalShiftPos.Y][ModalShiftPos.X].ElmRect, Color.LIGHTGRAY);

                // Content
                switch (modal)
                {
                    case "MenuList":
                    {
                        // Draw make scan
                        Texture2D makeScan = ModalTextures[modal]["MakeScan"];
                        Vector2 makeScanPos = new()
                        { 
                            X = modalRect.X + Center((int)modalRect.Width, Res(makeScan.Width)), 
                            Y = modalRect.Y + Center((int)modalRect.Height, Res(makeScan.Height))
                        };
                        Raylib.DrawTextureEx(makeScan, makeScanPos, 0, Res(makeScan.Width) / (float)makeScan.Width, Color.WHITE);
                    }
                    break;

                    case "SelectDirForScan":
                    {
                        // Draw coming soon
                        Texture2D comingSoon = ModalTextures[modal]["ComingSoon"];
                        Vector2 comingSoonPos = new()
                        { 
                            X = modalRect.X + Center((int)modalRect.Width, Res(comingSoon.Width)), 
                            Y = modalRect.Y + Center((int)modalRect.Height, Res(comingSoon.Height))
                        };
                        Raylib.DrawTextureEx(comingSoon, comingSoonPos, 0, Res(comingSoon.Width) / (float)comingSoon.Width, Color.WHITE);
                    }
                    break;

                    case "MenuGame":
                    {
                        // Draw menu game
                        Texture2D closeGame = ModalTextures[modal]["CloseGame"];
                        Texture2D saveGame = ModalTextures[modal]["SaveGame"];
                        Texture2D loadGame = ModalTextures[modal]["LoadGame"];

                        Vector2 closeGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(50) };
                        Raylib.DrawTextureEx(closeGame, closeGamePos, 0, Res(closeGame.Width) / (float)closeGame.Width, Color.WHITE);

                        Vector2 saveGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(150) };
                        Raylib.DrawTextureEx(saveGame, saveGamePos, 0, Res(saveGame.Width) / (float)saveGame.Width, Color.WHITE);

                        Vector2 loadGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(250) };
                        Raylib.DrawTextureEx(loadGame, loadGamePos, 0, Res(loadGame.Width) / (float)loadGame.Width, Color.WHITE);
                    }
                    break;
                }
            }

            // Modals match
            foreach(string modal in ModalsOpen)
            {
                switch (modal)
                {
                    case "MenuList": DrawModal(modal, Res(1024), Res(250)); break;
                    case "SelectDirForScan": DrawModal(modal, Res(900), Res(200)); break;
                    case "MenuGame": DrawModal(modal, Res(1024), Res(345)); break;
                }
            }

            // Select line clicked
            if(ModalShift.Count > 0 && ShiftClicked)
            Action(ModalShift[ModalShiftPos.Y][ModalShiftPos.X].Action);
        }

        // Modal destructor
        // ----------------
        
        static void ModalDestruct(string modalName = "")
        {
            // Destruct
            static void destruct(string MN)
            {
                if (ModalIsInit.ContainsKey(MN))
                {
                    foreach (var Item in ModalTextures[MN])
                    {
                        ModalIsInit[MN] = false;
                        Raylib.UnloadTexture(ModalTextures[MN][Item.Key]);
                    }

                    ModalTextures[MN].Clear();
                }
            }

            // All modal
            if (modalName == "")
            {
                foreach (var Item in ModalTextures)
                destruct(Item.Key);
            }

            // One modal
            destruct(modalName);
        }

        // Init Modal
        // ----------

        static void InitModal(string modal, Rectangle modalRect)
        {
            if (!ModalIsInit.ContainsKey(modal))
            ModalIsInit.Add(modal, false);

            // Init modal textures
            if (!ModalTextures.ContainsKey(modal))
            ModalTextures.Add(modal, new Dictionary<string, Texture2D>());

            if (!ModalIsInit[modal])
            {
                switch (modal)
                {
                    case "MenuList":
                        ModalTextures[modal].Add("MakeScan", SingleToTexture("Lancer un scan (ceci réinitialisera la liste)", 40.0f * TextResolution, 3.0f, Color.BLACK));
                    break;

                    case "SelectDirForScan":
                        ModalTextures[modal].Add("ComingSoon", SingleToTexture("Prochainement...", 40.0f * TextResolution, 3.0f, Color.BLACK));
                    break;

                    case "MenuGame":
                        ModalTextures[modal].Add("CloseGame", SingleToTexture("Quitter le jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
                        ModalTextures[modal].Add("SaveGame", SingleToTexture("Sauvegarder l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
                        ModalTextures[modal].Add("LoadGame", SingleToTexture("Charger l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
                    break;
                }
            }

            // Init modal shift
            ModalShift.Clear();

            switch (WhereIAm)
            {
                case "MenuList":
                {
                    Rectangle lineScanRect = new()
                    {
                        Width = modalRect.Width - Res(100),
                        Height = Res(80),
                        X = modalRect.X + Center((int)modalRect.Width, (int)modalRect.Width - Res(100)),
                        Y = modalRect.Y + Center((int)modalRect.Height, Res(80)),
                    };

                    List<ShiftElm> lineScan = new();
                    lineScan.Add(new ShiftElm() { Action = "SelectDirForScan", ElmRect = lineScanRect });
                    ModalShift.Add(lineScan);
                }
                break;

                case "SelectDirForScan":
                break;

                case "MenuGame":
                {
                    List<ShiftElm> line1 = new();
                    line1.Add(new ShiftElm() { Action = "CloseGame", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(40), Width = modalRect.Width - Res(80), Height = Res(60) } });
                    ModalShift.Add(line1);

                    List<ShiftElm> line2 = new();
                    line2.Add(new ShiftElm() { Action = "-", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(140), Width = modalRect.Width - Res(80), Height = Res(60) } });
                    ModalShift.Add(line2);

                    List<ShiftElm> line3 = new();
                    line3.Add(new ShiftElm() { Action = "-", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(240), Width = modalRect.Width - Res(80), Height = Res(60) } });
                    ModalShift.Add(line3);
                }
                break;
            }

            ModalIsInit[modal] = true;
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

        // Mouse click target
        private static int MouseLeftClickLastTarget = 0;
        private static int MouseLeftClickTarget = 0;

        private static void MouseClickTarget(int index)
        {
            MouseLeftClickLastTarget = MouseLeftClickTarget;
            MouseLeftClickTarget = index;
        }

        #endregion

        #region Tools

        // Vector
        private struct VecInt2
        {
            public int X;
            public int Y;
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

        // Percent
        private static int Percent(int percent, int integer)
        {
            return integer * percent / 100;
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