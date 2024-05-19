// ---------------
// Modal menu list
// ---------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int MenuList_ModalWidth = 1024;
        private const int MenuList_ModalHeight = 200;

        // Set textures
        // ------------

        private static void MenuList_SetTextures(string modal)
        {
            ModalTextures[modal].Add("MakeScan", SingleToTexture("Lancer un scan (ceci réinitialisera la liste)", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("CloseProgram", SingleToTexture("Fermer le programme", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void MenuList_DrawComponents(string modal, Rectangle modalRect)
        {
            DrawText(ModalTextures[modal]["MakeScan"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)));
            DrawText(ModalTextures[modal]["CloseProgram"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(110)));
        }

        // Set highlights
        // --------------

        private static void MenuList_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "PrepareScanList", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(40)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "CloseProgram", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(100)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
