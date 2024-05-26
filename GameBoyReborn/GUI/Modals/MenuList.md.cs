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
        private const int MenuList_ModalHeight = 320;

        // Set textures
        // ------------

        private static void MenuList_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("MakeScan", SingleToTexture("Préparer le scan", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("StartFullScreen", SingleToTexture("Affichage en plein écran", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("ShowFPS", SingleToTexture("Afficher les FPS", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("CloseProgram", SingleToTexture("Fermer le programme", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("Checkbox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Checkbox.png"));
        }

        // Draw components
        // ---------------

        private static void MenuList_DrawComponents(string modal, Rectangle modalRect)
        {
            DrawText(ModalsTexture[modal]["MakeScan"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)));
            DrawCheckbox(ModalsTexture[modal]["StartFullScreen"], ModalsTexture[modal]["Checkbox"], (int)(modalRect.X + Res(50)), (int)(modalRect.X + modalRect.Width - Res(100)), (int)(modalRect.Y + Res(110)), Res(40), Res(40), Program.AppConfig.FullScreen);
            DrawCheckbox(ModalsTexture[modal]["ShowFPS"], ModalsTexture[modal]["Checkbox"], (int)(modalRect.X + Res(50)), (int)(modalRect.X + modalRect.Width - Res(100)), (int)(modalRect.Y + Res(170)), Res(40), Res(40), Program.AppConfig.ShowFPS);
            DrawText(ModalsTexture[modal]["CloseProgram"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(230)));
        }

        // Set highlights
        // --------------

        private static void MenuList_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "PrepareScanList", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(40)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "SetFullScreen", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(100)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "SetShowFPS", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(160)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "CloseProgram", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(220)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
