// ---------------
// Modal menu game
// ---------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int MenuGame_ModalWidth = 1024;
        private const int MenuGame_ModalHeight = 255;

        // Set textures
        // ------------

        private static void MenuGame_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("CloseGame", SingleToTexture("Quitter le jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("SaveGame", SingleToTexture("Sauvegarder l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("LoadGame", SingleToTexture("Charger l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void MenuGame_DrawComponents(string modal, Rectangle modalRect)
        {
            DrawText(ModalsTexture[modal]["CloseGame"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)));
            DrawText(ModalsTexture[modal]["SaveGame"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(110)));
            DrawText(ModalsTexture[modal]["LoadGame"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(170)));
        }

        // Set highlights
        // --------------

        private static void MenuGame_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "CloseGame", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(40)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "ComingSoon", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(100)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "ComingSoon", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(160)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
