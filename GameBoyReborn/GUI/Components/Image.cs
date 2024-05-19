// --------
// Checkbox
// --------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawImage(Texture2D texture, int X, int Y, int width, int height)
        {
            Vector2 pos = new() { X = X, Y = Y };
            texture.Width = width;
            texture.Height = height;

            Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.DrawTextureEx(texture, pos, 0, 1, Color.WHITE);
        }
    }
}
