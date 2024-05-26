// -----------------
// Modal coming soon
// -----------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int ComingSoon_ModalWidth = 800;
        private const int ComingSoon_ModalHeight = 150;

        // Set textures
        // ------------

        private static void ComingSoon_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("ComingSoon", SingleToTexture("Prochainement...", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void ComingSoon_DrawComponents(string modal, Rectangle modalRect)
        {
            Texture2D texture = ModalsTexture[modal]["ComingSoon"];
            DrawText(texture, Formulas.CenterElm(ScreenWidth, Res(texture.Width)), Formulas.CenterElm(ScreenHeight, Res(texture.Height)));
        }

        // Set highlights
        // --------------

        private static void ComingSoon_SetHighlights(string modal, Rectangle modalRect)
        {
            
        }
    }
}
