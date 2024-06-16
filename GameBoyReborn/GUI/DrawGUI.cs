﻿// --------
// Draw GUI
// --------

using Raylib_cs;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Operating variables
        // -------------------

        private static Game[] GameListOrigin = Array.Empty<Game>();
        private static Game[] GameList = Array.Empty<Game>();
        private static Font MainFont = LoadFont(AppDomain.CurrentDomain.BaseDirectory + "Fonts/ErasBoldITC.ttf");
        private static Texture2D CartridgeGB = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/CartridgeGB.png");
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
            Raylib.SetTextureFilter(CartridgeGB, TextureFilter.TEXTURE_FILTER_BILINEAR);

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

        // Load font
        private static unsafe Font LoadFont(string path)
        {
            IntPtr ptrUtf8 = Marshal.StringToCoTaskMemUTF8(path);
            sbyte* sbytePtr = (sbyte*)ptrUtf8.ToPointer();
            Font fontTtf = Raylib.LoadFontEx(sbytePtr, 50, null, 250);
            Marshal.FreeCoTaskMem(ptrUtf8);

            return fontTtf;
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