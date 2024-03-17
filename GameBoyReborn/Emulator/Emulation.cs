// ---------
// Emulation
// ---------

#pragma warning disable CS8618

using Raylib_cs;
using GameBoyReborn;

namespace Emulator
{
    public class Emulation
    {
        // Construct
        public string FileName;
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
                // Set file name
                FileName = Path.GetFileName(RomPath);
                FileName = FileName.Replace(".gb", "", StringComparison.CurrentCultureIgnoreCase);
                FileName = FileName.Replace(".gbc", "", StringComparison.CurrentCultureIgnoreCase);
                FileName = FileName.Replace(".sgb", "", StringComparison.CurrentCultureIgnoreCase);

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

                // Init
                IO.Init();

                // Headers show
                Console.WriteLine("Load rom");
                Console.WriteLine("--------");
                Console.WriteLine();
                Console.WriteLine("Title : " + Cartridge.Title);
                Console.WriteLine("Manufacturer Code : " + Cartridge.ManufacturerCode);
                Console.WriteLine("Licensee : " + Cartridge.Licensee);
                Console.WriteLine("Destination Code : " + Cartridge.DestinationCode);
                Console.WriteLine("CGB Description : " + Cartridge.CGBDescription + " (0x"+ Cartridge.CGB_Flag.ToString("X2") + ")");
                Console.WriteLine("SGB Description : " + Cartridge.SGBDescription + " (0x" + Cartridge.SGB_Flag.ToString("X2") + ")");
                Console.WriteLine("Type Description : " + Cartridge.TypeDescription + (Cartridge.MBC1M ? " (M)" : "") + " (0x" + Cartridge.Type.ToString("X2") + ")");
                Console.WriteLine("RomSize Description : " + Cartridge.RomSizeDescription + " (0x" + Cartridge.RomSize.ToString("X2") + ")");
                Console.WriteLine("RamSize Description : " + Cartridge.RamSizeDescription + " (0x" + Cartridge.RamSize.ToString("X2") + ")");
                Console.WriteLine("Header Checksum : " + (Cartridge.HeaderChecksumTest ? "OK" : "KO"));
                //Console.WriteLine("Global Checksum : " + (Cartridge.GlobalChecksumTest ? "OK" : "KO"));
                Console.WriteLine();
            }

            else
            Console.WriteLine("ROM not found");
        }

        // Emulation loop
        public void Loop()
        {
            if (RomData != null)
            {
                PPU.CompletedFrame = false;

                while (!PPU.CompletedFrame)
                {
                    CPU.Execution();
                    PPU.Execution();
                    Timer.Execution();
                }
            }
        }

        // Save external ram
        public void SaveExternalRam()
        {
            Cartridge.SaveExternalRam();
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