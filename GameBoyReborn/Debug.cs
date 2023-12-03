// ----------
// DEBUG MODE
// ----------
using Raylib_cs;

public static class Debug
{
    private static List<TextStruct> TextQueue = new List<TextStruct>();
    private static List<int> TextToRemove = new List<int>();

    // Text structure
    public class TextStruct
    {
        public string text = "";
        public int remainingTime;
        public Color color;
    }

    // Add text to the screen
    public static void Text(string text, Color color, int remainingTime)
    {
        TextStruct newLine = new TextStruct
        {
            text = text,
            remainingTime = remainingTime,
            color = color
        };

        TextQueue.Add(newLine);
    }

    // Update texts
    public static void UpdateTexts()
    {
        TextToRemove.Clear();

        for (int i = 0; i < TextQueue.Count; i++)
        {
            Raylib.DrawText(TextQueue[i].text, 10, 10 + (i * 20), 20, TextQueue[i].color);
            TextQueue[i].remainingTime -= 16;

            if (TextQueue[i].remainingTime <= 0)
            TextToRemove.Add(i);
        }

        foreach (int index in TextToRemove)
        TextQueue.RemoveAt(index);
    }
}
