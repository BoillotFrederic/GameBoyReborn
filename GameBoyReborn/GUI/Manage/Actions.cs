// -------
// Actions
// -------

using Raylib_cs;
using Emulator;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Action requested
        public static void Action(string name)
        {
            switch (name)
            {
                // Metro GB list action
                // --------------------

                // Move in list
                case "ListMoveUp":
                case "ListMoveDown":
                case "ListMoveLeft":
                case "ListMoveRight":
                    static void upDown(bool direction)
                    {
                        int newPos = MouseLeftClickTarget + (!direction ? -6 : 6);
                        MouseLeftClickTarget = (!direction && newPos >= 0) || (direction && newPos < NbGame) ? newPos : MouseLeftClickTarget;
                        InputActionMove = true;
                    }

                    static void leftRight(bool direction)
                    {
                        if (!direction) MouseLeftClickTarget = MouseLeftClickTarget % 6 == 0 ? MouseLeftClickTarget : MouseLeftClickTarget - 1;
                        else MouseLeftClickTarget = MouseLeftClickTarget % 6 == 5 ? MouseLeftClickTarget : MouseLeftClickTarget + 1;
                    }

                    switch (name)
                    {
                        case "ListMoveUp": upDown(false); break;
                        case "ListMoveDown": upDown(true); break;
                        case "ListMoveLeft": leftRight(false); break;
                        case "ListMoveRight": leftRight(true); break;
                    }
                break;

                // Move in modal
                case "ModalMoveUp":
                case "ModalMoveDown":
                case "ModalMoveLeft":
                case "ModalMoveRight":
                    if(ModalHighlight.Count > 0)
                    {
                        void modalHighlightPosY(List<List<HighlightElm>> elm, int newPos)
                        {
                            ModalHighlightPos.X = 0;
                            ModalHighlightPos.Y = elm.ElementAtOrDefault(newPos) != null ? newPos : ModalHighlightPos.Y;
                        }

                        void modalHighlightPosX(List<HighlightElm> elm, int newPos)
                        {
                            ModalHighlightPos.X = newPos >= 0 && newPos < elm.Count ? newPos : ModalHighlightPos.X;
                        }

                        switch (name)
                        {
                            case "ModalMoveUp": modalHighlightPosY(ModalHighlight, ModalHighlightPos.Y - 1); break;
                            case "ModalMoveDown": modalHighlightPosY(ModalHighlight, ModalHighlightPos.Y + 1); break;
                            case "ModalMoveLeft": modalHighlightPosX(ModalHighlight[ModalHighlightPos.Y], ModalHighlightPos.X - 1); break;
                            case "ModalMoveRight": modalHighlightPosX(ModalHighlight[ModalHighlightPos.Y], ModalHighlightPos.X + 1); break;
                        }
                    }
                break;

                // Open menu
                case "ListOpenMenu":
                    ActionOpenModal("MenuList", false);
                break;

                // Open global config
                case "ListOpenGlobalConfig":
                    ActionOpenModal("ComingSoon", false);
                break;

                // Open config
                case "ListOpenConfig":
                    ActionOpenModal("ComingSoon", false);
                break;

                // Play game
                case "ListPlay":
                    Emulation.Start(GameList[MouseLeftClickTarget].Path);
                break;

                // Close all modals
                case "CloseAllModals":
                    ActionCloseModal("");
                break;

                // Select directory for scan
                case "SelectDirForScan":
                    ActionOpenModal("SelectDirForScan", true);
                break;

                // Menu in game
                case "MenuGame":
                    ActionOpenModal("MenuGame", true);
                break;

                // Return to the game
                case "MenuGameRestore":
                    if(Program.Emulation != null)
                    {
                        Program.Emulation.MenuIsOpen = false;
                        Program.Emulation.UnPause();
                    }
                break;

                // Close the game
                case "CloseGame":
                    ActionCloseModal("");

                    if (Program.Emulation != null)
                    Program.Emulation.Stop();
                break;

                // Coming soon
                case "ComingSoon":
                    ActionOpenModal("ComingSoon", false, WhereIAm);
                break;

                // Coming soon close
                case "ComingSoonClose":
                    ActionCloseModal("ComingSoon");
                break;

                // Prepare scan list
                case "PrepareScanList":
                    ActionOpenModal("PrepareScanList", false, WhereIAm);
                break;

                // Close the program
                case "CloseProgram":
                    Log.Close();
                    Raylib.CloseWindow();
                    Environment.Exit(1);
                break;

                default: break;
            }
        }

        // Action open modal
        private static void ActionOpenModal(string modal, bool isNew, string WhereBack = "List")
        {
            WhereIAmBack = WhereBack;
            WhereIAm = modal;

            if(isNew)
            ModalsOpen.Clear();

            ModalsOpen.Add(modal);
            ModalsListenning();
            BtnInfoInit(35.0f * TextResolution);
        }

        // Action close modal
        private static void ActionCloseModal(string modal)
        {
            if (modal == "")
            {
                ModalsOpen.Clear();
                ModalDestruct();
                WhereIAm = "List";
            }
            else
            {
                ModalDestruct(modal);
                ModalsOpen.RemoveAt(ModalsOpen.FindIndex(x => x == modal));
                WhereIAm = WhereIAmBack;
            }

            ModalsListenning();
            BtnInfoInit(35.0f * TextResolution);
        }

        // Actions movement
        private static void ActionMove(string component)
        {
            // Move up
            if (Input.Repeat("Up", Input.DPadUp || Input.AxisLeftPadUp, 0.2f))
            Action(component + "MoveUp");

            // Move down
            if (Input.Repeat("Down", Input.DPadDown || Input.AxisLeftPadDown, 0.2f))
            Action(component + "MoveDown");

            // Move left
            if (Input.Repeat("Left", Input.DPadLeft || Input.AxisLeftPadLeft, 0.2f))
            Action(component + "MoveLeft");

            // Move right
            if (Input.Repeat("Right", Input.DPadRight || Input.AxisLeftPadRight, 0.2f))
            Action(component + "MoveRight");
        }

        // Actions listenning
        public static void ActionsListenning()
        {
            switch (WhereIAm)
            {
                // Metro GB list
                // -------------
                case "List":
                {
                    // Play
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP))
                    Action("ListPlay");

                    // Open menu
                    if(Input.Pressed("Press M", Input.KeyM || (Input.AxisLS && Input.AxisRS)))
                    Action("ListOpenMenu");

                    // Open global config
                    if(Input.Pressed("Press G", Input.XabyPadY || Input.KeyG))
                    Action("ListOpenGlobalConfig");

                    // Open config
                    if(Input.Pressed("Press C", Input.XabyPadX || Input.KeyC))
                    Action("ListOpenConfig");

                    // Move
                    ActionMove("List");
                }
                break;

                case "PrepareScanList":
                {
                    // Select
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyS) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Scan
                    if(Input.Pressed("Press X", Input.XabyPadX || Input.KeyP) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("CloseAllModals");

                    // Move
                    ActionMove("Modal");
                }
                break;

                case "ComingSoon":
                {
                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("ComingSoonClose");
                }
                break;

                case "MenuList":
                {
                    // Confirm
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("CloseAllModals");

                    // Move
                    ActionMove("Modal");
                }
                break;

                case "MenuGame": 
                {
                    // Confirm
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("MenuGameRestore");

                    // Move
                    ActionMove("Modal");
                }
                break;

                default: break;
            }
        }
    }
}
