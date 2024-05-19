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

        private const int PrepareScanList_ModalWidth = 1024;
        private const int PrepareScanList_ModalHeight = 520;

        // Set textures
        // ------------

        private static void PrepareScanList_SetTextures(string modal)
        {
            // Image
            ModalTextures[modal].Add("Checkbox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Checkbox.png"));
            ModalTextures[modal].Add("Folder", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Folder.png"));

            // Text
            ModalTextures[modal].Add("Dir", SingleToTexture("Dossier", 45.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("DirSelected", SingleToTexture("Aucun", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("Recursive", SingleToTexture("Scanner les sous-dossiers", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("Zip", SingleToTexture("Archive", 45.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("Archive", SingleToTexture("Dossier à scanner", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("HookTag", SingleToTexture("Tag [] privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("HookTagSelect", SingleToTexture("[!]", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("BracketsTag", SingleToTexture("Tag () privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("BracketsTagSelect", SingleToTexture("(E)", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void PrepareScanList_DrawComponents(string modal, Rectangle modalRect)
        {
            // Draw dir section
            DrawTitle(ModalTextures[modal]["Dir"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)), Res(5), Color.BLACK);

            DrawText(ModalTextures[modal]["DirSelected"], (int)(modalRect.X + Res(90)), (int)(modalRect.Y + Res(140)));
            DrawImage(ModalTextures[modal]["Folder"], (int)(modalRect.X + modalRect.Width - Res(120)), (int)(modalRect.Y + Res(140)), Res(40), Res(40));
            DrawText(ModalTextures[modal]["Recursive"], (int)(modalRect.X + Res(90)), (int)(modalRect.Y + Res(200)));
            DrawCheckbox(ModalTextures[modal]["Checkbox"], (int)(modalRect.X + modalRect.Width - Res(122)), (int)(modalRect.Y + Res(200)), Res(40), Res(40), true);

            // Draw zip section
            DrawTitle(ModalTextures[modal]["Zip"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(280)), Res(5), Color.BLACK);

            DrawText(ModalTextures[modal]["HookTag"], (int)(modalRect.X + Res(90)), (int)(modalRect.Y + Res(370)));
            DrawText(ModalTextures[modal]["BracketsTag"], (int)(modalRect.X + Res(90)), (int)(modalRect.Y + Res(430)));

        }

        // Set highlights
        // --------------

        private static void PrepareScanList_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(130)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(190)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(360)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(420)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
