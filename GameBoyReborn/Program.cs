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
        public static bool EmulatorRun = false;
        public static int WindowWidth = 1024;
        public static int WindowHeight = 768;
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
                Emulation.SaveExternal();

                Log.Close();
            };

            // Set Raylib
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
            Raylib.InitWindow(WindowWidth, WindowHeight, "GameBoyReborn");
            Raylib.SetTargetFPS(60);

            // Init emulation
            if(args.Length > 0)
            {
                EmulatorRun = true;
                Emulation = new Emulation(args[0]);
                 Audio.Init();
            }

            // Init Metro GB
            DrawingGUI.InitMetroGB();

            // Game loop
            while (!Raylib.WindowShouldClose())
            {
                if(EmulatorRun)
                DrawingGB.Screen();
                else
                DrawingGUI.MetroGB();

                Input.Update();
            }

            // Exit program
            Log.Close();
            Audio.Close();
            Raylib.CloseWindow();

            return Task.CompletedTask;
        }
    }
}