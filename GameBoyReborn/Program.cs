// -------------
// Start program
// -------------
using Raylib_cs;
using System.Runtime.InteropServices;

namespace GameBoyReborn
{
    public class Program
    {
        public const int SystemWidth = 160;
        public const int SystemHeight = 144;

        public static Task Main(string[] args)
        {
            // Log handle
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
                Log.Close();
            };

            // Set Raylib
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
            Raylib.InitWindow(800, 600, "GameBoyReborn");
            Raylib.SetTargetFPS(60);

            // Init screen image
            Drawing.ScreenImage = Raylib.GenImageColor(SystemWidth, SystemHeight, Color.RAYWHITE);

            // Load game
            if(args.Length > 0)
            Emulation.Load(args[0]);
            else
            Emulation.Load("Roms/Tetris.gb");
            Emulation.Init();
            Audio.Init();

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                Audio.Loop();
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