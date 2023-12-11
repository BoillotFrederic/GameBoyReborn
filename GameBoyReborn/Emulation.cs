// ---------
// Emulation
// ---------
using Raylib_cs;
using System.IO;

public static class Emulation
{
    private static byte[]? romData;
    private static Cartridge? Cartridge;
    private static Memory? Memory;
    private static CPU? CPU;

    // All cores init
    public static void Init()
    {
        if (romData != null && romData.Length != 0)
        {
            byte[] header = new byte[0x1C];
            Array.Copy(romData, 0x0134, header, 0, 0x1C);

            Cartridge = new Cartridge(header);
            Memory = new Memory(Cartridge, romData);
            CPU = new CPU(Memory);

            Debug.Text(Cartridge.Title, Color.BLACK, 10000);
            Debug.Text(Cartridge.ManufacturerCode, Color.BLACK, 10000);
            Debug.Text(Cartridge.CGBDescription, Color.BLACK, 10000);
            Debug.Text(Cartridge.Licensee, Color.BLACK, 10000);
            Debug.Text(Cartridge.SGBDescription, Color.BLACK, 10000);
            Debug.Text(Cartridge.TypeDescription, Color.BLACK, 10000);
            Debug.Text(Cartridge.SizeDescription, Color.BLACK, 10000);
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

    }

    // Stop emulation
    public static void Stop()
    {
        Cartridge = null;
        Memory = null;
        CPU = null;
    }
}
