// ----
// Text
// ----

using Raylib_cs;
using System.Text;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static readonly int TextResolution = 4;
        private static TextureTitleSet[][] TitleTextures = Array.Empty<TextureTitleSet[]>();
        
        private struct TextureTitleSet
        {
            public Texture2D Texture;
            public float Width;
            public float Height;
        }

        private struct TextSet
        {
            public string Text = "";
            public int Width = 0;
            public int Height = 0;
        }

        // Single text
        private static Texture2D SingleToTexture(string textSet, float size, float space, Color color)
        {
            Image textToImg = Raylib.ImageTextEx(MainFont, textSet, size, space, color);
            Texture2D textTexture = Raylib.LoadTextureFromImage(textToImg);
            Raylib.UnloadImage(textToImg);
            Raylib.SetTextureFilter(textTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
            textTexture.Width = textToImg.Width / TextResolution;
            textTexture.Height = textToImg.Height / TextResolution;

            return textTexture;
        }

        // Title game
        private static TextureTitleSet[] TitleGameToTexture(List<TextSet> textSet, float size, float space, Color color)
        {
            TextureTitleSet[] Texture2DArr = new TextureTitleSet[textSet.Count];

            for (int i = 0; i < textSet.Count; i++)
            {
                Image titleToImg = Raylib.ImageTextEx(MainFont, textSet[i].Text, size, space, color);
                Texture2D titleTexture = Raylib.LoadTextureFromImage(titleToImg);
                Raylib.UnloadImage(titleToImg);
                Raylib.SetTextureFilter(titleTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
                Texture2DArr[i].Texture = titleTexture;
                Texture2DArr[i].Width = titleToImg.Width / TextResolution;
                Texture2DArr[i].Height = titleToImg.Height / TextResolution;
            }

            return Texture2DArr;
        }

        // Text wrap and split line
        // ------------------------

        private static List<TextSet> TextNlWrap(string _text, float size, float space, float sizeMax, int limitLine)
        {
            string text = WordWrap(_text, space, size, sizeMax);
            float spaceWidth = GetCharSize((sbyte)' ', size).Width + space;
            float heightSize = 0;
            string[] words = text.Split(' ',StringSplitOptions.RemoveEmptyEntries);
            int nbWord = words.Length;
            float wordSize;
            int nbWordThisLine = 0;
            float lineSize = 0;
            string lineText = "";
            List<TextSet> lines = new();

            for(int w = 0, c = 0; w < nbWord; w++, c++)
            {
                // Word
                wordSize = 0;
                for(int t = 0; t < words[w].Length; t++, c++)
                {
                    CharSize CharSize = GetCharSize((sbyte)text[t], size);
                    if (t + 1 < words[w].Length) CharSize.Width += space;
                    if (heightSize < CharSize.Height) heightSize = CharSize.Height;

                    wordSize += CharSize.Width;
                }

                // Line
                lineSize += wordSize;
                nbWordThisLine++;

                // Add line
                void AddLine(string text, int width)
                {
                    TextSet line = new();
                    line.Text = text;
                    line.Width = width;
                    line.Height = (int)heightSize;
                    lines.Add(line);
                };

                // Width line max
                if(lineSize + (nbWordThisLine * spaceWidth) > sizeMax)
                {
                    AddLine(lineText.Trim(), (int)(lineSize - wordSize + (nbWordThisLine * spaceWidth)));

                    lineText = "";
                    lineSize = wordSize;
                    nbWordThisLine = 0;

                    if (text.Length == c)
                    AddLine(words[w].Trim(), (int)wordSize);
                }
                else if(text.Length == c)
                AddLine((lineText + words[w]).Trim(), (int)(lineSize + (nbWordThisLine * spaceWidth)));

                lineText += words[w] + " ";
            }

            // Limit line
            if(lines.Count > limitLine)
            {
                lines.RemoveRange(limitLine, lines.Count - limitLine);
                int lastIndex = lines.Count - 1;
                var lastItem = lines[lastIndex];
                lastItem.Text += "...";
                lines[lastIndex] = lastItem;
            }

            return lines;
        }


        // Word wrap
        // ---------

        private static string WordWrap(string text, float space, float size, float wordSizeMax)
        {
            string newText = "";
            float wordSize = 0;

            for (int i = 0; i < text.Length; i++)
            {
                // Word detect
                if(text[i] == ' ')
                wordSize = 0;

                // Word size
                CharSize CharSize = GetCharSize((sbyte)text[i], size);

                if (i + 1 < text.Length)
                CharSize.Width += space;

                wordSize += CharSize.Width;

                // Word too long
                if (wordSize > wordSizeMax)
                {
                    wordSize = 0;
                    newText = newText.Remove(newText.Length-1);
                    newText += " " + text[i - 1];
                }

                // New text
                newText += text[i];
            }

            return newText;
        }

        // Convert UTF16 to UTF8
        public static unsafe sbyte* StrToSbyte(string str)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(str);

            fixed (byte* p = bytes)
            {
                sbyte* sp = (sbyte*)p;
                return sp;
            }               
        }

        // Get char size
        // -------------

        private struct CharSize
        {
            public float Width = 0;
            public float Height = 0;
        }

        private static unsafe CharSize GetCharSize(sbyte c, float size)
        {
            float scaleFactor = size / MainFont.BaseSize;
            int codepointByteCount = 0;
            int codepoint = Raylib.GetCodepoint(&c, &codepointByteCount);
            int index = Raylib.GetGlyphIndex(MainFont, codepoint);

            CharSize charSize = new();
            charSize.Width = (MainFont.Glyphs[index].AdvanceX == 0) ? MainFont.Recs[index].Width * scaleFactor : MainFont.Glyphs[index].AdvanceX * scaleFactor;
            charSize.Height = MainFont.Recs[index].Height * scaleFactor;

            return charSize;
        }
    }
}
