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
        private static int PosListTop = 0;
        private static bool ScrollVisible = false;
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
            int cartridgeHeight = Res(300);
            int yStart = Res(100);
            int yMargin = Res(65);
            double nbLine = Math.Ceiling(NbGame / 6.0f);
            int FullThumbnailHeight = cartridgeHeight + yMargin;
            int cartridgeWidth = Res(!ScrollVisible ? 400 : 395);
            int gameListPageHeight = (int)(nbLine * FullThumbnailHeight);
            ThumbnailClicked = false;

            // Delemiter area
            InitScreenSize();
            Raylib.BeginScissorMode(0, yStart - 1, ScreenWidth - Res(30), ScreenHeight - yStart * 2 + Res(10));

            // Draw line selected
            int selectedCartridgeX = MouseLeftClickTarget % 6 * cartridgeWidth;
            int selectedCartridgeY = (int)(PosListTop + (Math.Floor(MouseLeftClickTarget / 6.0f) * FullThumbnailHeight));
            int selectedCartridgeHeight = FullThumbnailHeight + Res(5);
            Raylib.DrawRectangle(selectedCartridgeX + Res(10), selectedCartridgeY, cartridgeWidth - Res(20), selectedCartridgeHeight, Color.LIGHTGRAY);
            Raylib.DrawRectangleLines(selectedCartridgeX + Res(10), selectedCartridgeY, cartridgeWidth - Res(20), selectedCartridgeHeight, Color.GRAY);

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
                    int Y = PosListTop + (y * FullThumbnailHeight);

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

            // Set scrollbar
            string ScrollBarName = "ScrollList";
            ScrollBarInit(ScrollBarName, 0);
            PosListTop = Res(100) - (int)ScrollBarList[ScrollBarName].ContentPosY;

            if (gameListPageHeight != 0)
            ScrollVisible = ScrollBarY(ScrollBarName, ScreenHeight - Res(200), gameListPageHeight, Res(30), ScreenHeight, ScreenWidth - Res(30), 0, focus, Color.RAYWHITE, Color.DARKGRAY) != 1;

            // Hidden lines selected
            selectedCartridgeY = (int)(PosListTop + (Math.Floor(MouseLeftClickTarget / 6.0f) * FullThumbnailHeight));
            float nbLineDisplayed = (ScreenHeight - yStart * 2.0f) / FullThumbnailHeight;
            int heightDisplayed = (int)(nbLineDisplayed * FullThumbnailHeight);
            int lineRequested = (MouseLeftClickTarget / 6) + 1;
            int lineTopHiddenRequestedY = lineRequested * FullThumbnailHeight - FullThumbnailHeight;
            int lineBottomHiddenRequestedY = lineRequested * FullThumbnailHeight - heightDisplayed;
            bool topHidden = selectedCartridgeY < yStart;
            bool bottomHidden = selectedCartridgeY + FullThumbnailHeight > ScreenHeight - yStart + 1;
            bool topFullHidden = selectedCartridgeY + FullThumbnailHeight < yStart;
            bool bottomFullHidden = selectedCartridgeY > ScreenHeight - yStart;

            if (topHidden && (InputActionMove || (ThumbnailClicked && !topFullHidden)) && !ScrollBarList[ScrollBarName].HangClicked)
            {
                ScrollBarSetY(ScrollBarName, lineTopHiddenRequestedY);
                ThumbnailClicked = false;
            }
            if (bottomHidden && (InputActionMove || (ThumbnailClicked && !bottomFullHidden)) && !ScrollBarList[ScrollBarName].HangClicked)
            {
                ScrollBarSetY(ScrollBarName, lineBottomHiddenRequestedY);
                ThumbnailClicked = false;
            }

            InputActionMove = false;

            // Launch game by double click
            if (Input.MouseLeftDoubleClick && ThumbnailClicked && MouseLeftClickLastTarget == MouseLeftClickTarget)
            Action("ListPlay");
        }
    }
}
