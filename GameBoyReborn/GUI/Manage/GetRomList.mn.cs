// ------------
// Get rom list
// ------------

using System.Text.RegularExpressions;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;

namespace GameBoyReborn
{
    // Game set
    public class Game
    {
        public string Path { get; set; } = "";
        public string Name { get; set; } = "";
        public string Cover { get; set; } = "";
        public string ZippedFile { get; set; } = "";
    }

    public partial class DrawGUI
    {
        // Write game list
        private static async Task WriteGameList()
        {
            await Task.Run(() =>
            {
                SearchOption searchOption = Program.AppConfig.ScanListRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                List<string> ext = new(){ "gb", "gbc", "sgb", "zip", "7z", "7zip", "rar" };
                IEnumerable<string> Games = Directory.EnumerateFiles((string)Program.AppConfig.PathRoms, "*.*", searchOption).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));
                int nbFiles = Games.Count();
                List<Game> gameList = new();

                for(int i = 0; i < nbFiles; i++)
                {
                    // Add entry
                    Game game = new();

                    // Paths
                    game.Path = Games.ElementAt(i);

                    // Names
                    Match match = Regex.Match(game.Path, @".*\\([^(\[]+).*\.(gb|sgb|gbc|zip|7z|7zip|rar)$", RegexOptions.IgnoreCase);
                    game.Name = match.Groups[1].Value.Trim();

                    // Zipped file
                    string extFile = match.Groups[2].Value;
                    if(Regex.Match(extFile, @"zip|7z|7zip|rar", RegexOptions.IgnoreCase).Success)
                    {
                        // Open archive
                        IArchive? archive = null;

                        if(Regex.Match(extFile, @"zip", RegexOptions.IgnoreCase).Success)
                        archive = ZipArchive.Open(game.Path);
                        else if(Regex.Match(extFile, @"7z|7zip", RegexOptions.IgnoreCase).Success)
                        archive = SevenZipArchive.Open(game.Path);
                        else if(Regex.Match(extFile, @"rar", RegexOptions.IgnoreCase).Success)
                        archive = RarArchive.Open(game.Path);

                        // Filter content
                        if (archive != null)
                        {
                            if(archive.Entries.Any())
                            {
                                string achiveFileSelected = "";
                                static bool checkExt(string? path) { return Regex.Match(path ?? "", @".*\.(?:gb|sgb|gbc)$", RegexOptions.IgnoreCase).Success; };
                                static bool hookTagPriority(string? path) { return Regex.Match(path ?? "", Regex.Escape(Program.AppConfig.HookTagPriority), RegexOptions.IgnoreCase).Success; };
                                static bool bracketsTagPriority(string? path) { return Regex.Match(path ?? "", Regex.Escape(Program.AppConfig.BracketsTagPriority), RegexOptions.IgnoreCase).Success; };

                                // Check extensions and isn't a directory
                                IEnumerable<IArchiveEntry>? filteredExtDirEntries = archive.Entries.Where(e => checkExt(e.Key) && !e.IsDirectory);

                                // Priorites
                                IEnumerable<IArchiveEntry>? filteredEntriesPriority = filteredExtDirEntries.Where(e => hookTagPriority(e.Key) && bracketsTagPriority(e.Key));

                                if(!filteredEntriesPriority.Any())
                                filteredEntriesPriority = filteredExtDirEntries.Where(e => hookTagPriority(e.Key));

                                if(!filteredEntriesPriority.Any())
                                filteredEntriesPriority = filteredExtDirEntries.Where(e => bracketsTagPriority(e.Key));

                                // Reorder
                                filteredEntriesPriority = filteredEntriesPriority.OrderBy(e => e.Key, new AlphanumericComparer());
                                filteredExtDirEntries = filteredExtDirEntries.OrderBy(e => e.Key, new AlphanumericComparer());

                                // Select file
                                if (!filteredEntriesPriority.Any() && filteredExtDirEntries.Any())
                                {
                                    IEnumerable<IArchiveEntry>? filteredNoTags = filteredExtDirEntries.Where(e => !Regex.Match(e.Key ?? "", @"\[|\(", RegexOptions.IgnoreCase).Success);

                                    if(filteredNoTags.Any())
                                    achiveFileSelected = filteredNoTags.First().Key ?? "";
                                    else
                                    achiveFileSelected = filteredExtDirEntries.First().Key ?? "";

                                    game.ZippedFile = GetRelativePath(achiveFileSelected);
                                    gameList.Add(game);
                                }
                                else if(filteredEntriesPriority.Any())
                                {
                                    achiveFileSelected = filteredEntriesPriority.First().Key ?? "";
                                    game.ZippedFile = GetRelativePath(achiveFileSelected);
                                    gameList.Add(game);
                                }
                            }

                        }
                    }
                    // File only
                    else
                    gameList.Add(game);

                    Loading_Percent = (float)i / nbFiles;
                }

                // Write list config file
                ConfigJson.Save("Config/ListGameConfig.json", gameList);
            });
        }

        // Read game list
        private static void ReadGameList()
        {
            GameList = ConfigJson.LoadListGameConfig().ToArray();
            GameList = GameList.OrderBy(e => e.Name, new AlphanumericComparer()).ToArray();
            NbGame = GameList.Length;
        }

        // Get last folder name
        public static string? GetLastFolderName(string path)
        {
            string? lastFolderName;

            path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            lastFolderName = Path.GetFileName(path);

            if (string.IsNullOrEmpty(lastFolderName))
            {
                lastFolderName = Path.GetPathRoot(path);

                if (lastFolderName != null)
                lastFolderName = lastFolderName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return lastFolderName;
        }

        // Get relative path
        private static string GetRelativePath(string path)
        {
            string[]? parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            return string.Join("/", parts.Skip(1));

            return path;
        }

        // Get directories list
        public static List<string> GetAccessibleDirectories(string path)
        {
            List<string> accessibleDirectories = new();

            try
            {
                string[] directories = Directory.GetDirectories(path);

                foreach (string dir in directories)
                {
                    try
                    {
                        Directory.GetDirectories(dir);
                        accessibleDirectories.Add(dir);
                    }
                    catch(Exception) { }
                }
            }
            catch (Exception) {}

            return accessibleDirectories;
        }

        // Get parent directory
        public static string GetParentDirectory(string path)
        {
            DirectoryInfo directoryInfo = new(path);
            DirectoryInfo? parentDirectory = directoryInfo.Parent;

            if (parentDirectory != null)
            return parentDirectory.FullName;
            else
            return "";
        }

        // Order management
        // ----------------

        // Alphanumeric
        public class AlphanumericComparer : IComparer<string?>
        {
            public int Compare(string? x, string? y)
            {
                if (x == null || y == null)
                return string.Compare(x, y);

                string[] xParts = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
                string[] yParts = Regex.Split(y.Replace(" ", ""), "([0-9]+)");

                for (int i = 0; i < xParts.Length && i < yParts.Length; i++)
                {
                    if (xParts[i] != yParts[i])
                    {
                        if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
                        return xNum.CompareTo(yNum);

                        return xParts[i].CompareTo(yParts[i]);
                    }
                }

                return xParts.Length.CompareTo(yParts.Length);
            }
        }
    }
}
