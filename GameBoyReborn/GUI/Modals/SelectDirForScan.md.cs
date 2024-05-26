// -------------------------
// Modal select dir for scan
// -------------------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int SelectDirForScan_ModalWidth = 1024;
        private const int SelectDirForScan_ModalHeight = 768;

        // Set textures
        // ------------

        private static void SelectDirForScan_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("ComingSoon", SingleToTexture("Prochainement...", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void SelectDirForScan_DrawComponents(string modal, Rectangle modalRect)
        {
            Texture2D texture = ModalsTexture[modal]["ComingSoon"];
            DrawText(texture, Formulas.CenterElm(ScreenWidth, Res(texture.Width)), Formulas.CenterElm(ScreenHeight, Res(texture.Height)));
        }

        // Set highlights
        // --------------

        private static void SelectDirForScan_SetHighlights(string modal, Rectangle modalRect)
        {
            
        }
    }
}
