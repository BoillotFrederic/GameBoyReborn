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
            ModalsTexture[modal].Add("Checkbox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Checkbox.png"));
            ModalsTexture[modal].Add("Folder", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Folder.png"));
            ModalsTexture[modal].Add("SelectBox", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/SelectBox.png"));

            // Text
            ModalsTexture[modal].Add("Dir", SingleToTexture("Dossier", 45.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("DirSelected", SingleToTexture("Aucun", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("Recursive", SingleToTexture("Scanner les sous-dossiers", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("Zip", SingleToTexture("Archive", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("HookTag", SingleToTexture("Tag [] privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("BracketsTag", SingleToTexture("Tag () privilégié", 40.0f * TextResolution, 3.0f, Color.BLACK));

            // Select box hook tag
            Dictionary<string, string> hookTagSelectBoxText = new() 
            { 
                { "[!]", "vérifié" }, 
                { "[! p]", "presque bon" }, 
                { "[a]", "alternative" }, 
                { "[b]", "mauvais" }, 
                { "[c]", "cracké" }, 
                { "[C]", "Gameboy Color" }, 
                { "[f]", "fixé" }, 
                { "[h]", "hacké" }, 
                { "[o]", "taille dépassée" },
                { "[p]", "piraté" }, 
                { "[S]", "Super Gameboy" }, 
                { "[t]", "menu de triche" }, 
                { "[T]", "traduit" },
                { "[T +]", "nouvelle traduction" },
                { "[T-]", "ancienne traduction" },
                { "[x]", "checksum incorrect" },
                { "[###]", "checksum" },
            };
            foreach(string val in new string[] { "[!]", "[! p]", "[a]", "[b]", "[c]", "[C]", "[f]", "[h]", "[o]", "[p]", "[S]", "[t]", "[T]", "[T +]", "[T-]", "[x]", "[###]" })
            ModalsTexture[modal].Add(val, SingleToTexture(val + " " + hookTagSelectBoxText[val], 40.0f * TextResolution, 3.0f, Color.BLACK));

            string hookSelected = Program.AppConfig.HookTagPriority;
            ModalsTexture[modal].Add("HookTagSelectBlack", ModalsTexture[modal][hookSelected]);
            ModalsTexture[modal].Add("HookTagSelectWhite", SingleToTexture(hookSelected + " " + hookTagSelectBoxText[hookSelected], 40.0f * TextResolution, 3.0f, Color.WHITE));

            // Select box brackets tag
            Dictionary<string, string> bracketsTagSelectBoxText = new()
            { 
                { "(E)", "Europe" }, 
                { "(U)", "USA" }, 
                { "(J)", "Japon" }, 
                { "(F)", "France" }, 
                { "(G)", "Allemagne" }, 
                { "(H)", "Pays-Bas" }, 
                { "(I)", "Italie" },
                { "(R)", "Russie" },
                { "(S)", "Espagne" },
                { "(Unl)", "Sans licence" },
            };
            foreach(string val in new string[] { "(E)", "(U)", "(J)", "(F)", "(G)", "(H)", "(I)", "(R)", "(S)", "(Unl)" })
            ModalsTexture[modal].Add(val, SingleToTexture(val + " " + bracketsTagSelectBoxText[val], 40.0f * TextResolution, 3.0f, Color.BLACK));

            string bracketsSelected = Program.AppConfig.BracketsTagPriority;
            ModalsTexture[modal].Add("BracketsTagSelectBlack", ModalsTexture[modal][bracketsSelected]);
            ModalsTexture[modal].Add("BracketsTagSelectWhite", SingleToTexture(bracketsSelected + " " + bracketsTagSelectBoxText[bracketsSelected], 40.0f * TextResolution, 3.0f, Color.WHITE));
        }

        // Draw components
        // ---------------

        private static void PrepareScanList_DrawComponents(string modal, Rectangle modalRect)
        {
            // Short access
            Texture2D Checkbox = ModalsTexture[modal]["Checkbox"];
            Texture2D Folder = ModalsTexture[modal]["Folder"];
            Texture2D SelectBox = ModalsTexture[modal]["SelectBox"];
            Texture2D Dir = ModalsTexture[modal]["Dir"];
            Texture2D DirSelected = ModalsTexture[modal]["DirSelected"];
            Texture2D Recursive = ModalsTexture[modal]["Recursive"];
            Texture2D Zip = ModalsTexture[modal]["Zip"];
            Texture2D HookTag = ModalsTexture[modal]["HookTag"];
            Texture2D HookTagSelect = ModalsTexture[modal]["HookTagSelectBlack"];
            Texture2D BracketsTag = ModalsTexture[modal]["BracketsTag"];
            Texture2D BracketsTagSelect = ModalsTexture[modal]["BracketsTagSelectBlack"];
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
            SetHighLight(modal, "SetScanListRecursive", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(190)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "OpenSelectBoxHookTag", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(360)), (int)(modalRect.Width - Res(80)), Res(60));
            SetHighLight(modal, "OpenSelectBoxBracketsTag", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(420)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
