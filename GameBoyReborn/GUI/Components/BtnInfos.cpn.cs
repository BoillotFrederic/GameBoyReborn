// -------------
// Buttons infos
// -------------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private struct BtnInfo
        {
            public string Action = "";
            public Texture2D IconPad;
            public Texture2D IconKey;
            public Texture2D Text;
            public int IconPadWidth = 0;
            public int IconKeyWidth = 0;
        }

        private static BtnInfo[]? BtnInfos;

        // Buttons init
        private static void BtnInfoInit()
        {
            // Clear
            if(BtnInfos != null)
            {
                Array.Clear(BtnInfos);
                BtnInfos = Array.Empty<BtnInfo>();
            }

            // Setting
            void btnInfosSet(string action, int index, string text, Color textColor, string IPad, string IKey, int IPadWidth, int IKeyWidth)
            {
                if(BtnInfos != null && index < BtnInfos.Length)
                {
                    BtnInfos[index].Action = action;
                    BtnInfos[index].Text = SingleToTexture(text, 35.0f * TextResolution, 3.0f, textColor);
                    BtnInfos[index].IconPad = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IPad);
                    BtnInfos[index].IconKey = Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/" + IKey);
                    BtnInfos[index].IconPadWidth = IPadWidth;
                    BtnInfos[index].IconKeyWidth = IKeyWidth;
                }
            }

            // Load btns
            switch (WhereIAm)
            {
                case "List":
                    BtnInfos = new BtnInfo[4];
                    btnInfosSet("ListOpenMenu", 0, "Menu", Color.WHITE, "TriggerLR.png", "KeyM.png", 140, 60);
                    btnInfosSet("ListOpenGlobalConfig", 1, "Paramètres globaux", Color.WHITE, "ButtonY.png", "KeyG.png", 60, 60);
                    btnInfosSet("ListOpenConfig", 2, "Paramètres", Color.WHITE, "ButtonX.png", "KeyC.png", 60, 60);
                    btnInfosSet("ListPlay", 3, "Jouer", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;

                case "ComingSoon":
                    BtnInfos = new BtnInfo[1];
                    btnInfosSet("ComingSoonClose", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                break;

                case "MenuList":
                    BtnInfos = new BtnInfo[2];
                    btnInfosSet("CloseAllModals", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet("HighLightAction", 1, "Valider", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;

                case "MenuGame":
                    BtnInfos = new BtnInfo[2];
                    btnInfosSet("MenuGameRestore", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet("HighLightAction", 1, "Valider", Color.WHITE, "ButtonA.png", "KeyP.png", 60, 60);
                break;

                case "PrepareScanList":
                    BtnInfos = new BtnInfo[3];
                    btnInfosSet("CloseAllModals", 0, "Annuler", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet("HighLightAction", 1, "Sélectionner", Color.WHITE, "ButtonA.png", "KeyS.png", 60, 60);
                    btnInfosSet("ScanDir", 2, "Lancer le scan", Color.WHITE, "ButtonX.png", "KeyP.png", 60, 60);
                break;

                case "SelectFolder":
                    BtnInfos = new BtnInfo[4];
                    btnInfosSet("CloseSelectFolder", 0, "Annuler", Color.WHITE, "ButtonY.png", "KeyC.png", 60, 60);
                    btnInfosSet("SelectFolderBack", 1, "Dossier parent", Color.WHITE, "ButtonB.png", "KeyP.png", 60, 60);
                    btnInfosSet("HighLightAction", 2, "Sélectionner", Color.WHITE, "ButtonA.png", "KeyS.png", 60, 60);
                    btnInfosSet("SelectFolderSubmit", 3, "Confirmer", Color.WHITE, "ButtonX.png", "KeyV.png", 60, 60);
                break;

                case "SelectBox":
                    BtnInfos = new BtnInfo[2];
                    btnInfosSet("CloseSelectBox", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet("HighLightAction", 1, "Sélectionner", Color.WHITE, "ButtonA.png", "KeyS.png", 60, 60);
                break;

                case "SelectDirForScan":
                    BtnInfos = new BtnInfo[3];
                    btnInfosSet("CloseAllModals", 0, "Fermer", Color.WHITE, "ButtonB.png", "KeyC.png", 60, 60);
                    btnInfosSet("HighLightAction", 1, "Sélectionner", Color.WHITE, "ButtonA.png", "KeyS.png", 60, 60);
                    btnInfosSet("ScanDir", 2, "Scanner", Color.WHITE, "ButtonX.png", "KeyP.png", 60, 60);
                break;
            }
        }

        private static void DrawBtnInfos()
        {
            if(BtnInfos != null)
            {
                int yStart = Res(100);
                int lineWidth = 0;
                int lineSpacing = Res(20);
                int marginWidth = Res(20);

                static int iconWidthCalcul(BtnInfo btn)
                {
                    if(Input.IsPad)
                    return Program.AppConfig.ShowShortcutsPadButton ? btn.IconPadWidth : 0;
                    else
                    return Program.AppConfig.ShowShortcutsKeyboardKey ? btn.IconKeyWidth : 0;
                }

                for (int b = 0; b < BtnInfos.Length; b++)
                {
                    int iconWidth = iconWidthCalcul(BtnInfos[b]);
                    int nbMargin = iconWidth == 0 ? 2 : 3;
                    lineWidth += Res(iconWidth) + marginWidth * nbMargin + Res(BtnInfos[b].Text.Width);
                }

                int buttonsStartPosX = Formulas.CenterElm(ScreenWidth, lineWidth + lineSpacing * (BtnInfos.Length - 1));

                for (int b = 0; b < BtnInfos.Length; b++)
                {
                    int iconWidth = iconWidthCalcul(BtnInfos[b]);
                    int iconTextureWidth = Input.IsPad ? BtnInfos[b].IconPad.Width : BtnInfos[b].IconKey.Width;
                    int nbMargin = iconWidth == 0 ? 2 : 3;
                    int textWidth = Res(BtnInfos[b].Text.Width);
                    int btnWidth = Res(iconWidth) + marginWidth * nbMargin + textWidth;
                    int btnHeight = Res(70);
                    Vector2 textPos = new() { X = buttonsStartPosX + Res(iconWidth) + marginWidth * (nbMargin - 1), Y = ScreenHeight - yStart + Res(36) };
                    Vector2 iconPos = new() { X = buttonsStartPosX + marginWidth, Y = ScreenHeight - yStart + Res(25) };
                    float textScale = textWidth / (float)BtnInfos[b].Text.Width;
                    float iconScale = Res(iconWidth) / (float)iconTextureWidth;

                    Rectangle rect = new() { X = buttonsStartPosX, Y = ScreenHeight - yStart + Res(20), Height = btnHeight, Width = btnWidth };
                    Raylib.DrawRectangleRec(rect, Color.DARKGRAY);
                    Raylib.DrawRectangleLinesEx(rect, 1.0f, Color.BLACK);
                    Raylib.DrawTextureEx(BtnInfos[b].Text, textPos, 0, textScale, Color.WHITE);

                    if(iconWidth != 0)
                    Raylib.DrawTextureEx(Input.IsPad ? BtnInfos[b].IconPad : BtnInfos[b].IconKey, iconPos, 0, iconScale, Color.WHITE);

                    buttonsStartPosX += btnWidth + lineSpacing;

                    // Hover and click
                    if (Raylib.CheckCollisionPointRec(Mouse, rect))
                    {
                        Cursor = MouseCursor.MOUSE_CURSOR_POINTING_HAND;

                        if (Input.Pressed("Click", Input.MouseLeftClick))
                        {
                            string action = BtnInfos[b].Action == "HighLightAction" && ModalHighlight.Count > 0 ? ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action : BtnInfos[b].Action;
                            Action(action);
                        }
                    }
                }
            }
        }
    }
}
