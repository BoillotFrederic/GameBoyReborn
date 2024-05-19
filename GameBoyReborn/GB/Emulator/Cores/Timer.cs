// -----
// Timer
// -----

using GameBoyReborn;

namespace Emulator
{
    public class Timer
    {
        // Construct
        private readonly IO IO;
        private readonly CPU CPU;

        public Timer(Emulation Emulation)
        {
            IO = Emulation.IO;
            CPU = Emulation.CPU;
        }

        private bool Enable = false;
        private short DivCycles = 0;
        private short TacCycles = 0;
        private short LastTacCycles = 0;
        private const int DivClock = 256;
        private ushort TacClock = 0;
        private ushort LastTacClock = 0;

        /// <summary>
        /// Timer ticks
        /// </summary>
        public void Execution()
        {
            UpdateDIV();
            UpdateTIMA();
        }

        // IO Write registers
        // ------------------

        /// <summary>
        /// Divider register
        /// </summary>
        public void DIV()
        {
            if (TacCycles == 0)
            IO.TIMA++;

            TacCycles = 0;
        }

        /// <summary>
        /// Timer control
        /// </summary>
        public void TAC(byte b)
        {
            SetTIMAClockFrequency(GetTIMAClockFrequency(b));
            Enable = Binary.ReadBit(b, 2);
        }


        // TIMA frequency
        // --------------
        private static int GetTIMAClockFrequency(byte b)
        {
            return (b & 0x03) switch
            {
                0 => 4096,
                1 => 262144,
                2 => 65536,
                3 => 16384,
                _ => 4096,
            };
        }

        private void SetTIMAClockFrequency(int clockFrequency)
        {
            LastTacClock = TacClock;
            int cyclesPerIncrement = CPU.Frequency / clockFrequency;
            TacClock = (ushort)cyclesPerIncrement;
        }


        /// <summary>
        /// Increment DIV
        /// </summary>
        private void UpdateDIV()
        {
            DivCycles += (byte)(CPU.Cycles * 4);

            if (DivCycles >= DivClock)
            {
                IO.DIV++;
                DivCycles -= DivCycles;
            }
        }

        /// <summary>
        /// Increment TIMA
        /// </summary>
        private void UpdateTIMA()
        {
            LastTacCycles = TacCycles;
            TacCycles += (byte)(CPU.Cycles * 4);

            if (TacCycles >= TacClock)
            {
                TacCycles -= TacCycles;
                if (Enable && LastTacCycles > 0)
                {
                    IO.TIMA++;
                    if (IO.TIMA == 0)
                    {
                        IO.TIMA = IO.TMA;
                        Binary.SetBit(ref IO.IF, 2, true);
                    }
                }
            }
        }
    }
}