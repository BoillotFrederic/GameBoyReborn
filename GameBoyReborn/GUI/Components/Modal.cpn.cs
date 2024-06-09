// -----
// Modal
// -----

using Raylib_cs;
using System.Reflection;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Operating variables
        private static VecInt2 ModalHighlightPos = new() { X = 0, Y = 0 };
        private static VecInt2 ModalHighlightLastPos = new() { X = 0, Y = 0 };
        private static readonly List<List<HighlightElm>> ModalHighlight = new();
        private static bool HighlightClicked = false;
        private static Rectangle? HighLightArea;

        // Structures
        private struct HighlightElm 
        {
            public string Action = "";
            public Rectangle ElmRect;
            public bool MouseHover = true;
        }

        private struct VecInt2
        {
            public int X;
            public int Y;
        }

        // Delegations
        private delegate void SetTextures(string modal);
        private delegate void DrawComponents(string modal, Rectangle modalRect);
        private delegate void SetHighlights(string modal, Rectangle modalRect);
        private static readonly Dictionary<string, SetTextures> ModalSetTextures = new();
        private static readonly Dictionary<string, DrawComponents> ModalDrawComponents = new();
        private static readonly Dictionary<string, SetHighlights> ModalSetHighlights = new();

        // Draw modal
        private static void DrawModal(string modal)
        {
            // Stop drawing
            if (ModalsLoop.ContainsKey(modal)) ModalsLoop[modal]++;
            else ModalsLoop.Add(modal, 0);
            bool DontDraw = ModalsLoop[modal] < 5;

            // Modal rectangle
            int width = Res(RefLight.GetIntByName(modal + "_ModalWidth", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static));
            int height = Res(RefLight.GetIntByName(modal + "_ModalHeight", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static));
            int top = RefLight.GetIntByName(modal + "_ModalTop", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
            int left = RefLight.GetIntByName(modal + "_ModalLeft", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
            int border = RefLight.GetIntByName(modal + "_ModalBorder", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);

            top = top != 0 ? top : Formulas.CenterElm(ScreenHeight, height);
            left = left != 0 ? left : Formulas.CenterElm(ScreenWidth, width);
            border = border == 0 ? Res(10) : border;

            Rectangle modalRect = new() { Width = width, Height = height, X = left, Y = top };

            if (DontDraw)
            Raylib.BeginScissorMode(0, 0, 0, 0);

            Raylib.DrawRectangleRec(modalRect, Color.WHITE);
            Raylib.DrawRectangleLinesEx(modalRect, border, Color.DARKGRAY);

            if (DontDraw)
            Raylib.EndScissorMode();

            // Init
            InitModalTextures(modal, modalRect);

            // Select line by hover and click handle
            int x = 0;
            int y = 0;

            if(ModalHighlight.Count > 0 && modal == WhereIAm)
            foreach (List<HighlightElm> selectLines in ModalHighlight)
            {
                foreach (HighlightElm selectLine in selectLines)
                {
                    Rectangle collisionArea = HighLightArea != null ? HighLightArea.Value : modalRect;

                    if(WhereIAm == "SelectBox")
                    {
                        collisionArea.Y += Res(70);
                        collisionArea.Height -= Res(70);
                    }

                    if(Raylib.CheckCollisionPointRec(Mouse, selectLine.ElmRect) && Raylib.CheckCollisionPointRec(Mouse, collisionArea))
                    {
                        if (selectLine.MouseHover)
                        Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;

                        ModalHighlightPos.X = x;
                        ModalHighlightPos.Y = y;

                        if (Input.Pressed("Click", Input.MouseLeftClick))
                        {
                            HighlightClicked = true;

                            if(WhereIAm != "SelectBox")
                            ModalHighlightLastPos = ModalHighlightPos;
                        }
                    }
                    x++;
                }
                x = 0;
                y++;
            }

            // Draw select line
            if(DontDraw)
            Raylib.BeginScissorMode(0, 0, 0, 0);
            else
            Raylib.BeginScissorMode((int)modalRect.X + 1, (int)modalRect.Y + 1, (int)modalRect.Width - 2, (int)modalRect.Height - 2);

            // High light special area
            if(HighLightArea != null)
            Raylib.BeginScissorMode((int)HighLightArea.Value.X, (int)HighLightArea.Value.Y, (int)HighLightArea.Value.Width, (int)HighLightArea.Value.Height);

            if(ModalHighlight.Count > 0)
            Raylib.DrawRectangleRec(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].ElmRect, Color.LIGHTGRAY);
            
            if(HighLightArea != null)
            Raylib.EndScissorMode();

            // Draw content
            if (ModalDrawComponents.ContainsKey(modal))
            ModalDrawComponents[modal].Invoke(modal, modalRect);
            Raylib.EndScissorMode();
        }
    }
}