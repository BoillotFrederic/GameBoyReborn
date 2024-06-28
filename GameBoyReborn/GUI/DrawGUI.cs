// --------
// Draw GUI
// --------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Operating variables
        // -------------------

        private static Game[] GameListOrigin = Array.Empty<Game>();
        private static Game[] GameList = Array.Empty<Game>();
        private static Font MainFont = Raylib.LoadFontEx(AppDomain.CurrentDomain.BaseDirectory + "Fonts/ErasBoldITC.ttf", 50, null, 250);
        private static Texture2D CartridgeGBClassic = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/CartridgeGBClassic.png");
        private static Texture2D CartridgeGBSuper = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/CartridgeGBSuper.png");
        private static Texture2D CartridgeGBColor = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/CartridgeGBColor.png");
        private static readonly int SizeRef = 2400; // 6 * (item cartridge 400) + (scrollbar 20)
        private static int ScreenWidth;
        private static int ScreenHeight;
        private static int NbGame;
        private static string WhereIAm = "List";
        private static string WhereIAmBack = "List";
        private static Vector2 Mouse;
        private static MouseCursor Cursor;

        // Metro GB
        // --------

        // Init
        public static void InitMetroGB()
        {
            // Init modals
            InitModals();

            // Game list
            ReadGameList();

            // Texture cartridge
            Raylib.SetTextureFilter(CartridgeGBClassic, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.SetTextureFilter(CartridgeGBSuper, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.SetTextureFilter(CartridgeGBColor, TextureFilter.TEXTURE_FILTER_BILINEAR);

            // Init buttons infos
            BtnInfoInit();

            // Init buttons filter
            BtnFilterInit();
        }

        // Draw metro GB
        public static void MetroGB()
        {
            // Start draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.RAYWHITE);

            // Init mouse
            InitMouse();

            // Draw game list
            DrawListGame();

            // Draw info buttons
            DrawBtnInfos();

            // Draw filter buttons
            DrawBtnFilters();

            // Actions listenning
            ActionsListenning();

            // Modals listenning
            ModalsListenning();

            // Update cursor
            UpdateMouse();

            // Show FPS
            DrawFPS();

            // End draw
            Raylib.EndDrawing();
        }

        // Draw menu in game
        public static void MenuInGame()
        {
            Texture2D screenTexture = new();
            
            // Start draw
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            DrawGB.ScreenPaused(ref screenTexture);

            // Init screen size
            InitScreenSize();

            // Init mouse
            InitMouse();

            // Draw info buttons
            DrawBtnInfos();

            // Actions listenning
            ActionsListenning();

            // Modals listenning
            ModalsListenning();

            // Update cursor
            UpdateMouse();

            // Show FPS
            DrawFPS();

            // End draw
            Raylib.EndDrawing();
            Raylib.UnloadTexture(screenTexture);
        }

        // Draw FPS
        public static void DrawFPS()
        {
            if(Program.AppConfig.ShowFPS)
            Raylib.DrawFPS(Res(30), Res(30));
        }


        // Short useful methods
        // --------------------

        // Size screen
        public static void InitScreenSize()
        {
            ScreenWidth = Raylib.GetRenderWidth();
            ScreenHeight = Raylib.GetRenderHeight();
        }

        // Mouse
        public static void InitMouse()
        {
            Mouse = Raylib.GetMousePosition();
            Cursor = MouseCursor.MOUSE_CURSOR_DEFAULT;
        }

        public static void UpdateMouse()
        {
            Raylib.SetMouseCursor(Cursor);
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
    }
}