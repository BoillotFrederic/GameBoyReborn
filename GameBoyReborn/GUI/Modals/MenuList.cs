// ---------------
// Modal menu list
// ---------------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int MenuList_ModalWidth = 1024;
        private const int MenuList_ModalHeight = 250;

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
            // Draw make scan
            Texture2D makeScan = ModalTextures[modal]["MakeScan"];
            Vector2 makeScanPos = new() {  X = modalRect.X + Res(50), Y = modalRect.Y + Res(50) };
            Raylib.DrawTextureEx(makeScan, makeScanPos, 0, Res(makeScan.Width) / (float)makeScan.Width, Color.WHITE);

            // Draw close program
            Texture2D closeProgram = ModalTextures[modal]["CloseProgram"];
            Vector2 closeProgramPos = new() { X = modalRect.X + Res(50), Y = modalRect.Y + Res(150) };
            Raylib.DrawTextureEx(closeProgram, closeProgramPos, 0, Res(closeProgram.Width) / (float)closeProgram.Width, Color.WHITE);
        }

        // Set highlights
        // --------------

        private static void MenuList_SetHighlights(string modal, Rectangle modalRect)
        {
            List<HighlightElm> line1 = new();
            line1.Add(new HighlightElm() { Action = "SelectDirForScan", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(40), Width = modalRect.Width - Res(80), Height = Res(60) } });
            ModalHighlight.Add(line1);

            List<HighlightElm> line2 = new();
            line2.Add(new HighlightElm() { Action = "-", ElmRect = new() { X = modalRect.X + Res(40), Y = modalRect.Y + Res(140), Width = modalRect.Width - Res(80), Height = Res(60) } });
            ModalHighlight.Add(line2);
        }
    }
}
