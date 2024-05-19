// --------
// Rom list
// --------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static bool InputActionMove = false;
        private static bool ThumbnailClicked = false;
        private static bool scrollClicked = false;
        private static int scrollClickedPos = 0;
        private static int scrollClickedLastPosList = 0;
        private static int GameListTopShift = 100;
        private static int MouseLeftClickLastTarget = 0;
        private static int MouseLeftClickTarget = 0;

        // Game clicked
        private static void MouseClickTarget(int index)
        {
            MouseLeftClickLastTarget = MouseLeftClickTarget;
            MouseLeftClickTarget = index;
        }

        // Draw list
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
                        X = X + Formulas.CenterElm(CartridgeGB.Width, CartridgeWidthCol),
                        Y = Y + Formulas.CenterElm(CartridgeGB.Height, CartridgeHeightCol),
                        Width = CartridgeWidthCol,
                        Height = CartridgeHeightCol
                    };

                    // Draw title
                    for (int t = 0; t < TitleTextures[index].Length; t++)
                    {
                        TitleTextures[index][t].Texture.Width = Res(TitleTextures[index][t].Width);
                        TitleTextures[index][t].Texture.Height = Res(TitleTextures[index][t].Height);
                        int centerX = Formulas.CenterElm(CartridgeGB.Width, TitleTextures[index][t].Texture.Width);
                        
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
    }
}
