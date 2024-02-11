// ---------
// Emulation
// ---------

#pragma warning disable CS8618

namespace Emulator
{
    public class Emulation
    {
        // Construct
        public byte[] RomData;
        public Cartridge Cartridge;
        public IO IO;
        public Memory Memory;
        public CPU CPU;
        public PPU PPU;
        public APU APU;
        public Timer Timer;

        // Emulation load
        public Emulation(string RomPath)
        {
            if (File.Exists(RomPath))
            {
                // Load bytes
                RomData = File.ReadAllBytes(RomPath);

                // Instances
                IO = new IO(this);
                Cartridge = new Cartridge(this);
                Memory = new Memory(this);
                CPU = new CPU(this);
                PPU = new PPU(this);
                APU = new APU(this);
                Timer = new Timer(this);

                // Relations
                IO.APU = APU;
                IO.PPU = PPU;
                IO.Timer = Timer;
                IO.Cartridge = Cartridge;

                // Headers show
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

            else
            Console.WriteLine("ROM not found");
        }

        // Emulation loop
        public void Loop()
        {
            if (RomData.Length != 0)
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
        }

        // Pause emulation
        public static void Pause()
        {
        }

        // Rewind emulation
        public static void Rewind()
        {
        }

        // Save emulation
        public static void Save()
        {
        }
    }
}