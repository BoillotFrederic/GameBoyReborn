// -----
// Timer
// -----

namespace GameBoyReborn
{
    public class Timer
    {
        // Construct
        private readonly IO IO;
        private readonly CPU CPU;

        public Timer(IO _IO, CPU _CPU)
        {
            IO = _IO;
            CPU = _CPU;
        }

        // TIMER IO Write
        public void DIV(byte b)
        {
            IO.DIV = 0x00;
        }

        public void TAC(byte b)
        {
            IO.TAC = b;

            Enable = Binary.ReadBit(IO.TAC, 2);
            TmaClock = IO.TAC & 3 switch
            {
                0 => 69,
                1 => 4389,
                2 => 1097,
                3 => 274,
                _ => 69,
            };
        }

        // TAC - Timer Control
        private int TmaClock = 69;
        private bool Enable = false;

        // Cycles
        private ushort DivCycles = 0;
        private ushort TacCycles = 0;
        private const int DivClockDMG = 274; // 16384 / 59.73 = 274.3
        private const int DivClockSGB = 281; // 16779 / 59.73 = 280.9

        // Execution
        public void Execution()
        {
            if (IO != null)
            {
                // DIV
                DivCycles += (ushort)(CPU.Cycles * 4);

                if (CPU.Cycles == 0 || CPU.Stop)
                {
                    IO.DIV = 0;
                    DivCycles = 0;
                }
                else if (DivCycles >= DivClockDMG)
                {
                    IO.DIV++;
                    DivCycles -= DivClockDMG;
                }

                // TIMA and TMA
                TacCycles += CPU.Cycles;

                if (Enable)
                {
                    if (TacCycles > TmaClock)
                    {
                        ushort NewTima = (ushort)(IO.TIMA + 1);

                        if (NewTima > 0xFF)
                        {
                            NewTima = IO.TMA;
                            IO.TMA = 0;
                            Binary.SetBit(ref IO.IF, 2, true);
                        }

                        IO.TIMA = (byte)NewTima;

                        if (IO.TMA == 0xFF || (IO.TIMA % (0xFF - IO.TMA) == 0))
                        Binary.SetBit(ref IO.IF, 2, true);

                        TacCycles -= (ushort)TmaClock;
                    }
                }
                else
                TacCycles = 0;
            }
        }
    }
}