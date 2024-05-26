// -----
// Title
// -----

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void DrawTitle(Texture2D texture, int X, int Y, int underLineHeight, Color underLineColor)
        {
            Vector2 texutrePos = new() { X = X, Y = Y };
            Vector2 lineStartPos = new() { X = texutrePos.X, Y = texutrePos.Y + Res(texture.Height) };
            Vector2 lineEndPos = new() { X = texutrePos.X + Res(texture.Width), Y = texutrePos.Y + Res(texture.Height) };

            Raylib.SetTextureFilter(texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            Raylib.DrawTextureEx(texture, texutrePos, 0, Res(texture.Width) / (float)texture.Width, Color.WHITE);
            Raylib.DrawLineEx(lineStartPos, lineEndPos, underLineHeight, underLineColor);
        }
    }
}
