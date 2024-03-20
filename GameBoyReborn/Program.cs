// -------------
// Start program
// -------------

#pragma warning disable CA2211

using Raylib_cs;
using Emulator;

namespace GameBoyReborn
{
    public class Program
    {
        public const int SystemWidth = 160;
        public const int SystemHeight = 144;
        public const int WindowWidth = 800;
        public const int WindowHeight = 600;
        public const bool DebugEnable = true;
        public static bool OneByOne = false;
        public const int DrawWidth = DebugEnable ? WindowWidth / 2 : WindowWidth;
        public const int DrawHeight = DebugEnable ? WindowHeight / 2 : WindowHeight;
        public static Emulation ? Emulation = null;

        public static Task Main(string[] args)
        {
            // Exit handle
            Log.Start();
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                Console.WriteLine($"Unhandled Exception: {ex}");
                Log.Close();
                Raylib.CloseWindow();
                Environment.Exit(1);
            };
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                if(Emulation != null)
                Emulation.SaveExternalRam();

                Log.Close();
            };

            // Set Raylib
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
            Raylib.InitWindow(800, 600, "GameBoyReborn");
            Raylib.SetTargetFPS(60);

            // Init screen image
            Drawing.ScreenImage = Raylib.GenImageColor(SystemWidth, SystemHeight, Color.RAYWHITE);

            // Init emulation
            Emulation = new Emulation(args.Length > 0 ? args[0] : "Roms/Tetris.gb");

            // Init audio
            Audio.Init();

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Input.Set();
                Emulation.Loop();
                Drawing.Screen();
            }

            // Exit program
            Log.Close();
            Audio.Close();
            Raylib.CloseWindow();

            return Task.CompletedTask;
        }
    }
}