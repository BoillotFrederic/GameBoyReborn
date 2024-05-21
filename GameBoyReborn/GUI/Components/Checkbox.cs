// --------
// Checkbox
// --------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawCheckbox(Texture2D text, Texture2D checkbox, int X, int XLeft, int Y, int width, int height, bool isCheck)
        {
            Vector2 pos = new() { X = XLeft, Y = Y };
            checkbox.Width = width;
            checkbox.Height = height;

            Raylib.SetTextureFilter(checkbox, TextureFilter.TEXTURE_FILTER_BILINEAR);

            DrawText(text, X, Y);
            Raylib.DrawRectangle(XLeft + 2, Y + 2, width - 4, height - 4, !isCheck ? Color.BLACK : Color.WHITE);
            Raylib.DrawTextureEx(checkbox, pos, 0, 1, Color.WHITE);
        }
    }
}
