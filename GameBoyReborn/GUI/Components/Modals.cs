// ------
// Modals
// ------

using Raylib_cs;
using System.Reflection;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Modals list
        private static readonly string[] Modals = new string[]{
            "MenuList",
            "SelectDirForScan",
            "MenuGame"
        };

        // Operating variables
        private static readonly List<string> ModalsOpen = new();
        private static readonly Dictionary<string, Dictionary<string, Texture2D>> ModalTextures = new();
        private static readonly Dictionary<string, bool> ModalIsInit = new();
        private static VecInt2 ModalHighlightPos = new() { X = 0, Y = 0 };
        private static readonly List<List<HighlightElm>> ModalHighlight = new();

        // Delegations
        private delegate void SetTextures(string modal);
        private delegate void DrawComponents(string modal, Rectangle modalRect);
        private delegate void SetHighlights(string modal, Rectangle modalRect);
        private static readonly Dictionary<string, SetTextures> ModalSetTextures = new();
        private static readonly Dictionary<string, DrawComponents> ModalDrawComponents = new();
        private static readonly Dictionary<string, SetHighlights> ModalSetHighlights = new();

        // Structures
        private struct HighlightElm 
        {
            public string Action;
            public Rectangle ElmRect;
        }

        private struct VecInt2
        {
            public int X;
            public int Y;
        }

        // Modals listenning
        private static void ModalsListenning()
        {
            // Draw modal
            bool ShiftClicked = false;

            void DrawModal(string modal, int width, int height)
            {
                // Modal
                Rectangle modalRect = new() { Width = width, Height = height, X = Formulas.CenterElm(ScreenWidth, width), Y = Formulas.CenterElm(ScreenHeight, height) };
                Raylib.DrawRectangleRec(modalRect, Color.WHITE);
                Raylib.DrawRectangleLinesEx(modalRect, Res(10), Color.DARKGRAY);

                // Init
                InitModalTextures(modal, modalRect);

                // Select line by hover and click handle
                int x = 0;
                int y = 0;

                if(ModalHighlight.Count > 0)
                foreach (List<HighlightElm> selectLines in ModalHighlight)
                {
                    foreach (HighlightElm selectLine in selectLines)
                    {
                        if(Raylib.CheckCollisionPointRec(Mouse, selectLine.ElmRect))
                        {
                            ModalHighlightPos.X = x;
                            ModalHighlightPos.Y = y;

                            if (Input.Pressed("Click", Input.MouseLeftClick))
                            ShiftClicked = true;
                        }
                        x++;
                    }
                    x = 0;
                    y++;
                }

                // Draw select line
                if(ModalHighlight.Count > 0)
                Raylib.DrawRectangleRec(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].ElmRect, Color.LIGHTGRAY);

                // Draw content
                if (ModalDrawComponents.ContainsKey(modal))
                ModalDrawComponents[modal].Invoke(modal, modalRect);
            }

            // Modals match
            foreach(string modal in ModalsOpen)
            {
                int modalWidth = RefLight.GetIntByName(modal + "_ModalWidth", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
                int modalHeight = RefLight.GetIntByName(modal + "_ModalHeight", typeof(DrawGUI), BindingFlags.NonPublic | BindingFlags.Static);
                DrawModal(modal, Res(modalWidth), Res(modalHeight));
            }

            // Select line clicked
            if(ModalHighlight.Count > 0 && ShiftClicked)
            Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);
        }

        // Modal destructor
        // ----------------
        
        private static void ModalDestruct(string modalName = "")
        {
            // Destruct
            static void destruct(string MN)
            {
                if (ModalIsInit.ContainsKey(MN))
                {
                    foreach (var Item in ModalTextures[MN])
                    {
                        ModalIsInit[MN] = false;
                        Raylib.UnloadTexture(ModalTextures[MN][Item.Key]);
                    }

                    ModalTextures[MN].Clear();
                }
            }

            // All modal
            if (modalName == "")
            {
                foreach (var Item in ModalTextures)
                destruct(Item.Key);
            }

            // One modal
            destruct(modalName);
        }

        // Init
        // ----

        // Init all modals
        private static void InitModals()
        {
            static void addMethod(string modal, string name, Type methodThype)
            {
                Delegate? method = RefLight.GetDelegateMethodByName(modal + "_" + name, typeof(DrawGUI), methodThype, BindingFlags.Static | BindingFlags.NonPublic);
            
                if(method != null)
                switch (name)
                {
                    case "SetTextures": ModalSetTextures.Add(modal, (SetTextures)method); break;
                    case "DrawComponents": ModalDrawComponents.Add(modal, (DrawComponents)method); break;
                    case "SetHighlights": ModalSetHighlights.Add(modal, (SetHighlights)method); break;
                }
            }

            foreach(string modal in Modals)
            {
                addMethod(modal, "SetTextures", typeof(SetTextures));
                addMethod(modal, "DrawComponents", typeof(DrawComponents));
                addMethod(modal, "SetHighlights", typeof(SetHighlights));
            }
        }

        // Init all textures
        private static void InitModalTextures(string modal, Rectangle modalRect)
        {
            // If modal is not init
            if (!ModalIsInit.ContainsKey(modal))
            ModalIsInit.Add(modal, false);

            // Init modal textures dictionary
            if (!ModalTextures.ContainsKey(modal))
            ModalTextures.Add(modal, new Dictionary<string, Texture2D>());

            // Create textures for this modal
            if (!ModalIsInit[modal] && ModalSetTextures.ContainsKey(modal))
            ModalSetTextures[modal].Invoke(modal);

            // Init modal Highlight
            ModalHighlight.Clear();

            if (ModalSetHighlights.ContainsKey(modal))
            ModalSetHighlights[modal].Invoke(modal, modalRect);

            // Modal is init now
            ModalIsInit[modal] = true;
        }
    }
}
