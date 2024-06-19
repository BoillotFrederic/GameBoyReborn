// -----------------------
// Modal setting this game
// -----------------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int SettingThisGame_ModalWidth = 1024;
        private const int SettingThisGame_ModalHeight = 255;

        // Set textures
        // ------------

        private static void SettingThisGame_SetTextures(string modal)
        {
            ModalsTexture[modal].Add("FileBin", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/FileBin.png"));
            ModalsTexture[modal].Add("ChangeZippedFile", SingleToTexture("Archive : fichier à émuler", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("ZippedFilesSelected", SingleToTexture(TruncateTextWithEllipsis(GameList[MouseLeftClickTarget].ZippedFile, 40.0f * TextResolution, 3.0f, 1024 - 230), 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void SettingThisGame_DrawComponents(string modal, Rectangle modalRect)
        {
            // Short access
            Texture2D Folder = ModalsTexture[modal]["FileBin"];
            Texture2D ChangeZippedFile = ModalsTexture[modal]["ChangeZippedFile"];
            Texture2D ZippedFilesSelected = ModalsTexture[modal]["ZippedFilesSelected"];
            int cX(int interger) { return (int)(modalRect.X + interger); }
            int cY(int interger) { return (int)(modalRect.Y + interger); }

            // Draw dir section
            DrawTitle(ChangeZippedFile, cX(Res(50)), cY(Res(50)), Res(5), Color.BLACK);
            DrawTextWithImage(ZippedFilesSelected, Folder, cX(Res(90)), cX((int)modalRect.Width - Res(120)), cY(Res(140)));
        }

        // Set highlights
        // --------------

        private static void SettingThisGame_SetHighlights(string modal, Rectangle modalRect)
        {
            SetHighLight(modal, "GameSelectZippedFile", true, (int)(modalRect.X + Res(40)), (int)(modalRect.Y + Res(130)), (int)(modalRect.Width - Res(80)), Res(60));
        }
    }
}
