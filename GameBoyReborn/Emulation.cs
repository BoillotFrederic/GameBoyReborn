// ---------
// Emulation
// ---------

namespace GameBoyReborn
{
    public static class Emulation
    {
        private static byte[]? RomData;
        private static Cartridge? Cartridge;
        private static IO? IO;
        private static Memory? Memory;
        private static CPU? CPU;
        private static PPU? PPU;
        private static APU? APU;
        private static Timer? Timer;

        // All cores init
        public static void Init()
        {
            if (RomData != null && RomData.Length != 0)
            {
                // Cores instance
                IO = new IO();
                Cartridge = new Cartridge(RomData.ToArray());
                Memory = new Memory(Cartridge, IO, RomData.ToArray());
                CPU = new CPU(IO, Memory);
                PPU = new PPU(IO, Memory, CPU);
                APU = new APU(IO, CPU, PPU);
                Timer = new Timer(IO, CPU);

                // Relation
                IO.APU = APU;
                IO.PPU = PPU;
                IO.Timer = Timer;
                IO.Cartridge = Cartridge;
                Memory.CPU = CPU;
                Memory.PPU = PPU;

                // Log rom loaded
                Console.WriteLine("Load rom");
                Console.WriteLine("--------");
                Console.WriteLine();
                Console.WriteLine("Title : " + Cartridge.Title);
                Console.WriteLine("Manufacturer Code : " + Cartridge.ManufacturerCode);
                Console.WriteLine("CGB Description : " + Cartridge.CGBDescription);
                Console.WriteLine("Licensee : " + Cartridge.Licensee);
                Console.WriteLine("SGB Description : " + Cartridge.SGBDescription);
                Console.WriteLine("Type Description : " + Cartridge.TypeDescription);
                Console.WriteLine("Size Description : " + Cartridge.SizeDescription);
                Console.WriteLine();
            }
        }

        // Load rom
        public static void Load(string path)
        {
            if (File.Exists(path))
            RomData = File.ReadAllBytes(path);

            else
            Console.WriteLine("ROM not found");
        }

        // Emulation loop
        public static void Loop()
        {
            if (CPU != null && PPU != null && APU != null && Timer != null)
            {
                PPU.CompletedFrame = false;

                while (!PPU.CompletedFrame)
                {
                    CPU.Execution();
                    PPU.Execution();
                    APU.Execution();
                    Timer.Execution();
                }
            }
        }

        // Stop emulation
        public static void Stop()
        {
            IO = null;
            Cartridge = null;
            Memory = null;
            CPU = null;
            PPU = null;
            APU = null;
            Timer = null;
        }
    }
}