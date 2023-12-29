// ---------
// Emulation
// ---------
using Raylib_cs;

public static class Emulation
{
    private static byte[]? romData;
    private static Cartridge? Cartridge;
    private static IO? IO;
    private static Memory? Memory;
    private static CPU? CPU;
    private static PPU? PPU;

    // All cores init
    public static void Init()
    {
        if (romData != null && romData.Length != 0)
        {
            byte[] header = new byte[0x1C];

            if (romData.Length > 0x0134)
            Array.Copy(romData, 0x0134, header, 0, 0x1C);

            IO = new IO();
            Cartridge = new Cartridge(header);
            Memory = new Memory(Cartridge, IO, romData);
            CPU = new CPU(Memory);
            PPU = new PPU(IO, Memory, CPU);

            // Update ref
            Memory.CPU = CPU;
            CPU.IO = IO;
/*
            Debug.Text(Cartridge.Title, Color.RED, 10000);
            Debug.Text(Cartridge.ManufacturerCode, Color.RED, 10000);
            Debug.Text(Cartridge.CGBDescription, Color.RED, 10000);
            Debug.Text(Cartridge.Licensee, Color.RED, 10000);
            Debug.Text(Cartridge.SGBDescription, Color.RED, 10000);
            Debug.Text(Cartridge.TypeDescription, Color.RED, 10000);
            Debug.Text(Cartridge.SizeDescription, Color.RED, 10000);
*/
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
        if(CPU != null && PPU != null)
        {
            PPU.CompletedFrame = false;

            while (!PPU.CompletedFrame)
            {
                CPU.Execution();
                PPU.Execution();
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
    }
}
