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
        private static readonly string[] Modals = new string[]
        {
            "MenuList",
            "SelectDirForScan",
            "MenuGame",
            "ComingSoon",
            "PrepareScanList",
            "SelectBoxOpen"
        };

        // Operating variables
        private static readonly List<string> ModalsOpen = new();
        private static readonly Dictionary<string, Dictionary<string, Texture2D>> ModalsTexture = new();
        private static readonly Dictionary<string, bool> ModalsIsInit = new();
        private static readonly Dictionary<string, int> ModalsLoop = new();

        // Modals listenning
        // -----------------

        private static void ModalsListenning()
        {
            // Highlight clicked
            HighlightClicked = false;

            // Modal loop counter
            foreach(string modal in Modals)
            if (!ModalsOpen.Contains(modal))
            {
                if (!ModalsLoop.ContainsKey(modal))
                ModalsLoop.Add(modal, 0);
                else
                ModalsLoop[modal] = 0;
            }

            // Modals draw
            foreach(string modal in ModalsOpen)
            DrawModal(modal);

            // Select line clicked
            if(ModalHighlight.Count > 0 && HighlightClicked)
            Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);
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
            if (!ModalsIsInit.ContainsKey(modal))
            ModalsIsInit.Add(modal, false);

            // Init modal textures dictionary
            if (!ModalsTexture.ContainsKey(modal))
            ModalsTexture.Add(modal, new Dictionary<string, Texture2D>());

            // Create textures for this modal
            if (!ModalsIsInit[modal] && ModalSetTextures.ContainsKey(modal))
            ModalSetTextures[modal].Invoke(modal);

            // Init modal Highlight
            ModalHighlight.Clear();

            if (ModalSetHighlights.ContainsKey(modal))
            ModalSetHighlights[modal].Invoke(modal, modalRect);

            // Modal is init now
            ModalsIsInit[modal] = true;
        }

        // Modal destructor
        // ----------------
        
        private static void ModalDestruct(string modalName = "")
        {
            // Init
            ModalHighlightPos.X = modalName == "SelectBoxOpen" ? ModalHighlightLastPos.X : 0;
            ModalHighlightPos.Y = modalName == "SelectBoxOpen" ? ModalHighlightLastPos.Y : 0;

            // Destruct
            static void destruct(string MN)
            {
                if (ModalsIsInit.ContainsKey(MN))
                {
                    foreach (KeyValuePair<string, Texture2D> Item in ModalsTexture[MN])
                    {
                        ModalsIsInit[MN] = false;
                        Raylib.UnloadTexture(ModalsTexture[MN][Item.Key]);
                    }

                    ModalsTexture[MN].Clear();
                }
            }

            // All modal
            if (modalName == "")
            {
                foreach (KeyValuePair<string, Dictionary<string, Texture2D>> Item in ModalsTexture)
                destruct(Item.Key);
            }

            // One modal
            else
            destruct(modalName);
        }
    }
}
