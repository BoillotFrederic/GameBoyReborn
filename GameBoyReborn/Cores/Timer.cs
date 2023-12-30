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

        // Cycles
        private ushort DivCycles = 0;
        private ushort TacCycles = 0;
        private readonly ushort[] ClockCPU = new ushort[4] { 69, 4389, 1097, 274 };
        private const int DivClockDMG = 274; // 16384 / 59.73 = 274.3
        private const int DivClockSGB = 281; // 16779 / 59.73 = 280.9

        // Execution
        public void Execution()
        {
            if (IO != null)
            {
                // DIV
                DivCycles += CPU.Cycles;

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
                byte ClockSelect = (byte)(IO.TAC & 3);

                if (Binary.ReadBit(IO.TAC, 2))
                {
                    if (TacCycles > ClockCPU[ClockSelect])
                    {
                        ushort NewTima = (ushort)(IO.TIMA + 1);

                        if (NewTima > 0xFF)
                        {
                            NewTima = 0;
                            IO.TMA = 0;

                            Binary.SetBit(ref IO.IE, 2, true);
                        }

                        IO.TIMA = (byte)NewTima;

                        if (IO.TMA == 0xFF || (IO.TIMA % (0xFF - IO.TMA) == 0))
                        Binary.SetBit(ref IO.IE, 2, true);

                        TacCycles -= ClockCPU[ClockSelect];
                    }
                }
                else
                TacCycles = 0;
            }
        }
    }
}