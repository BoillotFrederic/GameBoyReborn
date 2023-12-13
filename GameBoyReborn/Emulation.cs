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

    // All cores init
    public static void Init()
    {
        if (romData != null && romData.Length != 0)
        {
            byte[] header = new byte[0x1C];
            Array.Copy(romData, 0x0134, header, 0, 0x1C);

            IO = new IO();
            Cartridge = new Cartridge(header);
            Memory = new Memory(Cartridge, IO, romData);
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
        if(CPU != null)
        {
            CPU.Cycles = 0;

            while (CPU.Cycles <= 70224)
            {
                CPU.Execution();

/*                if(CPU.Cycles  == 70224)
                Debug.Text("Test", Color.BLACK, 10000);*/
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
    }
}
