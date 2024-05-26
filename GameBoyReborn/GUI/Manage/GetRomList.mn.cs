// ------------
// Get rom list
// ------------

using System.Text.RegularExpressions;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private struct Game
        {
            public string Path = "";
            public string Name = "";
            public string Cover = "";
            public string[] Infos = Array.Empty<string>();
            public string[] Tags = Array.Empty<string>();
        }

        private static void GetGameListPath()
        {
            List<string> ext = new(){ "gb", "gbc", "sgb" };
            IEnumerable<string> Games = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "Roms", "*.*", SearchOption.AllDirectories).Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            NbGame = Games.Count();
            GameList = new Game[NbGame];
            TitleTextures = new TextureTitleSet[NbGame][];

            for(int i = 0; i < Games.Count(); i++)
            {
                // Paths
                GameList[i].Path = Games.ElementAt(i);

                // Names
                Match match = Regex.Match(Games.ElementAt(i), @".*\\([^(\[]+).*\.(gb|sgb|gbc)$", RegexOptions.IgnoreCase);
                GameList[i].Name = match.Groups[1].Value.Trim();

                // Infos
                MatchCollection Infos = Regex.Matches(Games.ElementAt(i), @"\(([^)]+)\)");
                GameList[i].Infos = new string[Infos.Count];
                for (int l = 0; l < Infos.Count; l++)
                GameList[i].Infos[l] = Infos[l].Groups[1].Value;

                // Tags
                MatchCollection Tags = Regex.Matches(Games.ElementAt(i), @"\[([^]]+)\]");
                GameList[i].Tags = new string[Tags.Count];
                for (int l = 0; l < Tags.Count; l++)
                GameList[i].Tags[l] = Tags[l].Groups[1].Value;
            }
        }
    }
}
