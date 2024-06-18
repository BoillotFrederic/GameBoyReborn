// -------------
// Start program
// -------------

using Raylib_cs;
using Emulator;
using System.Runtime.InteropServices;

namespace GameBoyReborn
{
    public class Program
    {
        // Setting
        // -------

        public static readonly dynamic AppConfig = ConfigJson.LoadAppConfig();
        public static bool EmulatorRun { get; set; } = false;
        public static int WindowWidth { get; set; } = 1024;
        public static int WindowHeight { get; set; } = 768;
        public static Emulation ? Emulation { get; set; } = null;
        public static int FPSMax { get; set; } = 0;

        // Main task
        // ---------

        public static Task Main(string[] args)
        {
            // Exit handle
            Log.Start();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                Log.Write($"Unhandled Exception: {ex}");
                Log.Close();
                Raylib.CloseWindow();
                Environment.Exit(1);
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                if(Emulation != null)
                Emulation.SaveExternal();

                Log.Close();
            };

            // Console hidden
            if(!AppConfig.ShowConsole)
            HideConsole();

            // Set Raylib
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
            Raylib.InitWindow(WindowWidth, WindowHeight, "GameBoyReborn");

            // Set FPS
            FPSMax = GetScreenRefreshRate();
            Raylib.SetTargetFPS(FPSMax);

            // Start emulation by drag and drop
            if(args.Length > 0)
            {
                var game = new Game() { Name = DrawGUI.GetLastFolderName(args[0]) ?? "", Path = args[0] };
                Emulation.Start(game);
            }

            // Init Metro GB
            DrawGUI.InitMetroGB();

            // In fullscreen
            if (AppConfig.FullScreen)
            ToogleFullScreen();

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                if(EmulatorRun && Emulation != null && !Emulation.MenuIsOpen)
                DrawGB.Screen();
                else if(!EmulatorRun)
                DrawGUI.MetroGB();

                Input.Update();
            }

            // Exit program
            Log.Close();
            Audio.Close();
            Raylib.CloseWindow();

            return Task.CompletedTask;
        }

        // Toogle fullscreen
        // -----------------

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int DESKTOPVERTRES = 117;
        private const int DESKTOPHORZRES = 118;
        private static int WidthBeforeFullscreen = WindowWidth;
        private static int HeightBeforeFullscreen = WindowHeight;

        public static void ToogleFullScreen()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            int width = GetDeviceCaps(hdc, DESKTOPHORZRES);
            int height = GetDeviceCaps(hdc, DESKTOPVERTRES);

            if (!Raylib.IsWindowFullscreen())
            {
                WidthBeforeFullscreen = Raylib.GetScreenWidth();
                HeightBeforeFullscreen = Raylib.GetScreenHeight();
                Raylib.SetWindowSize(width, height);
                Raylib.ToggleFullscreen();
            }
            else
            {
                Raylib.ToggleFullscreen();
                Raylib.SetWindowSize(WidthBeforeFullscreen, HeightBeforeFullscreen);
            }
        }

        // Refresh rate
        // ------------

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int VREFRESH = 116;

        public static int GetScreenRefreshRate()
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get device context.");

            int refreshRate = GetDeviceCaps(hdc, VREFRESH);
            _ = ReleaseDC(IntPtr.Zero, hdc);

            return refreshRate;
        }

        // Display console
        // ---------------

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void ShowConsole()
        {
            AllocConsole();
            Log.Init();
            Log.ConsoleEnable = true;
        }

        public static void HideConsole()
        {
            FreeConsole();
            Log.ConsoleEnable = false;
        }
    }
}