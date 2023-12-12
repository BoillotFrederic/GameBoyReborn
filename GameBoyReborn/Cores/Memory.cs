// ------
// Memory
// ------
using Raylib_cs;

public class Memory
{
    private byte[] RomData;
    public byte[] RomBank_00 = new byte[0x4000];
    public byte[][] RomBank_nn;
    public byte[,] VideoRam_nn = new byte[2, 0x2000];
    public byte[] ExternalRam = new byte[0x2000];
    public byte[] WorkRam = new byte[0x1000];
    public byte[,] WorkRamCGB = new byte[7, 0x1000];
    public byte[] EchoRam = new byte[0x1E00];
    public byte[] OAM = new byte[0xA0];
    public byte[] NotUsable = new byte[0x60];
    public byte[] IO_Registers = new byte[0x80];
    public byte[] HighRAM = new byte[0x7F];
    public byte InterruptEnableregister = 0x00;

    public byte selectedRomBank = 0;
    public byte selectedVideoBank = 0;
    public byte selectedWorkBank = 0;

    public Memory(Cartridge Cartridge, byte[] _RomData)
    {
        RomData = _RomData;

        // Set RomBank 00
        Array.Copy(RomData, 0, RomBank_00, 0, 0x4000);

        // Set RomBank 01~NN
        ushort nbBank = (ushort)(Cartridge.Size.Bank);
        int bankStart = 0x4000;
        RomBank_nn = new byte[bankStart][];

        for(ushort i = 0; i < nbBank; i++, bankStart += 0x4000)
        {
            RomBank_nn[i] = new byte[0x4000];
            Array.Copy(RomData, bankStart * (i + 1), RomBank_nn[i], 0, 0x4000);
        }
    }

    // Read memory
    public byte Read(ushort at)
    {
        // Rom bank 00
        if (at >= 0 && at <= 0x3FFF)
        return RomBank_00[at];

        // Rom bank NN
        else if (at >= 0x4000 && at <= 0x7FFF)
        return RomBank_nn[selectedRomBank][at - 0x4000];

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        return VideoRam_nn[selectedVideoBank, at - 0x8000];

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        return ExternalRam[at - 0xA000];

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        return WorkRam[at - 0xC000];

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        return WorkRamCGB[selectedWorkBank, at - 0xD000];

        // ECHO RAM
        else if (at >= 0xE000 && at <= 0xFDFF)
        return EchoRam[at - 0xE000];

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        return OAM[at - 0xFE00];

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        return NotUsable[at - 0xFEA0];

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        return IO_Registers[at - 0xFF00];

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        return HighRAM[at - 0xFF80];

        // Interrupt Enable register
        else
        return InterruptEnableregister;
    }

    // Write memory
    public void Write(ushort at, byte b)
    {
        // Rom bank 00
        if (at >= 0 && at <= 0x3FFF)
        RomBank_00[at] = b;

        // Rom bank 01~NN
        if (at >= 0x4000 && at <= 0x7FFF)
        RomBank_nn[selectedRomBank][at - 0x4000] = b;

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        VideoRam_nn[selectedVideoBank, at - 0x8000] = b;

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        ExternalRam[at - 0xA000] = b;

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        WorkRam[at - 0xC000] = b;

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        WorkRamCGB[selectedWorkBank, at - 0xD000] = b;

        // ECHO RAM
        else if (at >= 0xE000 && at <= 0xFDFF)
        EchoRam[at - 0xE000] = b;

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        OAM[at - 0xFE00] = b;

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        NotUsable[at - 0xFEA0] = b;

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        IO_Registers[at - 0xFF00] = b;

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        HighRAM[at - 0xFF80] = b;

        // Interrupt Enable register
        else
        InterruptEnableregister = b;
    }
}
