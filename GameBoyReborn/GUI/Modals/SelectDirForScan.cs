// -------------------------
// Modal select dir for scan
// -------------------------

using Raylib_cs;
using System.Numerics;

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
            ModalTextures[modal].Add("ComingSoon", SingleToTexture("Prochainement...", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void SelectDirForScan_DrawComponents(string modal, Rectangle modalRect)
        {
            // Draw coming soon
            Texture2D comingSoon = ModalTextures[modal]["ComingSoon"];
            Vector2 comingSoonPos = new()
            { 
                X = modalRect.X + Formulas.CenterElm((int)modalRect.Width, Res(comingSoon.Width)), 
                Y = modalRect.Y + Formulas.CenterElm((int)modalRect.Height, Res(comingSoon.Height))
            };
            Raylib.DrawTextureEx(comingSoon, comingSoonPos, 0, Res(comingSoon.Width) / (float)comingSoon.Width, Color.WHITE);
        }

        // Set highlights
        // --------------

        private static void SelectDirForScan_SetHighlights(string modal, Rectangle modalRect)
        {
            
        }
    }
}
