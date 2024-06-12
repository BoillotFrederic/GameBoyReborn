// ---------
// Emulation
// ---------

//#pragma warning disable CS8618

using Raylib_cs;
using GameBoyReborn;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;

namespace Emulator
{
    public class Emulation
    {
        // Debug mode
        public bool DebugEnable = false;
        public bool OneByOne = false;

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

        // Emulation control
        public bool MenuIsOpen = false;
        public bool Paused = false;

        // Emulation contruct
        public Emulation(Game game)
        {
            // Set file name
            FileName = game.Name;

            // Load bytes
            RomData = LoadGameData(game) ?? Array.Empty<byte>();

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

        // Emulation loop
        public void Loop()
        {
            if (RomData != null && !Paused)
            {
                PPU.CompletedFrame = OneByOne;

                #region If debug enable
                if (DebugEnable)
                {
                    if (Input.XabyPadY)
                    OneByOne = true;

                    if (Input.XabyPadX)
                    OneByOne = false;

                    if (Input.TriggerPadRB)
                    PPU.CompletedFrame = false;
                }
                #endregion

                while (!PPU.CompletedFrame)
                {
                    CPU.Execution();
                    PPU.Execution();
                    Timer.Execution();

                    #region If debug enable
                    if (DebugEnable && OneByOne)
                    {
                        PPU.CompletedFrame = true;
                        Thread.Sleep(150);
                    }
                    #endregion
                }
            }
        }

        // Save external
        public void SaveExternal()
        {
            Cartridge.SaveExternalRam();
            Cartridge.SaveExternalTimer();
        }

        // Start emulation
        public static void Start(Game game)
        {
            Raylib.SetTargetFPS(60);
            Program.EmulatorRun = true;
            Program.Emulation = new(game);
            Program.Emulation.Paused = false;
            Audio.Init();
        }

        // Stop emulation
        public void Stop()
        {
            Raylib.SetTargetFPS(Program.FPSMax);
            Program.EmulatorRun = false;
            Array.Clear(RomData);
            DrawGB.ClearScreen();
            Audio.Stop();
            Program.Emulation = null;
        }

        // Pause emulation
        public void Pause()
        {
            Raylib.SetTargetFPS(Program.FPSMax);
            Paused = true;
        }

        // UnPause emulation
        public void UnPause()
        {
            Raylib.SetTargetFPS(60);
            Paused = false;
        }

        // Rewind emulation
        public void Rewind()
        {
        }

        // Save stat emulation
        public void Save()
        {
        }

        // Load stat emulation
        public void Load()
        {
        }

        // Load game data
        private static byte[]? LoadGameData(Game game)
        {
            // Zipped file
            if (game.ZippedFile != "")
            {
                string extension = Path.GetExtension(game.Path).ToLower();
                IArchive? archive = null;

                // Open archive
                switch (extension)
                {
                    case ".zip": archive = ZipArchive.Open(game.Path); break;
                    case ".7z": case ".7zip": archive = SevenZipArchive.Open(game.Path); break;
                    case ".rar": archive = RarArchive.Open(game.Path); break;
                }

                // Find entry
                if (archive != null)
                {
                    IArchiveEntry? entry = archive.Entries.FirstOrDefault(e => e.Key.EndsWith(game.ZippedFile, StringComparison.OrdinalIgnoreCase));
                    
                    if (entry == null)
                    return null;

                    using Stream? entryStream = entry.OpenEntryStream();
                    using var memoryStream = new MemoryStream();
                    entryStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }

                else
                return null;
            }

            // Simple file
            else
            return File.ReadAllBytes(game.Path);
        }
    }
}