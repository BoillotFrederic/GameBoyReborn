// ------
// Memory
// ------
using Raylib_cs;

namespace GameBoyReborn
{
    public class Memory
    {
        public byte[] RomData;
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

        private bool RamEnable = false;
        private bool BankingModeSelect = false;

        public byte selectedRomBank = 0;
        public byte selectedVideoBank = 0;
        public byte selectedWorkBank = 0;

        private byte[] RomBoot = new byte[256];
        public bool booting = false;

        private readonly IO IO;
        public CPU? CPU;
        public PPU? PPU;
        public Cartridge? Cartridge;

        // Init ram
        public Memory(Cartridge _Cartridge, IO _IO, byte[] _RomData)
        {
            // I/O Ports
            IO = _IO;

            // Cartridge
            Cartridge = _Cartridge;

            // Load rom
            RomData = _RomData;

            // Load rom boot
            LoadRomBoot();

            // Set RomBank 00
            Array.Copy(RomData.ToArray(), 0, RomBank_00, 0, 0x4000);

            // Set RomBank 01~NN
            ushort nbBank = Cartridge.Size.Bank;
            int bankStart = 0x4000;
            RomBank_nn = new byte[nbBank][];

            if (RomData.Length > 0x4000)
            for (ushort i = 0; i < nbBank - 1; i++, bankStart += 0x4000)
            {
                RomBank_nn[i] = new byte[0x4000];
                Array.Copy(RomData.ToArray(), bankStart, RomBank_nn[i], 0, 0x4000);
            }
            else
            {
                RomBank_nn[0] = new byte[0x4000];
                Array.Copy(RomData.ToArray(), 0, RomBank_nn[0], 0, RomData.Length);
            }

            // Set video ram
            VideoRam_nn = new byte[2][];
            for (byte i = 0; i < 2; i++)
            VideoRam_nn[i] = new byte[0x2000];

            // Set work ram
            WorkRamCGB = new byte[7][];
            for (byte i = 0; i < 7; i++)
            WorkRamCGB[i] = new byte[0x1000];
        }

        // Read memory
        public byte Read(ushort at)
        {
            // Rom boot
            if (booting && at >= 0 && at < 0x100)
            return RomBoot[at];

            // MBC read
            else if (at <= 0x7FFF || (at >= 0xA000 && at <= 0xBFFF))
            return MBC_read(at);

            // Video RAM
            else if (at >= 0x8000 && at <= 0x9FFF)
            return VideoRam_nn[selectedVideoBank][at - 0x8000];

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
            return IO.Read((byte)(at - 0xFF00));

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

            // MBC write
            else if (at <= 0x7FFF || (at >= 0xA000 && at <= 0xBFFF))
            MBC_write(at, b);

            // Video RAM
            else if (at >= 0x8000 && at <= 0x9FFF)
            VideoRam_nn[selectedVideoBank][at - 0x8000] = b;

            // Work RAM (WRAM)
            else if (at >= 0xC000 && at <= 0xCFFF && CPU != null)
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
            else if ((at >= 0xFF00 && at <= 0xFF7F || at == 0xFFFF))
            IO.Write((byte)(at - 0xFF00), b);

            // High RAM (HRAM)
            else if (at >= 0xFF80 && at <= 0xFFFE)
            HighRAM[at - 0xFF80] = b;
        }

        // MBCs
        private byte MBC_read(ushort at)
        {
            if(Cartridge != null)
            switch (Cartridge.Type)
            {
                // No MBC
                // ------
                case 0x00: case 0x08: case 0x09:

                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomBank_00[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    return RomBank_nn[selectedRomBank][at - 0x4000];

                    // External RAM
                    else
                    return ExternalRam[at - 0xA000];

                // MBC1
                // ------
                case 0x01: case 0x02: case 0x03:

                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomBank_00[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    return RomBank_nn[selectedRomBank][at - 0x4000];

                    // External RAM
                    else
                    return ExternalRam[at - 0xA000];

                // MBC2
                // ------
                case 0x05: case 0x06:
                return 0;

                // MBC3
                // ------
                case 0x0F: case 0x10: case 0x11: case 0x12: case 0x13:
                return 0;

                // MBC5
                // ------
                case 0x1A: case 0x1B: case 0x1C: case 0x1D: case 0x1E:
                return 0;

                // MBC6
                // ------
                case 0x20:
                return 0;

                // MBC7
                // ------
                case 0x22:
                return 0;

                // MMM01
                // ------
                case 0x0B: case 0x0C: case 0x0D:
                return 0;

                // HuC1
                // ------
                case 0xFF:
                return 0;

                // HuC3
                // ------
                case 0xFE:
                return 0;

                // Other
                // ------
                default:
                return 0;
            }

            else
            return 0;
        } 

        private void MBC_write(ushort at, byte b)
        {
            if (Cartridge != null)
            switch (Cartridge.Type)
            {
                // No MBC
                // ------
                case 0x00: case 0x08: case 0x09:

/*                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    RomBank_00[at] = b;

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    RomBank_nn[selectedRomBank][at - 0x4000] = b;

                    // External RAM
                    else
                    ExternalRam[at - 0xA000] = b;*/

                break;

                // MBC1
                // ----
                case 0x01: case 0x02: case 0x03:

                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    RamEnable = b == 0xA;

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    {
                        selectedRomBank = (byte)((selectedRomBank & 0xE0) | (b & 0x1F));

                        if ((selectedRomBank & 0xF) == 0)
                        selectedRomBank |= 1;
                    }

                    // ROM Bank Number - High bits
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        if (!BankingModeSelect)
                        selectedRomBank = (byte)((selectedRomBank & 0xCF) | ((b & 3) << 8));

                        else
                        selectedRomBank = (byte)(b & 3);
                    }

                    // Banking Mode Select
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    BankingModeSelect = Binary.ReadBit(b, 0);

                break;

                // MBC2
                // ----
                case 0x05: case 0x06:
                    //
                break;

                // MBC3
                // ----
                case 0x0F: case 0x10: case 0x11: case 0x12: case 0x13:
                    //
                break;

                // MBC5
                // ----
                case 0x1A: case 0x1B: case 0x1C: case 0x1D: case 0x1E:
                    //
                break;

                // MBC6
                // ----
                case 0x20:
                    //
                break;

                // MBC7
                // ----
                case 0x22:
                    //
                break;

                // MMM01
                // ------
                case 0x0B: case 0x0C: case 0x0D:
                    //
                break;

                // HuC1
                // ----
                case 0xFF:
                    //
                break;

                // HuC3
                // ----
                case 0xFE:
                    //
                break;

                // Other
                // -----
                default:
                    //
                break;
            }
        }

        // Rom boot
        private void LoadRomBoot()
        {
            if (File.Exists("Boot/DMG_ROM.bin"))
            RomBoot = File.ReadAllBytes("Boot/DMG_ROM.bin");

            else
            booting = false;
        }
    }
}
