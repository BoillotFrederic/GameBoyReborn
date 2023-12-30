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
        private int TacCycles = 0;
        private readonly int[] ClockCPU = new int[4] { 4096, 262144, 65536, 16384 };

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
                else if (DivCycles >= 256)
                {
                    IO.DIV++;
                    DivCycles -= 256;
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
                            //IME = true;
                        }

                        IO.TIMA = (byte)NewTima;

                        //if (IO.TMA == 0xFF || (IO.TIMA % (0xFF - IO.TMA) == 0))
                        //IME = true;

                        TacCycles -= ClockCPU[ClockSelect];
                    }
                }
            }
        }
    }
}