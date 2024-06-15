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
        private static Rectangle ConfirmBorderRect = new();
        private static Rectangle CancelBorderRect = new();
        private static string Confirm_Action = "";

        // Set textures
        // ------------

        private static void Confirm_SetTextures(string modal)
        {
            Confirm_TextWrap = TextNlWrap(Confirm_Text, 40.0f * TextResolution, 3.0f, Confirm_ModalWidth * TextResolution - (100 * TextResolution), 0);

            for(int i = 0; i < Confirm_TextWrap.Count; i++)
            ModalsTexture[modal].Add("ConfirmText" + i, SingleToTexture(Confirm_TextWrap[i], 40.0f * TextResolution, 3.0f, Color.BLACK));

            Confirm_ModalHeight = 200 + Confirm_TextWrap.Count * 60;

            ModalsTexture[modal].Add("Confirm", SingleToTexture(Confirm_BtnConfirmText, 40.0f * TextResolution, 3.0f, Color.BLACK));
            ModalsTexture[modal].Add("Cancel", SingleToTexture(Confirm_BtnCancelText, 40.0f * TextResolution, 3.0f, Color.BLACK));
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

            // Btns
            float confirmWidth = Res(ModalsTexture[modal]["Confirm"].Width + 100);
            float cancelWidth = Res(ModalsTexture[modal]["Cancel"].Width + 100);
            int posX = (int)(modalRect.X + Formulas.CenterElm((int)modalRect.Width, (int)(confirmWidth + Res(100) + cancelWidth)));

            ConfirmBorderRect = new()
            {
                Width = confirmWidth,
                Height = Res(60),
                X = posX,
                Y = modalRect.Y + Res(90) + Res(60) * nbLine
            };

            CancelBorderRect = new()
            {
                Width = cancelWidth,
                Height = Res(60),
                X = posX + confirmWidth + Res(100),
                Y = modalRect.Y + Res(90) + Res(60) * nbLine
            };

            Raylib.DrawRectangleLinesEx(ConfirmBorderRect, 1, Color.BLACK);
            Raylib.DrawRectangleLinesEx(CancelBorderRect, 1, Color.BLACK);
            DrawText(ModalsTexture[modal]["Confirm"], posX + Res(50), (int)(modalRect.Y + Res(100) + (Res(60) * nbLine)));
            DrawText(ModalsTexture[modal]["Cancel"], (int)(posX + confirmWidth + Res(150)), (int)(modalRect.Y + Res(100) + (Res(60) * nbLine)));
        }

        // Set highlights
        // --------------

        private static void Confirm_SetHighlights(string modal, Rectangle modalRect)
        {
            if(modal == WhereIAm)
            {
                List<HighlightElm> highLight = new();
                highLight.Add(new HighlightElm() { Action = Confirm_Action, ElmRect = ConfirmBorderRect });
                highLight.Add(new HighlightElm() { Action = "ConfirmClose", ElmRect = CancelBorderRect });
                ModalHighlight.Add(highLight);
            }
        }
    }
}
