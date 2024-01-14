// -------
// Drawing
// -------
using Raylib_cs;

namespace GameBoyReborn
{
    public static class Drawing
    {
        // Drawn screen data
        // -----------------
        private static readonly Color[] ScreenData = new Color[Program.SystemWidth * Program.SystemHeight];

        private static Image _ScreenImage;
        public static Image ScreenImage
        {
            get { return _ScreenImage; }
            set { _ScreenImage = value; }
        }

        // Draw screen
        public static void Screen()
        {
            // Clear
            Raylib.ClearBackground(Color.WHITE);

            // Draw
            UpdateScreenImage();
            Texture2D screenTexture = Raylib.LoadTextureFromImage(ScreenImage);
            screenTexture.Width = 800;
            screenTexture.Height = 600;
            Raylib.DrawTexture(screenTexture, 0, 0, Color.RAYWHITE);
            UpdateAndDrawTexts();
            //ShowXboxButton();

            Raylib.EndDrawing();
            Raylib.UnloadTexture(screenTexture);
        }

        // Update screen image
        private static void UpdateScreenImage()
        {
            unsafe
            {
                fixed (Color* pData = &ScreenData[0])
                {
                    _ScreenImage.Data = pData;
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

        // Disable screen
        public static void DisableScreen()
        {
            Array.Clear(ScreenData);
        }

        // Draw text
        // ---------
        private readonly static List<TextStruct> TextQueue = new();
        private readonly static List<int> TextToRemove = new();

        // Text structure
        private class TextStruct
        {
            public string text = "";
            public int remainingTime;
            public Color color;
        }

        // Add text to the queue
        public static void Text(string text, Color color, int remainingTime)
        {
            TextStruct newLine = new()
            {
                text = text,
                remainingTime = remainingTime,
                color = color
            };

            TextQueue.Add(newLine);
        }

        // Update and draw texts
        private static void UpdateAndDrawTexts()
        {
            TextToRemove.Clear();

            for (int i = 0; i < TextQueue.Count; i++)
            {
                Raylib.DrawText(TextQueue[i].text, 10, 10 + (i * 20), 20, TextQueue[i].color);
                TextQueue[i].remainingTime -= 16;

                if (TextQueue[i].remainingTime <= 0)
                TextToRemove.Add(i);
            }

            for (int i = TextToRemove.Count - 1; i >= 0; i--)
            {
                int index = TextToRemove[i];

                if (index >= 0 && index < TextQueue.Count)
                TextQueue.RemoveAt(index);
            }
        }

        // Show PAD button (debug)
        private static void ShowXboxButton()
        {
            // Show keys/buttons
            // -----------------

            // D-PAD
            Raylib.DrawText("D-PAD : Down : " + Input.DPadDown, 10, 10, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Left : " + Input.DPadLeft, 10, 30, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Right : " + Input.DPadRight, 10, 50, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Up : " + Input.DPadUp, 10, 70, 20, Color.BLUE);

            // XABY-PAD
            Raylib.DrawText("XABY-PAD : A : " + Input.XabyPadA, 10, 90, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : B : " + Input.XabyPadB, 10, 110, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : X : " + Input.XabyPadX, 10, 130, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : Y : " + Input.XabyPadY, 10, 150, 20, Color.BLUE);

            // MIDDLE-PAD
            Raylib.DrawText("MIDDLE-PAD : Back : " + Input.MiddlePadLeft, 10, 170, 20, Color.BLUE);
            Raylib.DrawText("MIDDLE-PAD : Start : " + Input.MiddlePadRight, 10, 190, 20, Color.BLUE);
            Raylib.DrawText("MIDDLE-PAD : Center : " + Input.MiddlePadCenter, 10, 210, 20, Color.BLUE);

            // TRIGGER-PAD
            Raylib.DrawText("TRIGGER-PAD : LB : " + Input.TriggerPadLB, 10, 230, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : RB : " + Input.TriggerPadRB, 10, 250, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : LT : " + Input.TriggerPadLT, 10, 270, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : RT : " + Input.TriggerPadRT, 10, 290, 20, Color.BLUE);

            // AXIS-PAD
            Raylib.DrawText("AXIS-PAD : LEFT Down : " + Input.AxisLeftPadDown, 10, 310, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Left : " + Input.AxisLeftPadLeft, 10, 330, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Right : " + Input.AxisLeftPadRight, 10, 350, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Up : " + Input.AxisLeftPadUp, 10, 370, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Down : " + Input.AxisRightPadDown, 10, 390, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Left : " + Input.AxisRightPadLeft, 10, 410, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Right : " + Input.AxisRightPadRight, 10, 430, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Up : " + Input.AxisRightPadUp, 10, 450, 20, Color.BLUE);
        }
    }
}
