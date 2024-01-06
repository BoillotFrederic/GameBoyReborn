// ---------
// Emulation
// ---------

namespace GameBoyReborn
{
    public static class Emulation
    {
        private static byte[]? romData;
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
            if (romData != null && romData.Length != 0)
            {
                byte[] header = new byte[0x1C];

                if (romData.Length > 0x0134)
                Array.Copy(romData, 0x0134, header, 0, 0x1C);

                // Cores instance
                IO = new IO();
                Cartridge = new Cartridge(header);
                Memory = new Memory(Cartridge, IO, romData);
                CPU = new CPU(Memory);
                PPU = new PPU(IO, Memory, CPU);
                APU = new APU(IO, CPU, PPU);
                Timer = new Timer(IO, CPU);

                // Relation
                IO.APU = APU;
                Memory.CPU = CPU;

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
            romData = File.ReadAllBytes(path);
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