// ---
// Log
// ---
using System.Text;

namespace GameBoyReborn
{
    public class Log
    {
        // Log file
        private readonly static StreamWriter logStreamWriter = new(AppDomain.CurrentDomain.BaseDirectory + "Log.txt");
        private readonly static TextWriter originalConsoleOut = Console.Out;

        // Start console out
        public static void Start()
        {
            Console.SetOut(new MultiTextWriter(logStreamWriter, originalConsoleOut));
        }

        // Close console out and file
        public static void Close()
        {
            logStreamWriter.Close();
            Console.SetOut(originalConsoleOut);
        }

        // MultiTextWriter
        private class MultiTextWriter : TextWriter
        {
            private readonly List<TextWriter> writers;

            public MultiTextWriter(params TextWriter[] writers)
            {
                this.writers = new List<TextWriter>(writers);
            }

            public override void Write(char value)
            {
                foreach (var writer in writers)
                {
                    writer.Write(value);
                }
            }

            public override Encoding Encoding => Encoding.Default;
        }
    }
}