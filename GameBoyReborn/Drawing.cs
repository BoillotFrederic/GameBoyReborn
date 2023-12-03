// -------
// Drawing
// -------
using Raylib_cs;

public static class Drawing
{
    private static bool FirstLoop = true;
    public static Image ScreenImage;
    public static Color[] ScreenData = new Color[Program.SystemWidth * Program.SystemHeight];

    // Draw screen
    public static void Screen()
    {
        // Clear
        Raylib.ClearBackground(Color.RAYWHITE);

        // First background
        if (FirstLoop)
        {
            FirstLoop = false;
            //Debug.Text(screenImage.Width.ToString(), Color.RED, 5000);
        }

 /*       SetPixel(100, 50, Color.BLACK);
        SetPixel(101, 50, Color.BLACK);
        SetPixel(120, 60, Color.BLACK);*/

        // Draw
        UpdateScreenImage();
        Texture2D screenTexture = Raylib.LoadTextureFromImage(ScreenImage);
        screenTexture.Width = 800;
        screenTexture.Height = 600;
        Raylib.DrawTexture(screenTexture, 0, 0, Color.WHITE);

        Raylib.EndDrawing();
        Raylib.UnloadTexture(screenTexture);
    }

    // Update screen image
    private static void UpdateScreenImage()
    {
        /*
        var rand = new Random();

        for (int p = 0; p < Program.SystemWidth * Program.SystemHeight; p++)
        {
            Color newColor = new Color((byte)rand.Next(0, 60), (byte)rand.Next(0, 60), (byte)rand.Next(0, 60), (byte)255);
            screenData[p] = newColor;
        }
        */

        unsafe
        {
            fixed (Color* pData = &ScreenData[0])
            {
                ScreenImage.Data = pData;
            }
        }
    }

    // Set pixel
    public static void SetPixel(byte x, byte y, Color color)
    {
        ScreenData[y * Program.SystemWidth + x] = color;
    }

    // Get pixel
    public static Color GetPixel(byte x, byte y)
    {
        return ScreenData[y * Program.SystemWidth + x];
    }
}
