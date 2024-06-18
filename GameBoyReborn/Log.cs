// ---
// Log
// ---
using System.Text;

namespace GameBoyReborn
{
    public class Log
    {
        // Log file
        private static StreamWriter? logStreamWriter;
        private static TextWriter? originalConsoleOut;
        private static TextWriter? multiTextWriter;
        public static bool ConsoleEnable { get; set; } = false;

        // Start console out
        public static void Start()
        {
            Init();
            ConsoleEnable = true;

            if (logStreamWriter == null && originalConsoleOut == null)
            {
                logStreamWriter = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log.txt", append: false) { AutoFlush = true };
                originalConsoleOut = Console.Out;
                multiTextWriter = new MultiTextWriter(logStreamWriter, originalConsoleOut);
            }

            if (multiTextWriter != null)
            {
                Console.SetOut(multiTextWriter);
            }
        }

        // Close console out and file
        public static void Close()
        {
            if (logStreamWriter != null && originalConsoleOut != null)
            {
                logStreamWriter.Close();
                Console.SetOut(originalConsoleOut);
                originalConsoleOut.Close();
            }
        }

        // Reinitialize console streams
        public static void Init()
        {
            if (logStreamWriter != null && originalConsoleOut != null)
            {
                Stream? stdout = Console.OpenStandardOutput();

                multiTextWriter = new MultiTextWriter(new StreamWriter(stdout, Encoding.Default) { AutoFlush = true }, originalConsoleOut);
                Console.SetOut(multiTextWriter);
            }
        }

        // Write log
        public static void Write(string? message = "")
        {
            if (ConsoleEnable)
            {
                try
                {
                    Console.WriteLine(message);
                }
                catch (IOException)
                {
                    logStreamWriter?.WriteLine(message);
                }
            }
            else
            logStreamWriter?.WriteLine(message);
        }

        public static void Write(bool? message)
        {
            Write(message?.ToString());
        }

        public static void Write(int? message)
        {
            Write(message?.ToString());
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
                foreach (TextWriter writer in writers)
                {
                    writer.Write(value);
                }
            }

            public override void WriteLine(string? value)
            {
                foreach (TextWriter writer in writers)
                {
                    writer.WriteLine(value);
                }
            }

            public override Encoding Encoding => Encoding.Default;
        }
    }
}