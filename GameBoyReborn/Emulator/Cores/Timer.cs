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
        private ushort DivCycles = 0;
        private ushort TacCycles = 0;
        private const int DivClock = 256;
        private ushort TacClock = 0;

        // TIMER IO Write
        public void DIV()
        {
            IO.DIV = 0x00;
            //Enable = false;
        }

        public void TAC(byte b)
        {
            SetTIMAClockFrequency(GetTIMAClockFrequency(b));

            if (Binary.ReadBit(b, 2))
            {
                //IO.TIMA = IO.TMA;
                Enable = true;
            }
            else
            Enable = false;
        }

        private static int GetTIMAClockFrequency(byte b)
        {
            int clockSelect = b & 0x03;
            return clockSelect switch
            {
                0 => 4096, // 4096 Hz
                1 => 262144, // 262144 Hz
                2 => 65536, // 65536 Hz
                3 => 16384, // 16384 Hz
                _ => 4096, // Default to 4096 Hz
            };
        }

        private void SetTIMAClockFrequency(int clockFrequency)
        {
            int cyclesPerIncrement = CPU.Frequency / clockFrequency;
            TacClock = (ushort)cyclesPerIncrement;
        }

        public void Execution()
        {
            UpdateDIV();

            if (Enable)
            UpdateTIMA();
        }

        private void UpdateDIV()
        {
            DivCycles += (byte)(CPU.Cycles * 4);

            if (DivCycles >= DivClock)
            {
                IO.DIV++;
                DivCycles = 0;
            }
        }

        private void UpdateTIMA()
        {
            TacCycles += (byte)(CPU.Cycles * 4);

            if (TacCycles >= TacClock)
            {
                ushort NewTima = (ushort)(IO.TIMA + 1);

                if (NewTima > 0xFF)
                {
                    NewTima = IO.TMA;
                    Binary.SetBit(ref IO.IF, 2, true);
                }

                IO.TIMA = (byte)NewTima;
                TacCycles = 0;
            }
        }
    }
}