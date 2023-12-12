// ------
// Memory
// ------
public class Memory
{
    private byte[] RomData;
    public byte[] RomBank_00 = new byte[0x4000];
    public byte[,] RomBank_nn;
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
        ushort nbBank = (ushort)(Cartridge.Size.Bank - 1);
        int bankStart = 0x4000;
        RomBank_nn = new byte[nbBank, 0x4000];

        for(ushort i = 0; i < nbBank; i++, bankStart += 0x4000)
        Array.Copy(RomData, bankStart, RomBank_nn, 0, 0x4000);
    }

    // Read memory
    public byte Read(ushort at)
    {
        // Rom bank 00
        if (at >= 0 && at <= 0x3FFF)
        return RomBank_00[at];

        // Rom bank NN
        else if (at >= 0x4000 && at <= 0x7FFF)
        return RomBank_nn[selectedRomBank, at];

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        return VideoRam_nn[selectedVideoBank, at];

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        return ExternalRam[at];

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        return WorkRam[at];

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        return WorkRamCGB[selectedWorkBank, at];

        // ECHO RAM
        else if (at >= 0xE000 && at <= 0xFDFF)
        return EchoRam[at];

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        return OAM[at];

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        return NotUsable[at];

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        return IO_Registers[at];

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        return HighRAM[at];

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
        RomBank_nn[selectedRomBank, at] = b;

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        VideoRam_nn[selectedVideoBank, at] = b;

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        ExternalRam[at] = b;

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        WorkRam[at] = b;

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        WorkRamCGB[selectedWorkBank, at] = b;

        // ECHO RAM
        else if (at >= 0xE000 && at <= 0xFDFF)
        EchoRam[at] = b;

        // OAM
        else if (at >= 0xFE00 && at <= 0xFE9F)
        OAM[at] = b;

        // Not Usable
        else if (at >= 0xFEA0 && at <= 0xFEFF)
        NotUsable[at] = b;

        // I/O Registers
        else if (at >= 0xFF00 && at <= 0xFF7F)
        IO_Registers[at] = b;

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        HighRAM[at] = b;

        // Interrupt Enable register
        else
        InterruptEnableregister = b;
    }
}
