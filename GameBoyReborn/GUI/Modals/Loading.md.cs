// -------------
// Modal loading
// -------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int Loading_ModalWidth = 768;
        private const int Loading_ModalHeight = 250;
        private static string Loading_Text = "";
        private static float Loading_Percent = 0;

        // Set textures
        // ------------

        private static void Loading_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("LoadingText", SingleToTexture(Loading_Text, 40.0f * TextResolution, 3.0f, Color.BLACK)); 
        }

        // Draw components
        // ---------------

        private static void Loading_DrawComponents(string modal, Rectangle modalRect)
        {
            // Text
            int loadingTextPosX = (int)(modalRect.X + Formulas.CenterElm((int)modalRect.Width, Res(ModalsTexture[modal]["LoadingText"].Width)));
            DrawText(ModalsTexture[modal]["LoadingText"], loadingTextPosX, (int)(modalRect.Y + Res(50)));

            // Progress bar
            Rectangle progressBarRect = new()
            {
                X = (int)(modalRect.X + Res(50)),
                Y = (int)(modalRect.Y + Res(120)),
                Width = (int)(modalRect.Width - Res(100)),
                Height = Res(90)
            };

            Raylib.DrawRectangle((int)progressBarRect.X, (int)progressBarRect.Y, (int)(progressBarRect.Width * Loading_Percent), (int)progressBarRect.Height, Color.DARKGREEN);
            Raylib.DrawRectangleLinesEx(progressBarRect, 1, Color.BLACK);
        }
    }
}
