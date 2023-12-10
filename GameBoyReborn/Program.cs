// -------------
// Start program
// -------------
using Raylib_cs;

public class Program
{
    public const int SystemWidth = 160;
    public const int SystemHeight = 144;

    public static Task Main(string[] args)
    {
        // Debug log
        WriteLog().GetAwaiter().GetResult();

        // Set Raylib
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
        Raylib.InitWindow(800, 600, "GameBoyReborn");
        Raylib.SetTargetFPS(60);

        // Init screen image
        Drawing.ScreenImage = Raylib.GenImageColor(160, 144, Color.RAYWHITE);

        // Load game
        Emulation.Load("Tetris.gb");
        Emulation.Init();

        // Game loop
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Debug.UpdateTexts();
            Input.Set();
            Emulation.Loop();
            Drawing.Screen();
        }

        // Exit program
        Raylib.CloseWindow();
        return Task.CompletedTask;
    }

    // Write debug log
    private static async Task WriteLog()
    {
        string logFilePath = "log.txt";

        using (StreamWriter streamWriter = new StreamWriter(logFilePath))
        {
            Console.SetOut(streamWriter);
            Console.WriteLine("Starting the program...");

            await Task.Delay(1000);
            streamWriter.Close();
        }
    }
}