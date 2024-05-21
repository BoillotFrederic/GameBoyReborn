// ---------
// SelectBox
// ---------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawSelectBox(Texture2D text, Texture2D selected, Texture2D icon, int X, int XLeft, int Y, ref Vector2 SelectBoxTextPos)
        {
            SelectBoxTextPos.X = XLeft;
            SelectBoxTextPos.Y = Y - Res(2);

            DrawText(text, X, Y);
            DrawText(selected, XLeft - Res(selected.Width + 15), Y - Res(2));
            DrawImage(icon, XLeft, Y, Res(40), Res(40));
        }
    }
}
