// ------
// Memory
// ------
using Raylib_cs;

public class Memory
{
    private byte[] RomData;
    public byte[] RomBank_00 = new byte[0x4000];
    public byte[][] RomBank_nn;
    public byte[][] VideoRam_nn;
    public byte[] ExternalRam = new byte[0x2000];
    public byte[] WorkRam = new byte[0x1000];
    public byte[][] WorkRamCGB;
    public byte[] EchoRam = new byte[0x1E00];
    public byte[] OAM = new byte[0xA0];
    public byte[] NotUsable = new byte[0x60];
    public byte[] HighRAM = new byte[0x7F];

    public byte selectedRomBank = 0;
    public byte selectedVideoBank = 0;
    public byte selectedWorkBank = 0;

    private byte[] RomBoot = new byte[256];
    public bool booting = true;

    private IO IO;
    public CPU ?CPU;

    public Memory(Cartridge Cartridge, IO _IO, byte[] _RomData)
    {
        // I/O Ports
        IO = _IO;

        // Load rom
        RomData = _RomData;
        int RomBankSizeMin = RomData.Length >= 0x4000 ? 0x4000 : RomData.Length;

        // Load rom boot
        LoadRomBoot();

        // Set RomBank 00
        Array.Copy(RomData, 0, RomBank_00, 0, RomBankSizeMin);

        // Set RomBank 01~NN
        ushort nbBank = Cartridge.Size.Bank;
        int bankStart = 0x4000;
        RomBank_nn = new byte[nbBank][];

        if(RomData.Length > 0x4000)
        for (ushort i = 0; i < nbBank; i++, bankStart += 0x4000)
        {
            RomBank_nn[i] = new byte[0x4000];
            Array.Copy(RomData, bankStart * (i + 1), RomBank_nn[i], 0, RomBankSizeMin);
        }
        else
        {
            RomBank_nn[0] = new byte[0x4000];
            Array.Copy(RomData, 0, RomBank_nn[0], 0, RomBankSizeMin);
        }

        // Set video ram
        VideoRam_nn = new byte[2][];
        for (byte i = 0; i < 2; i++)
        VideoRam_nn[i] = new byte[0x2000];

        // Set work ram
        WorkRamCGB = new byte[7][];
        for(byte i = 0; i < 7; i++)
        WorkRamCGB[i] = new byte[0x1000];
    }

    // Read memory
    public byte Read(ushort at)
    {
        // Rom boot
        if(booting && at >= 0 && at < 0x100)
        return RomBoot[at];

        // Rom bank 00
        else if (at >= 0 && at <= 0x3FFF)
        return RomBank_00[at];

        // Rom bank NN
        else if (at >= 0x4000 && at <= 0x7FFF)
        return RomBank_nn[selectedRomBank][at - 0x4000];

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        return VideoRam_nn[selectedVideoBank][at - 0x8000];

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        return ExternalRam[at - 0xA000];

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        return WorkRam[at - 0xC000];

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        return WorkRamCGB[selectedWorkBank][at - 0xD000];

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
        else if (at >= 0xFF00 && at <= 0xFF7F || at == 0xFFFF)
        return IO.Read((ushort)(at - 0xFF00));

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        return HighRAM[at - 0xFF80];

        else
        return 0x00;
    }

    // Write memory
    public void Write(ushort at, byte b)
    {
        // Rom boot
        if (booting && at >= 0 && at < 0x100)
        RomBoot[at] = b;

        // Rom bank 00
        if (at >= 0 && at <= 0x3FFF)
        RomBank_00[at] = b;

        // Rom bank 01~NN
        else if (at >= 0x4000 && at <= 0x7FFF)
        RomBank_nn[selectedRomBank][at - 0x4000] = b;

        // Video RAM
        else if (at >= 0x8000 && at <= 0x9FFF)
        VideoRam_nn[selectedVideoBank][at - 0x8000] = b;

        // External RAM
        else if (at >= 0xA000 && at <= 0xBFFF)
        ExternalRam[at - 0xA000] = b;

        // Work RAM (WRAM)
        else if (at >= 0xC000 && at <= 0xCFFF)
        WorkRam[at - 0xC000] = b;

        // Work RAM (WRAM)
        else if (at >= 0xD000 && at <= 0xDFFF)
        WorkRamCGB[selectedWorkBank][at - 0xD000] = b;

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
        else if ((at >= 0xFF00 && at <= 0xFF7F || at == 0xFFFF) && at != 0xFF46)
        IO.Write((ushort)(at - 0xFF00), b);

        // DMA transfer
        else if (at == 0xFF46)
        DMATransfer(b);

        // High RAM (HRAM)
        else if (at >= 0xFF80 && at <= 0xFFFE)
        HighRAM[at - 0xFF80] = b;
    }

    // DMA Transfer
    private void DMATransfer(byte sourceAddress)
    {
        if (CPU != null)
        {
            ushort startAddress;

            // Rom source
            if (sourceAddress <= 0x7F)
            {
                startAddress = (ushort)(sourceAddress << 8);
                Array.Copy(RomData, startAddress, OAM, 0, 160);
            }
            // RAM source
            else if (sourceAddress >= 0xC0 && sourceAddress <= 0xFD)
            {
                startAddress = (ushort)((sourceAddress - 0xC0) << 8);
                Array.Copy(WorkRamCGB[selectedWorkBank], startAddress, OAM, 0, 160);
            }
        }
    }

    // Rom boot
    private void LoadRomBoot()
    {
        if (File.Exists("Boot\\DMG_ROM.bin"))
        RomBoot = File.ReadAllBytes("Boot\\DMG_ROM.bin");
    }
}
