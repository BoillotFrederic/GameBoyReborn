// ------
// Memory
// ------
public class Memory
{
    private byte MBC;
    private byte[] RomData;
    private byte[] RAM = new byte[0xFFFF];

    public Memory(Cartridge Cartridge, byte[] _RomData)
    {
        MBC = Cartridge.Type;
        RomData = _RomData;

        // Set RAM
        Array.Copy(RomData, 0, RAM, 0, 0x3FFF);

        if(RomData.Length > 0x3FFF && RomData.Length < 8000)
        Array.Copy(RomData, 0x4000, RAM, 0, RomData.Length - 0x3FFF);
        else
        Array.Copy(RomData, 0x4000, RAM, 0, 0x3FFF);
    }

    // Read memory
    public byte Read(ushort at)
    {
        // Rom bank 00 and bank NN
        if (at >= 0 && at <= 0x7FFF)
        return RAM[at];

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        return RAM[at];

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        return RAM[at];

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        return RAM[at];

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        return RAM[at];

        // Mirror of C000~DDFF
        else if (at >= 0xE000 && at <= 0xFDFF)
        return RAM[at];

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        return RAM[at];

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        return RAM[at];

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        return RAM[at];

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        return RAM[at];

        // Interrupt Enable register
        else
        return RAM[at];
    }

    // Write memory
    public void Write(ushort at, byte b)
    {
        // Rom bank 00 and bank NN
        if (at >= 0 && at <= 0x7FFF)
        RAM[at] = b;

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        RAM[at] = b;

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        RAM[at] = b;

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        RAM[at] = b;

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        RAM[at] = b;

        // Mirror of C000~DDFF
        else if (at >= 0xE000 && at <= 0xFDFF)
        RAM[at] = b;

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        RAM[at] = b;

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        RAM[at] = b;

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        RAM[at] = b;

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        RAM[at] = b;

        // Interrupt Enable register
        else
        RAM[at] = b;
    }
}
