// -------------
// Modal confirm
// -------------

using Raylib_cs;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int Confirm_ModalWidth = 1200;
        private static int Confirm_ModalHeight = 200;
        private static string Confirm_Text = "";
        private static string Confirm_BtnConfirmText = "";
        private static string Confirm_BtnCancelText = "";
        private static List<string> Confirm_TextWrap = new();
        private static string Confirm_Action = "";

        // Set textures
        // ------------

        private static void Confirm_SetTextures(string modal)
        {
            Confirm_TextWrap = TextNlWrap(Confirm_Text, 40.0f * TextResolution, 3.0f, Confirm_ModalWidth * TextResolution - (100 * TextResolution), 0);

            for(int i = 0; i < Confirm_TextWrap.Count; i++)
            ModalsTexture[modal].Add("ConfirmText" + i, SingleToTexture(Confirm_TextWrap[i], 40.0f * TextResolution, 3.0f, Color.BLACK));

            Confirm_ModalHeight = 80 + Confirm_TextWrap.Count * 60;
        }

        // Draw components
        // ---------------

        private static void Confirm_DrawComponents(string modal, Rectangle modalRect)
        {
            // Text
            int nbLine = Confirm_TextWrap.Count;
            for(int i = 0; i < nbLine; i++)
            {
                int textPosX = (int)(modalRect.X + Formulas.CenterElm((int)modalRect.Width, Res(ModalsTexture[modal]["ConfirmText" + i].Width)));
                DrawText(ModalsTexture[modal]["ConfirmText" + i], textPosX, (int)(modalRect.Y + Res(50) + (Res(60) * i)));
            }
        }
    }
}
