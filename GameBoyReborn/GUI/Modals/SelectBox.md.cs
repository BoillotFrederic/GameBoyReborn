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

        private static int SelectBox_ModalWidth = 0;
        private static int SelectBox_ModalHeight = 0;
        private static int SelectBox_ModalTop = 0;
        private static int SelectBox_ModalLeft = 0;
        private static int SelectBox_ModalBorder;
        private static int SelectBox_PosContentY = 0;
        private static int SelectBox_LastHighLightPosY = 0;
        private static bool SelectBox_Scroll;
        private static string SelectBox_ParentName = "";
        private static string SelectBox_Name = "";
        private static string SelectBox_Value = "";
        private static dynamic SelectBox_ConfigFile = "";
        private static SelectBoxItem SelectBox_ItemSelected = new();
        private static readonly List<SelectBoxItem> SelectBox_ItemsListed = new();
        private static readonly List<SelectBoxItem> SelectBox_Items = new();

        private struct SelectBoxItem
        {
            public string Value;
            public Texture2D Texture;
        }

        // Draw components
        // ---------------

        private static void SelectBox_DrawComponents(string modal, Rectangle modalRect)
        {
            // Operating variables
            Vector2 BoxPos = RefLight.GetVec2ByName("SelectBox_" + SelectBox_Name + "Pos", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
            int y = 0;
            int x = Res(30);
            int width;
            int height = 80;

            // Draw items not selected
            int i = 0;
            width = SelectBox_ItemSelected.Texture.Width;
            SelectBox_ItemsListed.Clear();
            foreach(SelectBoxItem item in SelectBox_Items)
            {
                if (SelectBox_Items.Count == 0)
                break;

                // If not selected
                if (item.Value != SelectBox_ItemSelected.Value)
                {
                    // Draw
                    SelectBox_ItemsListed.Add(item);
                    DrawText(item.Texture, (int)(modalRect.X + x), SelectBox_PosContentY + y);
                    y += Res(60);

                    // Set size
                    width = item.Texture.Width > width ? item.Texture.Width : width;
                    height += 60;
                }

                i++;
            }

            // Draw item selected
            Raylib.DrawRectangle((int)BoxPos.X - Res(SelectBox_ModalWidth) + Res(40), (int)BoxPos.Y, Res(SelectBox_ModalWidth), Res(70), Color.BLACK);
            DrawText(SelectBox_ItemSelected.Texture, (int)(modalRect.X + x), (int)(modalRect.Y + Res(15)));

            // Select box reduce height
            if (BoxPos.Y + Res(height) > ScreenHeight - Res(100))
            {
                width += 90;
                SelectBox_Scroll = true;
                int newSelectBoxHeight = (int)ResI(ScreenHeight - Res(100) - BoxPos.Y);
                int contentHeight = Res(height - 80);
                int containerHeight = Res(newSelectBoxHeight) - Res(90);

                // Set new height to the global select box height
                height = newSelectBoxHeight;

                // Set sroll bar
                string ScrollBarName = "ScrollSelectBox";
                ScrollBarInit(ScrollBarName, 0);

                SelectBox_PosContentY = (int)(modalRect.Y + Res(90) - (int)ScrollBarList[ScrollBarName].ContentPosY);
                ScrollBarY(ScrollBarName, containerHeight, contentHeight, Res(30), Res(height) - Res(70), (int)modalRect.X + Res(width) - Res(30), (int)modalRect.Y + Res(70), WhereIAm == "SelectBox", Color.RAYWHITE, Color.DARKGRAY);
            }

            // Select box fully visible
            else
            {
                SelectBox_PosContentY = (int)modalRect.Y + Res(90);
                SelectBox_Scroll = false;
                width += 60;
            }

            // Adjust display box
            SelectBox_ModalTop = (int)BoxPos.Y;
            SelectBox_ModalLeft = (int)BoxPos.X - Res(SelectBox_ModalWidth) + Res(40);
            SelectBox_ModalWidth = width;
            SelectBox_ModalHeight = height;
            SelectBox_ModalBorder = 1;

            // Close by click outside
            Rectangle SelectBoxRect = new()
            { 
                X = SelectBox_ModalLeft, 
                Y = SelectBox_ModalTop, 
                Width = Res(width), 
                Height = Res(height)
            };

            if (!Raylib.CheckCollisionPointRec(Mouse, SelectBoxRect) && Input.Pressed("Click", Input.MouseLeftClick))
            ActionsCallBack.Add("CloseSelectBox");
        }

        // Set highlights
        // --------------

        private static void SelectBox_SetHighlights(string modal, Rectangle modalRect)
        {
            // Variables
            int y = -Res(10);

            // Draw highlight
            int i = 0;
            foreach(SelectBoxItem item in SelectBox_Items)
            {
/*                if (SelectBox_Items.Count == 0)
                break;*/

                if (item.Value != SelectBox_ItemSelected.Value)
                {
                    int paddingScroll = SelectBox_Scroll ? Res(30) : 0;
                    SetHighLight(modal, "SelectBoxSubmit", true, (int)modalRect.X, SelectBox_PosContentY + y, (int)modalRect.Width - paddingScroll, Res(60) + (SelectBox_Items.Count == i + 1 ? 20 : 0));
                    y += Res(60);
                }

                i++;
            }

            // Move by pad/keyword (hidden lines)
            if (SelectBox_LastHighLightPosY != ModalHighlightPos.Y && (Input.DPadUp || Input.DPadDown || Input.AxisLeftPadUp || Input.AxisLeftPadDown))
            {
                string scrollbarName = "ScrollSelectBox";
                if (!ScrollBarList.ContainsKey(scrollbarName))
                return;


                Rectangle highlightRect = ModalHighlight[ModalHighlightPos.Y][0].ElmRect;
                int lineDownOverflowY = (int)(highlightRect.Y + highlightRect.Height - (modalRect.Y + modalRect.Height));
                int lineUpOverflowY = (int)(modalRect.Y - (highlightRect.Y - highlightRect.Height));
                ScrollBar scrollBar = ScrollBarList[scrollbarName];
                bool direction = SelectBox_LastHighLightPosY < ModalHighlightPos.Y;

                if(direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
                else if(lineUpOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY - lineUpOverflowY));
                else if (!direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
            }
            SelectBox_LastHighLightPosY = ModalHighlightPos.Y;
        }
    }
}
