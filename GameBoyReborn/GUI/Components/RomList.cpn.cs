// --------
// Rom list
// --------

using Raylib_cs;
using System.Numerics;

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
        private static readonly Dictionary<string, Texture2D> CoverTextures = new();

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

            if(MouseLeftClickTarget + 1 <= NbGame)
            {
                Raylib.DrawRectangle(selectedCartridgeX + Res(10), selectedCartridgeY, cartridgeWidth - Res(20), selectedCartridgeHeight, Color.LIGHTGRAY);
                Raylib.DrawRectangleLines(selectedCartridgeX + Res(10), selectedCartridgeY, cartridgeWidth - Res(20), selectedCartridgeHeight, Color.GRAY);
            }

            // Draw game list
            for(int y = 0; y < nbLine; y++)
            {
                for(int x = 0; x < 6; x++)
                {
                    int index = y * 6 + x;

                    // Check index if exist
                    if (index >= NbGame)
                    break;

                    // Where
                    int X = x * cartridgeWidth;
                    int Y = PosListTop + (y * FullThumbnailHeight);

                    // that the cartridge is not outside the area
                    if (Y < -Res(300) || Y > ScreenHeight + Res(300))
                    break;

                    // Draw cartridge
                    Texture2D CartridgeTexture = new();
                    switch (GameList[index].SystemTarget)
                    {
                        case 0: CartridgeTexture = CartridgeGBClassic; break;
                        case 1: CartridgeTexture = CartridgeGBSuper; break;
                        case 2: CartridgeTexture = CartridgeGBColor; break;
                    }

                    CartridgeTexture.Width = cartridgeWidth;
                    CartridgeTexture.Height = cartridgeHeight;
                    Raylib.DrawTexture(CartridgeTexture, X, Y, Color.WHITE);

                    // Draw cover
                    if (GameList[index].Cover != "")
                    {
                        if (!CoverTextures.ContainsKey(GameList[index].Name))
                        {
                            CoverTextures.Add(GameList[index].Name, Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Covers/" + GameList[index].Cover + ".png"));
                            Raylib.SetTextureFilter(CoverTextures[GameList[index].Name], TextureFilter.TEXTURE_FILTER_BILINEAR);
                        }

                        Texture2D CoverTexture = CoverTextures[GameList[index].Name];
                        CoverTexture.Width = Res(!ScrollVisible ? 158 : 156);
                        CoverTexture.Height = Res(140);
                        Raylib.DrawTexture(CoverTexture, X + Res(!ScrollVisible ? 121 : 120), Y + Res(100), Color.WHITE);
                    }

                    // Collision area
                    int CartridgeWidthCol = (int)(CartridgeTexture.Width * 0.55f);
                    int CartridgeHeightCol = (int)(CartridgeTexture.Height * 0.822f);

                    Rectangle CartridgeRect = new()
                    {
                        X = X + Formulas.CenterElm(CartridgeTexture.Width, CartridgeWidthCol),
                        Y = Y + Formulas.CenterElm(CartridgeTexture.Height, CartridgeHeightCol),
                        Width = CartridgeWidthCol,
                        Height = CartridgeHeightCol
                    };

                    // Draw title
                    List<string> TitleWrapped = TextNlWrap(GameList[index].Name, Res(30.0f), Res(3.0f), cartridgeWidth - Res(50), 3);
                    for (int t = 0; t < TitleWrapped.Count; t++)
                    {
                        Vector2 textMesure = Raylib.MeasureTextEx(MainFont, TitleWrapped[t], Res(30.0f), Res(3.0f));
                        int centerX = Formulas.CenterElm(CartridgeTexture.Width, (int)textMesure.X);
                        int shiftY = 0;

                        if(t != 0)
                        for (int s = 1; s < t + 1; s++)
                        shiftY += (int)textMesure.Y;

                        Raylib.DrawTextEx(MainFont, TitleWrapped[t], new Vector2() { X = X + centerX, Y = Y + shiftY + Res(275) }, Res(30), Res(3.0f), Color.GRAY);

                        // Collision text area
                        Rectangle CartridgeTextRect = new()
                        {
                            X = X + centerX,
                            Y = Y + shiftY + Res(275),
                            Width = (int)textMesure.X,
                            Height = (int)textMesure.Y
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
