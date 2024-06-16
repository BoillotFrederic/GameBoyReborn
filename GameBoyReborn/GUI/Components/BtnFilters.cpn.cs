// --------------
// Buttons filter
// --------------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // List
        private static readonly string[] BtnFiltersList = new string[] 
        { 
            "Récent", ".", "#", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
        };

        // Selected
        private static int BtnFilterSelected = 0;
        private static int BtnLastFilterSelected = -1;

        // Icon pad
        private static Texture2D BtnIconPadLeft;
        private static Texture2D BtnIconPadRight;

        // Init
        // ----
        private static void BtnFilterInit()
        {
            BtnIconPadLeft = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/ButtonLB.png");
            BtnIconPadRight = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/ButtonRB.png");

            Raylib.SetTextureFilter(BtnIconPadLeft, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.SetTextureFilter(BtnIconPadRight, TextureFilter.TEXTURE_FILTER_BILINEAR);
        }

        // Draw btn filters
        // ----------------

        private static void DrawBtnFilters()
        {
            // Get button sizes
            float btnsSizeX = 0;
            float btnsSizeY = 0;
            int btnFiltersSize = Res(40);
            int btnFiltersSpace = Res(3);
            var btnsSizeArr = new Vector2[BtnFiltersList.Length];

            for(int i = 0; i < BtnFiltersList.Length; i++)
            {
                btnsSizeArr[i] = Raylib.MeasureTextEx(MainFont, BtnFiltersList[i], btnFiltersSize, btnFiltersSpace);
                btnsSizeX += btnsSizeArr[i].X + Res(40);
                btnsSizeY = btnsSizeY < btnsSizeArr[i].Y ? btnsSizeY : btnsSizeArr[i].Y;
            }

            // // Draw
            int startX = Formulas.CenterElm(ScreenWidth, (int)btnsSizeX);

            if (Input.IsPad) // Draw icon left
            Raylib.DrawTextureEx(BtnIconPadLeft, new Vector2() { X = startX - Res(50), Y = Res(5) }, 0, Res(0.4f * 100) / 100.0f, Color.WHITE);

            for (int i = 0; i < BtnFiltersList.Length; i++)
            {
                // Button rectangle
                Rectangle btnRect = new()
                {
                    X = startX - Res(10),
                    Y = Res(3),
                    Width = btnsSizeArr[i].X + Res(20),
                    Height = btnsSizeArr[i].Y + Res(20)
                };

                // Button selected
                if(i == BtnFilterSelected)
                {
                    Raylib.DrawRectangleRec(btnRect, Color.LIGHTGRAY);
                    Raylib.DrawRectangleLinesEx(btnRect, 1, Color.BLACK);
                }

                // Button content
                Raylib.DrawTextEx(MainFont, BtnFiltersList[i], new() { X = startX, Y = Res(13) }, btnFiltersSize, btnFiltersSpace, Color.BLACK);

                // Mouse hover and mouse click
                if(Raylib.CheckCollisionPointRec(Mouse, btnRect) && WhereIAm == "List")
                {
                    // Hover
                    Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;

                    // Click
                    if(Input.Pressed("Click", Input.MouseLeftClick))
                    BtnFilterSelected = i;
                }

                // New pos
                startX += (int)btnsSizeArr[i].X + Res(40);
            }

            if (Input.IsPad) // Draw icon Right
            Raylib.DrawTextureEx(BtnIconPadRight, new Vector2() { X = startX - Res(20), Y = Res(5) }, 0, Res(0.4f * 100) / 100.0f, Color.WHITE);

            // Aplly change
            BtnFilterApply();

            // Remember last filter selected
            BtnLastFilterSelected = BtnFilterSelected;
        }

        // Filter apply
        // ------------

        private static void BtnFilterApply()
        {
            // Filter changed
            if(BtnFilterSelected != BtnLastFilterSelected)
            {
                ScrollBarSetY("ScrollList", 0);
                MouseClickTarget(0);

                // Recent
                if(BtnFilterSelected == 0)
                {
                    GameList = GameListOrigin.Where(game => game.LatestLaunch != 0).OrderByDescending(e => e.LatestLaunch).ToArray();
                    NbGame = Math.Clamp(GameList.Length, 0, 20);
                }

                // All
                else if(BtnFilterSelected == 1)
                {
                    GameList = GameListOrigin.OrderBy(e => e.Name, new AlphanumericComparer()).ToArray();
                    NbGame = GameList.Length;
                }

                // Start by
                else
                {
                    if(BtnFilterSelected != 2)
                    GameList = FilterGamesByFirstLetter(GameListOrigin, new char[] { BtnFiltersList[BtnFilterSelected][0] });
                    else
                    GameList = FilterGamesByFirstLetter(GameListOrigin, new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

                    GameList = GameList.OrderBy(e => e.Name, new AlphanumericComparer()).ToArray();
                    NbGame = GameList.Length;
                }
            }
        }
    }
}
