// --------
// Checkbox
// --------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawCheckbox(Texture2D texture, int X, int Y, int width, int height, bool isCheck)
        {
            Vector2 pos = new() { X = X, Y = Y };
            texture.Width = width;
            texture.Height = height;

            Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.DrawRectangle(X + 2, Y + 2, width - 4, height - 4, !isCheck ? Color.BLACK : Color.WHITE);
            Raylib.DrawTextureEx(texture, pos, 0, 1, Color.WHITE);
        }
    }
}
