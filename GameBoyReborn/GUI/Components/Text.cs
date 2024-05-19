// ----
// Text
// ----

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawText(Texture2D texture, int X, int Y)
        {
            Vector2 pos = new() { X = X, Y = Y };
            Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.DrawTextureEx(texture, pos, 0, Res(texture.Width) / (float)texture.Width, Color.WHITE);
        }
    }
}
