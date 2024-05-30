// ---------
// Scrollbar
// ---------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Operating variables
        private static readonly Dictionary<string, ScrollBar> ScrollBarList = new();

        // Structures
        private struct ScrollBar
        {
            public bool HangClicked = false;
            public float HangClickedPosY = 0;
            public float ContentPosY = 0;
            public float MousePos = 0;
        }

        // Init scrollbar
        private static void ScrollBarInit(string name, int contentPosOrigin)
        {
            if (!ScrollBarList.ContainsKey(name))
            {
                ScrollBar scrollBar = new();
                scrollBar.ContentPosY = contentPosOrigin;
                ScrollBarList.Add(name, scrollBar);
            }
        }

        // Set content Y
        private static void ScrollBarSetY(string name, int newPosY)
        {
            ScrollBar scrollBar = ScrollBarList[name];
            scrollBar.ContentPosY = newPosY;
            ScrollBarList[name] = scrollBar;
        }

        // Set scrollbar
        private static float ScrollBarY(string name, int containerHeight, int contentHeight, int scrollBarWidth, int scrollBarHeight, int scrollBarX, int scrollBarY, bool focused, Color colorBar, Color colorHang)
        {
            // First, draw
            // -----------

            // Get parameters
            ScrollBar scrollBar = ScrollBarList[name];

            // Scroll height in float percent
            float scrollHeightPercent = contentHeight > containerHeight ? ((float)containerHeight / contentHeight) : 1;

            // If no scroll, return 100%
            if (scrollHeightPercent == 1)
            return 1;

            // Calculate position and size
            int scrollBarHangHeight = (int)(scrollHeightPercent * scrollBarHeight);
            int contentNotVisibleHeight = contentHeight - containerHeight;
            int scrollBarEmptyHeight = scrollBarHeight - scrollBarHangHeight;
            float ContentPosYPercent = scrollBar.ContentPosY / contentNotVisibleHeight;

            // Limit hang
            ContentPosYPercent = Math.Clamp(ContentPosYPercent, 0, 1);

            // Set pos
            int scrollBarPosY = (int)(ContentPosYPercent * scrollBarEmptyHeight);

            // Scroll bar dimensions
            Rectangle scrollBarRect = new()
            {
                X = scrollBarX,
                Y = scrollBarY,
                Width = scrollBarWidth,
                Height = scrollBarHeight,
            };

            // Scroll bar hang dimensions
            Rectangle scrollBarHangRectHang = new()
            {
                X = scrollBarX,
                Y = scrollBarY + scrollBarPosY,
                Width = scrollBarWidth,
                Height = scrollBarHangHeight,
            };

            // Draw scroll bar / hang
            Raylib.DrawRectangleRec(scrollBarRect, colorBar);
            Raylib.DrawRectangleRec(scrollBarHangRectHang, colorHang);

            // Second, event management
            // ------------------------

            // New position after event
            int newContentPosY = 0;

            // If not focused, stop event
            if (!focused)
            return scrollHeightPercent;

            // Click pressed
            if (Raylib.CheckCollisionPointRec(Mouse, scrollBarHangRectHang) && Input.Pressed("Click", Input.MouseLeftClick))
            {
                scrollBar.HangClicked = true;
                scrollBar.HangClickedPosY = scrollBarPosY;
                scrollBar.MousePos = Mouse.Y;
            }

            // Click unpressed
            if (!Input.MouseLeftClick)
            scrollBar.HangClicked = false;

            // Move by wheel
            if (!scrollBar.HangClicked)
            newContentPosY = (int)(scrollBar.ContentPosY + (int)(Raylib.GetMouseWheelMove() * -100));

            // Move with mouse
            else if (scrollBar.HangClicked)
            {
                int hangPosDiff = (int)(scrollBar.HangClickedPosY  + (Mouse.Y - scrollBar.MousePos));
                float diffToPercent = (float)hangPosDiff / scrollBarEmptyHeight;
                newContentPosY = (int)(contentNotVisibleHeight * diffToPercent);
            }

            // Save parameters
            scrollBar.ContentPosY = Math.Clamp(newContentPosY, 0, contentNotVisibleHeight);
            ScrollBarList[name] = scrollBar;

            return scrollHeightPercent;
        }
    }
}
