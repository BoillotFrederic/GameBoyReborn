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
        private static Vector2 OpenSelectBoxHookTagPos = new();
        private static Vector2 OpenSelectBoxBracketsTagPos = new();

        // Set textures
        // ------------

        private static void PrepareScanList_SetTextures(string modal)
        {
            // Image
            ModalTextures[modal].Add("Checkbox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Checkbox.png"));
            ModalTextures[modal].Add("Folder", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Folder.png"));
            ModalTextures[modal].Add("SelectBox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/SelectBox.png"));

            // Text
            ModalTextures[modal].Add("Dir", SingleToTexture("Dossier", 45.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("DirSelected", SingleToTexture("Aucun", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("Recursive", SingleToTexture("Scanner les sous-dossiers", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("Zip", SingleToTexture("Archive", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("HookTag", SingleToTexture("Tag [] privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("BracketsTag", SingleToTexture("Tag () privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("BracketsTagSelect", SingleToTexture("(E)", 40.0f * TextResolution, 3.0f, Color.BLACK));

            // Select box hook tag
            ModalTextures[modal].Add("HookTagSelectBlack", SingleToTexture("[!] vérifié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("HookTagSelectWhite", SingleToTexture("[!] vérifié", 40.0f * TextResolution, 3.0f, Color.WHITE));
            ModalTextures[modal].Add("[!]", SingleToTexture("[!] vérifié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[a]", SingleToTexture("[a] alternative", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[c]", SingleToTexture("[c] cracké", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[f]", SingleToTexture("[f] fixé", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[h]", SingleToTexture("[h] hacké", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[p]", SingleToTexture("[p] piraté", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("[T]", SingleToTexture("[T] traduit", 40.0f * TextResolution, 3.0f, Color.BLACK));

            // Select box hook tag
            ModalTextures[modal].Add("BracketsTagSelectBlack", SingleToTexture("(E) Europe", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("BracketsTagSelectWhite", SingleToTexture("(E) Europe", 40.0f * TextResolution, 3.0f, Color.WHITE));
            ModalTextures[modal].Add("(E)", SingleToTexture("(E) Europe", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("(U)", SingleToTexture("(U) USA", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("(J)", SingleToTexture("(J) Japon", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("(F)", SingleToTexture("(F) France", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("(G)", SingleToTexture("(G) Allemagne", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("(I)", SingleToTexture("(I) Italie", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void PrepareScanList_DrawComponents(string modal, Rectangle modalRect)
        {
            // Short access
            Texture2D Checkbox = ModalTextures[modal]["Checkbox"];
            Texture2D Folder = ModalTextures[modal]["Folder"];
            Texture2D SelectBox = ModalTextures[modal]["SelectBox"];
            Texture2D Dir = ModalTextures[modal]["Dir"];
            Texture2D DirSelected = ModalTextures[modal]["DirSelected"];
            Texture2D Recursive = ModalTextures[modal]["Recursive"];
            Texture2D Zip = ModalTextures[modal]["Zip"];
            Texture2D HookTag = ModalTextures[modal]["HookTag"];
            Texture2D HookTagSelect = ModalTextures[modal]["HookTagSelectBlack"];
            Texture2D BracketsTag = ModalTextures[modal]["BracketsTag"];
            Texture2D BracketsTagSelect = ModalTextures[modal]["BracketsTagSelect"];
            int cX(int interger) { return (int)(modalRect.X + interger); }
            int cY(int interger) { return (int)(modalRect.Y + interger); }

            // Draw dir section
            DrawTitle(Dir, cX(Res(50)), cY(Res(50)), Res(5), Color.BLACK);
            DrawTextWithImage(DirSelected, Folder, cX(Res(90)), cX((int)modalRect.Width - Res(120)), cY(Res(140)));
            DrawCheckbox(Recursive, Checkbox, cX(Res(90)), cX((int)modalRect.Width - Res(122)), cY(Res(200)), Res(40), Res(40), true);

            // Draw zip section
            DrawTitle(Zip, cX(Res(50)), cY(Res(280)), Res(5), Color.BLACK);
            DrawSelectBox(HookTag, HookTagSelect, SelectBox, cX(Res(90)), cX((int)modalRect.Width - Res(120)), cY(Res(370)), ref OpenSelectBoxHookTagPos);
            DrawSelectBox(BracketsTag, BracketsTagSelect, SelectBox, cX(Res(90)), cX((int)modalRect.Width - Res(120)), cY(Res(430)), ref OpenSelectBoxBracketsTagPos);
        }

        // Set highlights
        // --------------

        private static void PrepareScanList_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(130)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "-", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(190)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "OpenSelectBoxHookTag", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(360)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "OpenSelectBoxBracketsTag", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(420)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
