// ----
// Text
// ----

using Raylib_cs;
using System.Text;
using System.Numerics;

namespace GameBoyReborn
{
    public partial class DrawGUI
    {
        private static readonly int TextResolution = 4;
        private const float SpacingFixFactor = 3.2f;
        
        private struct TextureTitleSet
        {
            public Texture2D Texture;
            public float Width;
            public float Height;
        }

        private struct TitleSet
        {
            public string Text;
            public float Width;
            public float Height;

            public TitleSet(string text = "", float width = 0, float height = 0)
            {
                Text = text;
                Width = width;
                Height = height;
            }
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

        // Text truncate
        private static string TruncateTextWithEllipsis(string text, float fontSize, float spacing, int maxWidth)
        {
            // Checking if the text does not exceed the requested width
            Vector2 textSize = Raylib.MeasureTextEx(MainFont, text, fontSize, spacing * SpacingFixFactor);

            if (textSize.X / TextResolution <= maxWidth)
            return text;

            // Otherwise the text is too big so we insert an ellipsis
            string ellipsis = "...";
            string part1 = "";
            string part2 = "";
            Vector2 ellipsisSize = Raylib.MeasureTextEx(MainFont, ellipsis, fontSize, spacing * SpacingFixFactor);

            for (int i = 0, ii = text.Length; i < text.Length; i++, ii--)
            {
                if (i < 10)
                part1 += text[i];

                else
                {
                    string part2Temp = text[ii + 9] + part2;

                    Vector2 sizePart1 = Raylib.MeasureTextEx(MainFont, part1, fontSize, spacing * SpacingFixFactor);
                    Vector2 sizePart2 = Raylib.MeasureTextEx(MainFont, part2Temp, fontSize, spacing * SpacingFixFactor);

                    int sizePart2Max = maxWidth - (int)sizePart1.X / TextResolution - (int)ellipsisSize.X / TextResolution;

                    if(sizePart2.X / TextResolution < sizePart2Max) part2 = part2Temp;
                    else break;
                }
            }

            // Return the new string
            return part1 + ellipsis + part2;
        }

        // Title game
        private static TextureTitleSet[] TitleGameToTexture(List<string> textSet, float size, float space, Color color)
        {
            var Texture2DArr = new TextureTitleSet[textSet.Count];

            for (int i = 0; i < textSet.Count; i++)
            {
                Image titleToImg = Raylib.ImageTextEx(MainFont, textSet[i], size, space, color);
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

        private static List<string> TextNlWrap(string _text, float size, float space, float sizeMax, int limitLine)
        {
            // Text
            string[] text = _text.Split(' ');
            List<string> lines = new();

            // Create lines
            string words = "";

            for (int i = 0; i < text.Length; i++)
            {
                string lastWords = words;
                string newWords = words.Length > 0 ? words + " " + text[i] : text[i]; // Manage spaces correctly
                float wordsSize = Raylib.MeasureTextEx(MainFont, newWords, size, space * SpacingFixFactor).X;

                if (wordsSize > sizeMax)
                {
                    if (!string.IsNullOrEmpty(lastWords))
                    {
                        if (limitLine != 0 && lines.Count + 1 >= limitLine)
                        {
                            lines.Add(lastWords.Trim() + "...");
                            break;
                        }
                        else
                        {
                            lines.Add(lastWords.Trim());
                            words = text[i];
                        }
                    }
                    else
                    {
                        if (limitLine != 0 && lines.Count + 1 >= limitLine)
                        {
                            lines.Add(newWords.Trim() + "...");
                            break;
                        }
                        else
                        {
                            lines.Add(newWords.Trim());
                            words = "";
                        }
                    }
                }
                else
                words = newWords;
            }

            // Add the last remaining words if any, and check the limit
            if (!string.IsNullOrEmpty(words))
            {
                if (limitLine == 0 || lines.Count < limitLine)
                lines.Add(words.Trim());
            }

            // Ensure the first line is not empty
            if (lines.Count > 0 && lines[0] == "")
            lines[0] = text[0];

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
            public float Width;
            public float Height;

            public CharSize(float width = 0, float height = 0)
            {
                Width = width;
                Height = height;
            }
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
