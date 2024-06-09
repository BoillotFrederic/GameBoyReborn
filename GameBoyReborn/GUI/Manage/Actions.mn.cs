// -------
// Actions
// -------

using Raylib_cs;
using Emulator;
using System.Reflection;

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

                // Modals list action
                // ------------------

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
                    ActionOpenModal("PrepareScanList", true, WhereIAm);
                break;

                // Close the program
                case "CloseProgram":
                    Log.Close();
                    Raylib.CloseWindow();
                    Environment.Exit(1);
                break;

                // Set fullscreen at startup
                case "SetFullScreen":
                    Program.AppConfig.FullScreen = !Program.AppConfig.FullScreen;
                    ConfigJson.Save("Config/AppConfig.json", Program.AppConfig);
                    Program.ToogleFullScreen();
                break;

                // Set show FPS
                case "SetShowFPS":
                    Program.AppConfig.ShowFPS = !Program.AppConfig.ShowFPS;
                    ConfigJson.Save("Config/AppConfig.json", Program.AppConfig);
                break;

                // Set show shortkeys
                case "SetShortKeys":
                    Program.AppConfig.ShowShortcutsKeyboardKey = !Program.AppConfig.ShowShortcutsKeyboardKey;
                    ConfigJson.Save("Config/AppConfig.json", Program.AppConfig);
                break;

                // Set scan list recursive
                case "SetScanListRecursive":
                    Program.AppConfig.ScanListRecursive = !Program.AppConfig.ScanListRecursive;
                    ConfigJson.Save("Config/AppConfig.json", Program.AppConfig);
                break;

                // Select boxes
                // ------------

                // Open select box hook tag
                case "OpenSelectBoxHookTag":
                    ActionOpenSelectBox(Program.AppConfig, "HookTagPriority", "HookTag", "PrepareScanList", PrepareScanList_HookTagKey);
                break;

                // Open select box hook tag
                case "OpenSelectBoxBracketsTag":
                    ActionOpenSelectBox(Program.AppConfig, "BracketsTagPriority", "BracketsTag", "PrepareScanList", PrepareScanList_BracketsTagKey);
                break;

                // Select box submit choice
                case "SelectBoxSubmit":
                    SelectBoxItem selectBoxItem = SelectBox_ItemsListed[ModalHighlightPos.Y];
                    RefLight.SetDynamicProperty(SelectBox_ConfigFile, SelectBox_Value, selectBoxItem.Value);
                    SelectBox_ItemSelected = selectBoxItem;
                    ConfigJson.Save("Config/AppConfig.json", SelectBox_ConfigFile);

                    ModalRefresh(SelectBox_ParentName);
                    ActionCloseModal("SelectBox");
                break;

                // Close select box
                case "CloseSelectBox":
                    ActionCloseModal("SelectBox");
                break;

                // Select folder
                // -------------

                // Select folder for scan list
                case "ScanListSelectFolder":
                    SelectFolder_Scroll = false;
                    SelectFolder_ParentName = "PrepareScanList";
                    SelectFolder_ConfigFile = Program.AppConfig;
                    SelectFolder_PathSelected = SelectFolder_ConfigFile.PathRoms;
                    SelectFolder_LastHighLightPosY = 0;
                    SelectFolder_PosContentY = 0;
                    ModalHighlightPos.Y = 0;
                    SelectFolder_Value = "PathRoms";
                    ActionOpenModal("SelectFolder", false, WhereIAm);
                break;

                // Select folder enter
                case "SelectFolderEnter":
                    SelectFolder_Scroll = false;
                    SelectFolder_PathSelected = SelectFolder_Directories[ModalHighlightPos.Y];
                    SelectFolder_LastHighLightPosY = 0;
                    SelectFolder_PosContentY = 0;
                    ModalHighlightPos.Y = 0;
                    ModalRefresh("SelectFolder");
                break;

                // Select folder back
                case "SelectFolderBack":
                    if(SelectFolder_PathSelected != null && SelectFolder_PathSelected != "")
                    {
                        SelectFolder_Scroll = false;
                        SelectFolder_BackFolder = SelectFolder_PathSelected;
                        SelectFolder_PathSelected = GetParentDirectory(SelectFolder_PathSelected);
                        SelectFolder_LastHighLightPosY = 0;
                        SelectFolder_PosContentY = 0;
                        ModalHighlightPos.Y = 0;
                        ModalRefresh("SelectFolder");
                    }
                break;

                // Select folder submit choice
                case "SelectFolderSubmit":
                    RefLight.SetDynamicProperty(SelectFolder_ConfigFile, SelectFolder_Value, SelectFolder_PathSelected);
                    ConfigJson.Save("Config/AppConfig.json", SelectFolder_ConfigFile);

                    HighLightArea = null;
                    ModalRefresh(SelectFolder_ParentName);
                    ActionCloseModal("SelectFolder");
                break;

                // Close select folder
                case "CloseSelectFolder":
                    HighLightArea = null;
                    ActionCloseModal("SelectFolder");
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
            BtnInfoInit();
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
            BtnInfoInit();
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

        // Action open select box
        private static void ActionOpenSelectBox(dynamic configFile, string configVarName, string name, string parentName, string[] keys)
        {
            SelectBox_ModalTop = 0;
            SelectBox_ModalLeft = 0;
            SelectBox_ParentName = parentName;
            SelectBox_Name = name;
            SelectBox_Value = configVarName;
            SelectBox_ConfigFile = configFile;

            ModalHighlightPos.Y = 0;
            SelectBox_Items.Clear();
            Dictionary<string, Texture2D> textures = ModalsTexture[parentName];

            SelectBox_ItemSelected = new() { Value = RefLight.GetDynamicProperty(configFile, configVarName), Texture = textures[name + "SelectWhite"] };

            foreach (string val in keys)
            SelectBox_Items.Add(new() { Value = val, Texture = textures[val] });

            ActionOpenModal("SelectBox", false, WhereIAm);
        }

        // Actions listenning
        private static readonly List<string> ActionsCallBack = new();

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

                // Modals
                // ------

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

                case "SelectBox": 
                {
                    // Confirm
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyP) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadB || Input.KeyC))
                    Action("CloseSelectBox");

                    // Move
                    ActionMove("Modal");
                }
                break;

                case "SelectFolder": 
                {
                    // Confirm
                    if(Input.Pressed("Press V", Input.XabyPadX || Input.KeyV))
                    Action("SelectFolderSubmit");

                    // Select
                    if(Input.Pressed("Press A", Input.XabyPadA || Input.KeyS) && ModalHighlight.Count > 0)
                    Action(ModalHighlight[ModalHighlightPos.Y][ModalHighlightPos.X].Action);

                    // Back to parent folder
                    if(Input.Pressed("Press P", Input.XabyPadB || Input.KeyP))
                    Action("SelectFolderBack");

                    // Close
                    if(Input.Pressed("Press C", Input.XabyPadY || Input.KeyC))
                    Action("CloseSelectFolder");

                    // Move
                    ActionMove("Modal");
                }
                break;

                default: break;
            }

            // Actions callback
            foreach(string action in ActionsCallBack)
            Action(action);
            ActionsCallBack.Clear();
        }
    }
}
