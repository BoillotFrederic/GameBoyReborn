// ---------------
// Modal menu game
// ---------------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int MenuGame_ModalWidth = 1024;
        private const int MenuGame_ModalHeight = 345;

        // Set textures
        // ------------

        private static void MenuGame_SetTextures(string modal)
        {
            ModalTextures[modal].Add("CloseGame", SingleToTexture("Quitter le jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("SaveGame", SingleToTexture("Sauvegarder l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalTextures[modal].Add("LoadGame", SingleToTexture("Charger l'état du jeu", 40.0f * TextResolution, 3.0f, Color.BLACK));
        }

        // Draw components
        // ---------------

        private static void MenuGame_DrawComponents(string modal, Rectangle modalRect)
        {
            // Draw menu game
            Texture2D closeGame = ModalTextures[modal]["CloseGame"];
            Texture2D saveGame = ModalTextures[modal]["SaveGame"];
            Texture2D loadGame = ModalTextures[modal]["LoadGame"];

            Vector2 closeGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(50) };
            Raylib.DrawTextureEx(closeGame, closeGamePos, 0, Res(closeGame.Width) / (float)closeGame.Width, Color.WHITE);

            Vector2 saveGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(150) };
            Raylib.DrawTextureEx(saveGame, saveGamePos, 0, Res(saveGame.Width) / (float)saveGame.Width, Color.WHITE);

            Vector2 loadGamePos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(250) };
            Raylib.DrawTextureEx(loadGame, loadGamePos, 0, Res(loadGame.Width) / (float)loadGame.Width, Color.WHITE);
        }

        // Set highlights
        // --------------

        private static void MenuGame_SetHighlights(string modal, Rectangle modalRect)
        {
            List<HighlightElm> line1 = new();
            line1.Add(new HighlightElm() { Action = "CloseGame", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(40), Width = modalRect.Width - Res(80), Height = Res(60) } });
            ModalHighlight.Add(line1);

            List<HighlightElm> line2 = new();
            line2.Add(new HighlightElm() { Action = "-", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(140), Width = modalRect.Width - Res(80), Height = Res(60) } });
            ModalHighlight.Add(line2);

            List<HighlightElm> line3 = new();
            line3.Add(new HighlightElm() { Action = "-", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(240), Width = modalRect.Width - Res(80), Height = Res(60) } });
            ModalHighlight.Add(line3);
        }
    }
}
