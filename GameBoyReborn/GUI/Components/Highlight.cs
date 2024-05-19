// -------
// Actions
// -------

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static void SetHighLight(string modal, string action, bool hover, int X, int Y, int width, int height)
        {
            if(modal == WhereIAm)
            {
                List<HighlightElm> highLight = new();
                highLight.Add(new HighlightElm() { Action = action, ElmRect = new() { X = X, Y = Y, Width = width, Height = height } });
                ModalHighlight.Add(highLight);
            }
        }
    }
}
