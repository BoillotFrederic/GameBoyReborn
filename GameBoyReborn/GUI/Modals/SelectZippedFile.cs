// -------------------
// Modal Select Folder
// -------------------

using Raylib_cs;
using System.Numerics;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;
using System.Text.RegularExpressions;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        // Set modal size
        // --------------

        private const int SelectZippedFile_ModalWidth = 1024;
        private const int SelectZippedFile_ModalHeight = 950;
        private static string? SelectZippedFile_PathArchive;
        private static string? SelectZippedFile_Action;
        private static string SelectZippedFile_ParentName = "";
        private static int SelectZippedFile_PosContentY = 0;
        private static int SelectZippedFile_LastHighLightPosY = 0;
        private static bool SelectZippedFile_Scroll;
        private static readonly List<string> SelectZippedFile_Files = new();

        // Set textures
        // ------------

        private static void SelectZippedFile_SetTextures(string modal)
        {
            if(SelectZippedFile_PathArchive != null)
            {
                string path = TruncateTextWithEllipsis(SelectZippedFile_PathArchive, 40.0f * TextResolution, 3.0f, 1024 - 140);
                string archiveName = TruncateTextWithEllipsis(GetLastFolderName(SelectZippedFile_PathArchive) ?? "", 40.0f * TextResolution, 3.0f, 1024 - 300);

                ModalsTexture[modal].Add("ArchiveImage", Raylib.LoadTexture(AppDomain.CurrentDomain.BaseDirectory + "Textures/Archive.png"));
                ModalsTexture[modal].Add("ArchiveName", SingleToTexture(archiveName, 45.0f * TextResolution, 3.0f, Color.BLACK));



                Match match = Regex.Match(SelectZippedFile_PathArchive, @".*\\([^(\[]+).*\.(gb|sgb|gbc|zip|7z|7zip|rar)$", RegexOptions.IgnoreCase);
                string extFile = match.Groups[2].Value;
                if (Regex.Match(extFile, @"zip|7z|7zip|rar", RegexOptions.IgnoreCase).Success)
                {
                    // Open archive
                    IArchive? archive = null;

                    if (Regex.Match(extFile, @"zip", RegexOptions.IgnoreCase).Success)
                    archive = ZipArchive.Open(SelectZippedFile_PathArchive);
                    else if (Regex.Match(extFile, @"7z|7zip", RegexOptions.IgnoreCase).Success)
                    archive = SevenZipArchive.Open(SelectZippedFile_PathArchive);
                    else if (Regex.Match(extFile, @"rar", RegexOptions.IgnoreCase).Success)
                    archive = RarArchive.Open(SelectZippedFile_PathArchive);

                    // Filter content
                    if (archive != null)
                    {
                        if (archive.Entries.Any())
                        {
                            static bool checkExt(string? path) { return Regex.Match(path ?? "", @".*\.(?:gb|sgb|gbc)$", RegexOptions.IgnoreCase).Success; };

                            // Check extensions
                            IEnumerable<IArchiveEntry>? filteredExtDirEntries = archive.Entries.Where(e => checkExt(e.Key));

                            foreach(IArchiveEntry file in filteredExtDirEntries)
                            {
                                if(!ModalsTexture.ContainsKey("ZIPPED_" + file.Key))
                                {
                                    SelectZippedFile_Files.Add(file.Key ?? "");
                                    string fileName = file.Key ?? "";
                                    fileName = TruncateTextWithEllipsis(fileName, 40.0f * TextResolution, 3.0f, 1024 - 230);
                                    ModalsTexture[modal].Add("ZIPPED_" + file.Key, SingleToTexture(fileName, 40.0f * TextResolution, 3.0f, Color.BLACK));
                                }
                            }
                        }
                    }
                }
            }
        }

        // Draw components
        // ---------------

        private static void SelectZippedFile_DrawComponents(string modal, Rectangle modalRect)
        {
            // Textures
            Dictionary<string, Texture2D> textures = ModalsTexture[modal];

            // Folder selected
            DrawImage(textures["ArchiveImage"], (int)(modalRect.X + Res(50)), (int)(modalRect.Y + Res(50)), Res(100), Res(100));
            DrawText(textures["ArchiveName"], (int)(modalRect.X + Res(180)), (int)(modalRect.Y + Res(70)));

            Vector2 underlineStart = new() { X = (int)(modalRect.X + Res(180)), Y = (int)(modalRect.Y + Res(120)) };
            Vector2 underlineEnd = new() { X = (int)(modalRect.X + Res(180) + Res(textures["ArchiveName"].Width)), Y = (int)(modalRect.Y + Res(120)) };
            Raylib.DrawLineEx(underlineStart, underlineEnd, Res(10), Color.BLACK);

            // Folder list container
            Rectangle container = new() { X = modalRect.X + Res(80), Y = modalRect.Y + Res(160), Width = modalRect.Width - Res(130), Height = modalRect.Height - Res(200) };
            HighLightArea = container;
            Raylib.BeginScissorMode((int)container.X, (int)container.Y, (int)container.Width, (int)container.Height);

            // Folder list content
            int y = (int)container.Y;

            foreach(string file in SelectZippedFile_Files)
            {
                Vector2 treeVertlineStart = new() { X = (int)container.X + Res(15), Y = SelectZippedFile_PosContentY + y - Res(5) };
                Vector2 treeVertlineEnd = new() { X = (int)container.X + Res(15), Y = SelectZippedFile_PosContentY + y + Res(25) };
                Raylib.DrawLineEx(treeVertlineStart, treeVertlineEnd, Res(5), Color.BLACK);

                Vector2 treeHorilineStart = new() { X = (int)container.X + Res(15), Y = SelectZippedFile_PosContentY + y + Res(22) };
                Vector2 treeHorilineEnd = new() { X = (int)container.X + Res(35), Y = SelectZippedFile_PosContentY + y + Res(22) };
                Raylib.DrawLineEx(treeHorilineStart, treeHorilineEnd, Res(5), Color.BLACK);

                DrawText(textures["ZIPPED_" + file], (int)(container.X + Res(40)), SelectZippedFile_PosContentY + y);
                y += Res(60);
            }

            // Set sroll bar
            if(container.Height < y - container.Y)
            {
                SelectZippedFile_Scroll = true;
                string ScrollBarName = "ScrollFilesList";
                ScrollBarInit(ScrollBarName, 0);

                SelectZippedFile_PosContentY = -(int)ScrollBarList[ScrollBarName].ContentPosY;
                ScrollBarY(ScrollBarName, (int)container.Height, (int)(y - container.Y), Res(30), (int)container.Height, (int)(container.X + container.Width - Res(30)), (int)container.Y, WhereIAm == "SelectZippedFile", Color.RAYWHITE, Color.BLACK);
            }

            Raylib.EndScissorMode();
        }

        // Set highlights
        // --------------

        private static void SelectZippedFile_SetHighlights(string modal, Rectangle modalRect)
        {
            // Variables
            int y = (int)modalRect.Y + Res(150);

            // Draw highlight
            int i = 0;
            foreach(string directory in SelectZippedFile_Files)
            {
                int paddingScroll = SelectZippedFile_Scroll ? Res(30) : 0;
                SetHighLight(modal, SelectZippedFile_Action ?? "", true, (int)modalRect.X + Res(50), SelectZippedFile_PosContentY + y, (int)modalRect.Width - Res(SelectZippedFile_Scroll ? 180 : 100) + paddingScroll, Res(60));
                y += Res(60);

                i++;
            }

            // Move by pad/keyword (hidden lines)
            if (SelectZippedFile_LastHighLightPosY != ModalHighlightPos.Y && (Input.DPadUp || Input.DPadDown || Input.AxisLeftPadUp || Input.AxisLeftPadDown) && HighLightArea != null)
            {
                string scrollbarName = "ScrollFilesList";
                if (!ScrollBarList.ContainsKey(scrollbarName))
                return;

                Rectangle highlightRect = ModalHighlight[ModalHighlightPos.Y][0].ElmRect;
                int lineDownOverflowY = (int)(highlightRect.Y + highlightRect.Height - (HighLightArea.Value.Y + HighLightArea.Value.Height));
                int lineUpOverflowY = (int)(HighLightArea.Value.Y - highlightRect.Y);
                ScrollBar scrollBar = ScrollBarList[scrollbarName];
                bool direction = SelectZippedFile_LastHighLightPosY < ModalHighlightPos.Y;

                if (direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
                else if (lineUpOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY - lineUpOverflowY));
                else if (!direction && lineDownOverflowY > 0)
                ScrollBarSetY(scrollbarName, (int)(scrollBar.ContentPosY + lineDownOverflowY));
            }
            SelectZippedFile_LastHighLightPosY = ModalHighlightPos.Y;
        }
    }
}
