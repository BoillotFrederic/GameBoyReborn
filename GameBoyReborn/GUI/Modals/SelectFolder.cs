// -------------------
// Modal Select Folder
// -------------------

using Raylib_cs;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int SelectFolder_ModalWidth = 1024;
        private const int SelectFolder_ModalHeight = 950;
        private static dynamic SelectFolder_ConfigFile = "";
        private static string? SelectFolder_PathSelected;
        private static string SelectFolder_ParentName = "";
        private static string SelectFolder_BackFolder = "";
        private static string SelectFolder_Value = "";
        private static int SelectFolder_PosContentY = 0;
        private static int SelectFolder_LastHighLightPosY = 0;
        private static bool SelectFolder_Scroll;
        private static List<string> SelectFolder_Directories = new();

        // Set textures
        // ------------

        private static void SelectFolder_SetTextures(string modal)
        {
            if(SelectFolder_PathSelected != null)
            {
                string path = TruncateTextWithEllipsis(SelectFolder_PathSelected == "" ? "Aucun" : SelectFolder_PathSelected, 40.0f * TextResolution, 3.0f, 1024 - 140);
                string dirSelected = TruncateTextWithEllipsis(SelectFolder_PathSelected == "" ? "Ordinateur" : (GetLastFolderName(SelectFolder_PathSelected) ?? ""), 40.0f * TextResolution, 3.0f, 1024 - 300);

                ModalsTexture[modal].Add("FolderImage", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Folder.png"));
                ModalsTexture[modal].Add("PathSelected", SingleToTexture(path, 40.0f * TextResolution, 3.0f, Color.BLACK));
                ModalsTexture[modal].Add("DirSelected", SingleToTexture(dirSelected, 45.0f * TextResolution, 3.0f, Color.BLACK));

                // List folder
                SelectFolder_Directories.Clear();

                if(SelectFolder_PathSelected == "")
                {
                    DriveInfo[] drives = DriveInfo.GetDrives();
                    foreach (DriveInfo drive in drives)
                    SelectFolder_Directories.Add(drive.Name);
                }
                else
                SelectFolder_Directories = GetAccessibleDirectories(SelectFolder_PathSelected).ToList();

                // List folder, create textures
                foreach(string directory in SelectFolder_Directories)
                {
                    if(!ModalsTexture.ContainsKey("DIRECTORY_" + directory))
                    {
                        string directoryName = GetLastFolderName(directory) ?? "";
                        directoryName = TruncateTextWithEllipsis(directoryName, 40.0f * TextResolution, 3.0f, 1024 - 230);
                        ModalsTexture[modal].Add("DIRECTORY_" + directory, SingleToTexture(directoryName, 40.0f * TextResolution, 3.0f, Color.BLACK));
                    }
                }
            }
        }

        // Draw components
        // ---------------

        private static void SelectFolder_DrawComponents(string modal, Rectangle modalRect)
        {
            // Textures
            Dictionary<string, Texture2D> textures = ModalsTexture[modal];

            // Folder selected
            Raylib.DrawRectangleLines((int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)), (int)(modalRect.Width - Res(100)), Res(70), Color.BLACK);
            DrawText(textures["PathSelected"], (int)(modalRect.X + Res(70)), (int)(modalRect.Y + Res(65)));
            DrawImage(textures["FolderImage"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(150)), Res(100), Res(100));
            DrawText(textures["DirSelected"], (int)(modalRect.X + Res(180)), (int)(modalRect.Y + Res(170)));

            Vector2 underlineStart = new() { X = (int)(modalRect.X + Res(180)), Y = (int)(modalRect.Y + Res(220)) };
            Vector2 underlineEnd = new() { X = (int)(modalRect.X + Res(180) + Res(textures["DirSelected"].Width)), Y = (int)(modalRect.Y + Res(220)) };
            Raylib.DrawLineEx(underlineStart, underlineEnd, Res(10), Color.BLACK);

            // Folder list container
            Rectangle container = new() { X = modalRect.X + Res(80), Y = modalRect.Y + Res(260), Width = modalRect.Width - Res(130), Height = modalRect.Height - Res(300) };
            HighLightArea = container;
            Raylib.BeginScissorMode((int)container.X, (int)container.Y, (int)container.Width, (int)container.Height);

            // Folder list content
            int y = (int)container.Y;

            foreach(string directory in SelectFolder_Directories)
            {
                Vector2 treeVertlineStart = new() { X = (int)container.X + Res(15), Y = SelectFolder_PosContentY + y - Res(5) };
                Vector2 treeVertlineEnd = new() { X = (int)container.X + Res(15), Y = SelectFolder_PosContentY + y + Res(25) };
                Raylib.DrawLineEx(treeVertlineStart, treeVertlineEnd, Res(5), Color.BLACK);

                Vector2 treeHorilineStart = new() { X = (int)container.X + Res(15), Y = SelectFolder_PosContentY + y + Res(22) };
                Vector2 treeHorilineEnd = new() { X = (int)container.X + Res(35), Y = SelectFolder_PosContentY + y + Res(22) };
                Raylib.DrawLineEx(treeHorilineStart, treeHorilineEnd, Res(5), Color.BLACK);

                DrawText(textures["DIRECTORY_" + directory], (int)(container.X + Res(40)), SelectFolder_PosContentY + y);
                y += Res(60);
            }

            // Set sroll bar
            if(container.Height < y - container.Y)
            {
                SelectFolder_Scroll = true;
                string ScrollBarName = "ScrollFolderList";
                ScrollBarInit(ScrollBarName, 0);

                SelectFolder_PosContentY = -(int)ScrollBarList[ScrollBarName].ContentPosY;
                ScrollBarY(ScrollBarName, (int)container.Height, (int)(y - container.Y), Res(30), (int)container.Height, (int)(container.X + container.Width - Res(30)), (int)container.Y, WhereIAm == "SelectFolder", Color.RAYWHITE, Color.BLACK);
            }

            // Back folder selected
            if (SelectFolder_BackFolder != "")
            {
                int folderSelectedIndex = SelectFolder_Directories.IndexOf(SelectFolder_BackFolder);

                if (folderSelectedIndex != -1 && folderSelectedIndex < ModalHighlight.Count)
                ModalHighlightPos.Y = folderSelectedIndex;

                if (SelectFolder_Scroll)
                ScrollBarSetY("ScrollFolderList", folderSelectedIndex * Res(60));

                SelectFolder_BackFolder = "";
            }

            Raylib.EndScissorMode();
        }

        // Set highlights
        // --------------

        private static void SelectFolder_SetHighlights(string modal, Rectangle modalRect)
        {
            // Variables
            int y = (int)modalRect.Y + Res(250);

            // Draw highlight
            int i = 0;
            foreach(string directory in SelectFolder_Directories)
            {
                int paddingScroll = SelectFolder_Scroll ? Res(30) : 0;
                SetHighLight(modal, "SelectFolderEnter", true, (int)modalRect.X + Res(50), SelectFolder_PosContentY + y, (int)modalRect.Width - Res(SelectFolder_Scroll ? 180 : 100) + paddingScroll, Res(60));
                y += Res(60);

                i++;
            }

            // Move by pad/keyword (hidden lines)
            if (SelectFolder_LastHighLightPosY != ModalHighlightPos.Y && (Input.DPadUp || Input.DPadDown || Input.AxisLeftPadUp || Input.AxisLeftPadDown) && HighLightArea != null)
            {
                string scrollbarName = "ScrollFolderList";
                if (!ScrollBarList.ContainsKey(scrollbarName))
                return;

                Rectangle highlightRect = ModalHighlight[ModalHighlightPos.Y][0].ElmRect;
                int lineDownOverflowY = (int)(highlightRect.Y + highlightRect.Height - (HighLightArea.Value.Y + HighLightArea.Value.Height));
                int lineUpOverflowY = (int)(HighLightArea.Value.Y - highlightRect.Y);
                ScrollBar scrollBar = ScrollBarList[scrollbarName];
                bool direction = SelectFolder_LastHighLightPosY < ModalHighlightPos.Y;

                if (direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
                else if (lineUpOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY - lineUpOverflowY));
                else if (!direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
            }
            SelectFolder_LastHighLightPosY = ModalHighlightPos.Y;
        }
    }
}
