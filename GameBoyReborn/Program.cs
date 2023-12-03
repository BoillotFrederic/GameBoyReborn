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
        // Set Raylib
        Raylib.InitWindow(800, 600, "GameBoyReborn");
        Raylib.SetTargetFPS(60);

        // Init screen image
        Drawing.ScreenImage = Raylib.GenImageColor(160, 144, Color.RAYWHITE);

        // Game loop
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Debug.UpdateTexts();
            Input.Set();
            Drawing.Screen();
        }

        // Exit program
        Raylib.CloseWindow();
        return Task.CompletedTask;
    }
}