// ----------
// Select box
// ----------

#pragma warning disable CS0414

using Raylib_cs;
using System.Numerics;
using System.Reflection;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private static int SelectBoxOpen_ModalWidth = 0;
        private static int SelectBoxOpen_ModalHeight = 0;
        private static int SelectBoxOpen_ModalTop = 0;
        private static int SelectBoxOpen_ModalLeft = 0;
        private static int SelectBoxOpen_ModalBorder;
        private static string SelectBoxOpen_name = "";
        private static Texture2D SelectBoxOpen_TextSelected = new();
        private static readonly List<SelectBox> SelectBoxOpen_Item = new();

        struct SelectBox
        {
            public string Value;
            public Texture2D Texture;
        }

        // Draw components
        // ---------------

        private static void SelectBoxOpen_DrawComponents(string modal, Rectangle modalRect)
        {
            // Variables
            Vector2 BoxPos = RefLight.GetVec2ByName(SelectBoxOpen_name + "Pos", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
            int y = Res(15);
            int x = Res(30);
            int width = 0;
            int height = 20;

            // Draw rectangle selected
            Raylib.DrawRectangle((int)BoxPos.X - Res(SelectBoxOpen_ModalWidth) + Res(40), (int)BoxPos.Y, Res(SelectBoxOpen_ModalWidth), Res(70), Color.BLACK);

            // Draw item
            int i = 0;
            foreach(SelectBox box in SelectBoxOpen_Item)
            {
                if (SelectBoxOpen_Item.Count == 0)
                break;


                if (box.Value != SelectBoxOpen_Item[0].Value || i == 0)
                {
                    DrawText(i == 0 ? SelectBoxOpen_TextSelected : box.Texture, (int)(modalRect.X + x), (int)(modalRect.Y + y));
                    y += Res(60) + Res(i == 0 ? 15 : 0);
                    height += 60;

                    // Set size
                    width = box.Texture.Width > width ? box.Texture.Width + 60 : width;
                }

                i++;
            }

            // Adjust display box
            SelectBoxOpen_ModalTop = (int)BoxPos.Y;
            SelectBoxOpen_ModalLeft = (int)BoxPos.X - Res(SelectBoxOpen_ModalWidth) + Res(40);
            SelectBoxOpen_ModalWidth = width;
            SelectBoxOpen_ModalHeight = height;
            SelectBoxOpen_ModalBorder = 1;

            // Close by click outside
            Rectangle SelectBoxRect = new()
            { 
                X = SelectBoxOpen_ModalLeft, 
                Y = SelectBoxOpen_ModalTop, 
                Width = Res(width), 
                Height = Res(height)
            };

            if (!Raylib.CheckCollisionPointRec(Mouse, SelectBoxRect) && Input.Pressed("Click", Input.MouseLeftClick))
            ActionsCallBack.Add("CloseSelectBox");
        }

        // Set highlights
        // --------------

        private static void SelectBoxOpen_SetHighlights(string modal, Rectangle modalRect)
        {
            // Variables
            int y = Res(80);

            // Draw highlight
            int i = 0;
            foreach(SelectBox box in SelectBoxOpen_Item)
            {
                if (SelectBoxOpen_Item.Count == 0)
                break;

                if (box.Value != SelectBoxOpen_Item[0].Value && i != 0)
                {
                    SetHighLight(modal, "", true, (int)modalRect.X, (int)modalRect.Y + y, (int)modalRect.Width, Res(60) + (SelectBoxOpen_Item.Count == i + 1 ? 20 : 0));
                    y += Res(60);
                }

                i++;
            }
        }
    }
}
