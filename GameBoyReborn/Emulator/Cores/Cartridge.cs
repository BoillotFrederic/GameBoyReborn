// ---------
// Cartridge
// ---------

#pragma warning disable CS8618

using GameBoyReborn;
using System.Text;
using System.Text.RegularExpressions;

namespace Emulator
{
    public class Cartridge
    {
        #region Headers

        // Power up sequence
        public PUS PUS;

        // Attribute of Cartridge
        private readonly string FileName;
        private readonly byte[] RomData;
        public string Title;
        public string ManufacturerCode;
        public byte CGB_Flag;
        public string CGBDescription;
        public string Licensee;
        public byte SGB_Flag;
        public string SGBDescription;
        public byte Type;
        public string TypeDescription;
        public string RomSizeDescription;
        public string RamSizeDescription;
        public byte RomSize;
        public byte RamSize;
        public string DestinationCode;
        public byte OldLicenseeCode;
        private byte MaskRomVersionNumber;
        private static byte HeaderChecksum;
        private ushort GlobalChecksum;
        public bool HeaderChecksumTest;
        public bool GlobalChecksumTest;
        public bool MBC1M = false;

        // Rom/ram set
        private int RomLength;
        private ushort RomBankCount;
        private int RomBankMask;
        private int RamLength;
        public ushort RamBankCount;
        private int RamBankMask;

        public Cartridge(Emulation Emulation)
        {
            FileName = Emulation.FileName;
            RomData = Emulation.RomData;

            byte[] NintendoLogo = new byte[0x2F];
            byte[] TitleRange = new byte[11];
            byte[] ManufacturerCodeRange = new byte[4];
            byte[] LicenseeRange = new byte[2];

            Array.Copy(RomData, 0x104, NintendoLogo, 0, 0x2F);
            Array.Copy(RomData, 0x134, TitleRange, 0, 11);
            Array.Copy(RomData, 0x13F, ManufacturerCodeRange, 0, 4);
            Array.Copy(RomData, 0x144, LicenseeRange, 0, 2);

            ManufacturerCode = Encoding.ASCII.GetString(ManufacturerCodeRange).TrimEnd('\0');
            Title = Encoding.ASCII.GetString(TitleRange).TrimEnd('\0');
            CGB_Flag = RomData[0x143];
            CGBDescription = GetCGBDescription(CGB_Flag);
            Licensee = GetLicense(BitConverter.ToUInt16(LicenseeRange));
            SGB_Flag = RomData[0x146];
            SGBDescription = GetSGBDescription(SGB_Flag);
            Type = RomData[0x147];
            TypeDescription = GetTypeDescription(Type);
            RomSize = RomData[0x148];
            RomSizeDescription = GetSizeDescription(RomSize);
            RamSize = RomData[0x149];
            RamSizeDescription = GetRamSizeDescription(RamSize);
            DestinationCode = RomData[0x14A] == 0x00 ? "Japanese" : "Non-Japanese";
            OldLicenseeCode = RomData[0x14B];
            MaskRomVersionNumber = RomData[0x14C];
            HeaderChecksum = RomData[0x14D];
            GlobalChecksum = Binary.U16(RomData[0x14F], RomData[0x14E]);

            // Set rom/ram
            RomLength = GetRomLength(RomSize);
            RomBankCount = GetRomBankCount(RomSize);
            RomBankMask = (1 << (int)Math.Ceiling(Math.Log2(RomBankCount))) - 1;
            RamLength = GetRamLength(RamSize);
            RamBankCount = GetRamBankCount(RamSize);
            RamBankMask = (1 << (int)Math.Ceiling(Math.Log2(RamBankCount))) - 1;

            // Checksum
            byte cs = 0;
            for (ushort i = 0x0134; i <= 0x014C; i++)
            cs = (byte)(cs - RomData[i] - 1);

            HeaderChecksumTest = cs == HeaderChecksum;
            //GlobalChecksumTest = RomData.Sum(x => (int)x) == GlobalChecksum;

            // Set power up sequence (DMG0=0, DMG=1, MGB=2, SGB=3, SGB2=4, CGB=5, AGB=6)
            PUS = new(1, HeaderChecksum != 0);

            // Check if MBC1M
            if((Type == 0x01 || Type == 0x02 || Type == 0x03) && RomBankCount > 0x10)
            {
                byte[] Data0x10 = new byte[0x2F];
                Array.Copy(RomData, 0x10 * 0x4000 + 0x104, Data0x10, 0, 0x2F);

                if (NintendoLogo.SequenceEqual(Data0x10))
                {
                    MBC1M = true;
                    MBC1_M_LowRegisterMask = 0xF;
                    MBC1_M_Shift = 4;
                }
            }

            // External ram
            LoadExternalRam();

            // Delegate read/write
            MBCs();
        }

        private static string GetLicense(ushort hex)
        {
            return hex switch
            {
                0x00 => "none",
                0x01 => "Nintendo R&D1",
                0x08 => "Capcom",
                0x13 => "Electronic Arts",
                0x18 => "Hudson Soft",
                0x19 => "b-ai",
                0x20 => "kss",
                0x22 => "pow",
                0x24 => "PCM Complete",
                0x25 => "san-x",
                0x28 => "Kemco Japan",
                0x29 => "seta",
                0x30 => "Viacom",
                0x31 => "Nintendo",
                0x32 => "Bandai",
                0x33 => "Ocean/Acclaim",
                0x34 => "Konami",
                0x35 => "Hector",
                0x37 => "Taito",
                0x38 => "Hudson",
                0x39 => "Banpresto",
                0x41 => "Ubi Soft",
                0x42 => "Atlus",
                0x44 => "Malibu",
                0x46 => "angel",
                0x47 => "Bullet-Proof",
                0x49 => "irem",
                0x50 => "Absolute",
                0x51 => "Acclaim",
                0x52 => "Activision",
                0x53 => "American sammy",
                0x54 => "Konami",
                0x55 => "Hi tech entertainment",
                0x56 => "LJN",
                0x57 => "Matchbox",
                0x58 => "Mattel",
                0x59 => "Milton Bradley",
                0x60 => "Titus",
                0x61 => "Virgin",
                0x64 => "LucasArts",
                0x67 => "Ocean",
                0x69 => "Electronic Arts",
                0x70 => "Infogrames",
                0x71 => "Interplay",
                0x72 => "Broderbund",
                0x73 => "sculptured",
                0x75 => "sci",
                0x78 => "THQ",
                0x79 => "Accolade",
                0x80 => "misawa",
                0x83 => "lozc",
                0x86 => "tokuma shoten i*",
                0x87 => "tsukuda ori*",
                0x91 => "Chunsoft",
                0x92 => "Video system",
                0x93 => "Ocean/Acclaim",
                0x95 => "Varie",
                0x96 => "Yonezawa/s'pal",
                0x97 => "Kaneko",
                0x99 => "Pack in soft",
                0xA4 => "Konami (Yu-Gi-Oh!)",
                _ => "none"
            };
        }

        private static string GetTypeDescription(byte hex)
        {
            return hex switch
            {
                0x00 => "ROM ONLY",
                0x01 => "MBC1",
                0x02 => "MBC1+RAM",
                0x03 => "MBC1+RAM+BATTERY",
                0x05 => "MBC2",
                0x06 => "MBC2+BATTERY",
                0x08 => "ROM+RAM",
                0x09 => "ROM+RAM+BATTERY",
                0x0B => "MMM01",
                0x0C => "MMM01+RAM",
                0x0D => "MMM01+RAM+BATTERY",
                0x0F => "MBC3+TIMER+BATTERY",
                0x10 => "MBC3+TIMER+RAM+BATTERY",
                0x19 => "MBC5",
                0x1A => "MBC5+RAM",
                0x1B => "MBC5+RAM+BATTERY",
                0x1C => "MBC5+RUMBLE",
                0x1D => "MBC5+RUMBLE+RAM",
                0x1E => "MBC5+RUMBLE+RAM+BATTERY",
                0x20 => "MBC6",
                0x22 => "MBC7+SENSOR+RUMBLE+RAM+BATTERY",
                0xFC => "POCKET CAMERA",
                0xFD => "BANDAI TAMA5",
                0xFE => "HuC3",
                0xFF => "HuC1+RAM+BATTERY",
                _ => "ROM ONLY",
            };
        }


        private static string GetCGBDescription(byte hex)
        {
            return hex switch
            {
                0x80 => "Game supports CGB functions, but works on old gameboys also.",
                0xC0 => "Game works on CGB only.",
                0x00 => "No CGB and no CGB functions supported",
                _ => "No CGB and no CGB functions supported",
            };
        }

        private static string GetSGBDescription(byte hex)
        {
            return hex switch
            {
                0x00 => "No SGB functions (Normal Gameboy or CGB only game)",
                0x03 => "Game supports SGB functions",
                _ => "Only DMG",
            };
        }

        private static string GetRamSizeDescription(byte hex)
        {
            return hex switch
            {
                0x00 => "None",
                0x01 => "2 KBytes",
                0x02 => "8 KBytes",
                0x03 => "32 KBytes (4 banks of 8KBytes each)",
                0x04 => "128 KBytes (16 banks of 8KBytes each)",
                0x05 => "64 KBytes (8 banks of 8KBytes each)",
                _ => "None",
            };
        }

        private static string GetSizeDescription(byte hex)
        {
            return hex switch
            {
                0x00 => "32KByte (no ROM banking)",
                0x01 => "64KByte (4 banks)",
                0x02 => "128KByte (8 banks)",
                0x03 => "256KByte (16 banks)",
                0x04 => "512KByte (32 banks)",
                0x05 => "1MByte (64 banks) - only 63 banks used by MBC1",
                0x06 => "2MByte (128 banks) - only 125 banks used by MBC1",
                0x07 => "4MByte (256 banks)",
                0x08 => "8MByte (512 banks)",
                0x52 => "1.1MByte (72 banks)",
                0x53 => "1.2MByte (80 banks)",
                0x54 => "1.5MByte (96 banks)",
                _ => "32KByte (no ROM banking)",
            };
        }

        private static int GetRamLength(byte hex)
        {
            return hex switch
            {
                0x00 => 0,
                0x01 => 2 * 1024,
                0x02 => 8 * 1024,
                0x03 => 32 * 1024,
                0x04 => 128 * 1024,
                0x05 => 64 * 1024,
                _ => 0,
            };
        }

        private static int GetRomLength(byte hex)
        {
            return hex switch
            {
                0x00 => 32 * 1024,
                0x01 => 64 * 1024,
                0x02 => 128 * 1024,
                0x03 => 256 * 1024,
                0x04 => 512 * 1024,
                0x05 => 1 * 1024 * 1024,
                0x06 => 2 * 1024 * 1024,
                0x07 => 4 * 1024 * 1024,
                0x08 => 8 * 1024 * 1024,
                0x52 => 1_100_000,
                0x53 => 1_200_000,
                0x54 => 1_500_000,
                _ => 32 * 1024,
            };
        }

        private static ushort GetRomBankCount(byte hex)
        {
            return hex switch
            {
                0x00 => 2,
                0x01 => 4,
                0x02 => 8,
                0x03 => 16,
                0x04 => 32,
                0x05 => 64,
                0x06 => 128,
                0x07 => 256,
                0x08 => 512,
                0x52 => 72,
                0x53 => 80,
                0x54 => 96,
                _ => 2,
            };
        }

        private byte GetRamBankCount(byte hex)
        {
            if(!Regex.Match(TypeDescription, "RAM", RegexOptions.IgnoreCase).Success)
            return 0;

            return hex switch
            {
                0x00 => 0,
                0x01 => 1,
                0x02 => 1,
                0x03 => 4,
                0x04 => 16,
                0x05 => 8,
                _ => 2,
            };
        }

        #endregion

        #region Memory bank controller

        #region External ram handle

        // External ram
        public byte[][] ExternalRam;

            /// <summary>
            /// Load external ram if it exist (directory = ExternalRam)
            /// </summary>
            private void LoadExternalRam()
            {
                string pathExternalRam = AppDomain.CurrentDomain.BaseDirectory + "ExternalRam/" + FileName + ".er";
                bool checkExternalRam = File.Exists(pathExternalRam);
                byte[]? bytesExternalRam = checkExternalRam ? File.ReadAllBytes(pathExternalRam) : null;

                ExternalRam = new byte[RamBankCount][];

                for (byte i = 0; i < RamBankCount; i++)
                {
                    ExternalRam[i] = new byte[0x2000];

                    if (checkExternalRam && bytesExternalRam != null)
                    Array.Copy(bytesExternalRam, 0x2000 * i, ExternalRam[i], 0, 0x2000);
                }
            }

            /// <summary>
            /// Save external ram (directory = ExternalRam)
            /// </summary>
            public void SaveExternalRam()
            {
                if (Regex.Match(TypeDescription, "BATTERY", RegexOptions.IgnoreCase).Success)
                {
                    string pathExternalRam = AppDomain.CurrentDomain.BaseDirectory + "ExternalRam/" + FileName + ".er";
                    byte[] externalRamBytes = new byte[0x2000 * RamBankCount];

                    for (byte i = 0; i < RamBankCount; i++)
                    Array.Copy(ExternalRam[i], 0, externalRamBytes, 0x2000 * i, 0x2000);

                    File.WriteAllBytes(pathExternalRam, externalRamBytes);
                }
            }

            #endregion

            #region MBCs handle

            // Delegate
            public delegate byte ReadDelegate(ushort at);
            public ReadDelegate Read;
            public delegate void WriteDelegate(ushort at, byte b);
            public WriteDelegate Write;

            /// <summary>
            /// MBC type selection
            /// </summary>
            private void MBCs()
            {
                // Set MBC
                switch (Type)
                {
                    // No MBC
                    // ------
                    case 0x00: case 0x08: case 0x09:
                    {
                        Read = NO_MBC_read;
                        Write = NO_MBC_write;
                        break;
                    }

                    // MBC1
                    // ----
                    case 0x01: case 0x02: case 0x03:
                    {
                        Read = MBC1_read;
                        Write = MBC1_write;
                        break;
                    }

                    // MBC2
                    // ----
                    case 0x05: case 0x06:
                    {

                        break;
                    }

                    // MBC3
                    // ----
                    case 0x0F: case 0x10: case 0x11: case 0x12: case 0x13:
                    {

                        break;
                    }

                    // MBC5
                    // ----
                    case 0x1A: case 0x1B: case 0x1C: case 0x1D: case 0x1E:
                    {

                        break;
                    }

                    // MBC6
                    // ----
                    case 0x20:
                    {

                        break;
                    }

                    // MBC7
                    // ----
                    case 0x22:
                    {

                        break;
                    }

                    // MMM01
                    // -----
                    case 0x0B: case 0x0C: case 0x0D:
                    {

                        break;
                    }

                    // HuC1
                    // ----
                    case 0xFF:
                    {

                        break;
                    }

                    // HuC3
                    // ----
                    case 0xFE:
                    {

                        break;
                    }

                    // Other
                    // -----
                    default:
                    {
                        Read = NO_MBC_read;
                        Write = NO_MBC_write;
                        break;
                    }
                }
            }

            #endregion

            #region MBCs operating variables

            // Rom/ram set
            private ushort selectedRomBank00 = 0;
            private ushort selectedRomBank = 1;
            public byte selectedRamBank = 0;

            #endregion

            #region MBCs read and write

                #region No MBC

                // Read
                private byte NO_MBC_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    return RomData[at];

                    // External RAM
                    else
                    return ExternalRam[0][at - 0xA000];
                }

                // Write
                private void NO_MBC_write(ushort at, byte b)
                {
                }

                #endregion

                #region MBC1

                // MBCs Registers
                private byte MBC1_LowRegister = 0;
                private byte MBC1_HighRegister = 0;
                private bool MBC1_RamEnable = false;
                private bool MBC1_BankingModeSelect = false;

                // MBC1 or MBC1M
                private readonly byte MBC1_M_Shift = 5;
                private readonly byte MBC1_M_LowRegisterMask = 0x1F;

                // Read
                private byte MBC1_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    {
                        int atInBank = 0x4000 * selectedRomBank00 + at;
                        return RomData[atInBank];
                    }

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // 00
                        if (MBC1_LowRegister == 0)
                        MBC1_LowRegister |= 1;

                        // Set
                        selectedRomBank = (byte)((MBC1_HighRegister << MBC1_M_Shift) | (MBC1_LowRegister & MBC1_M_LowRegisterMask));

                        // Max banks
                        selectedRomBank = (byte)(selectedRomBank & RomBankMask);

                        int atInBank = 0x4000 * selectedRomBank + at - 0x4000;

                        if (RomData.Length > atInBank)
                        return RomData[atInBank];

                        else
                        return 0;
                    }

                    // External RAM
                    else
                    {
                        if (MBC1_RamEnable)
                        return ExternalRam[selectedRamBank][at - 0xA000];
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MBC1_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    MBC1_RamEnable = (b & 0x0F) == 0x0A;

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    MBC1_LowRegister = (byte)(b & 0x1F);

                    // ROM Bank Number - High bits
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        if (!MBC1_BankingModeSelect)
                        {
                            MBC1_HighRegister = (byte)(b & 3);

                            // RomBank00 and RamBank = 0
                            selectedRomBank00 = 0;
                            selectedRamBank = 0;
                        }

                        else
                        {
                            // Set RomBank00
                            selectedRomBank00 = (byte)((b & 3) << MBC1_M_Shift);

                            // Max banks
                            selectedRomBank00 = (byte)(selectedRomBank00 & RomBankMask);

                            MBC1_HighRegister = (byte)(selectedRomBank00 >> MBC1_M_Shift);

                            // Set RamBank
                            selectedRamBank = (byte)(b & 3);
                            selectedRamBank = (byte)(selectedRamBank & RamBankMask);
                        }
                    }

                    // Banking Mode Select
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    MBC1_BankingModeSelect = Binary.ReadBit(b, 0);

                    // Exteral ram
                    else if (at >= 0xA000 && at <= 0xBFFF && MBC1_RamEnable)
                    ExternalRam[selectedRamBank][at - 0xA000] = b;
                }

                #endregion 

            #endregion

        #endregion
    }

    #region Power up sequence

    public class PUS
    {
        // Registers
        public byte A, B, C, D, E, H, L;

        // Flags
        public bool FlagZ, FlagN, FlagH, FlagC;

        // Hardware registers
        public byte P1, SB, SC, DIV, TIMA, TMA, TAC, IF, NR10, NR11, NR12, NR13, NR14, NR21, NR22, NR23, NR24,
                    NR30, NR31, NR32, NR33, NR34, NR41, NR42, NR43, NR44, NR50, NR51, NR52, LCDC, STAT, SCY,
                    SCX, LY, LYC, DMA, BGP, OPB0, OBP1, WY, WX, KEY1, VBK, HDMA1, HDMA2, HDMA3, HDMA4, HDMA5,
                    RP, BCPS, BCPD, OCPS, OCPD, SVBK, IE;

        public PUS(byte PUS_select, bool HCn0)
        {
            // Registers          DMG0   DMG    MGB    SGB    SGB2   CGB    AGB
            A     = new byte[7] { 0x01,  0x01,  0xFF,  0x01,  0xFF,  0x11,  0x11 }[PUS_select];
            B     = new byte[7] { 0xFF,  0x00,  0x00,  0x00,  0x00,  0x00,  0x01 }[PUS_select];
            C     = new byte[7] { 0x13,  0x13,  0x13,  0x14,  0x14,  0x00,  0x00 }[PUS_select];
            D     = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            E     = new byte[7] { 0xC1,  0xD8,  0xD8,  0x00,  0x00,  0x56,  0x56 }[PUS_select];
            H     = new byte[7] { 0x84,  0x01,  0x01,  0xC0,  0xC0,  0x00,  0x00 }[PUS_select];
            L     = new byte[7] { 0x03,  0x4D,  0x4D,  0x60,  0x60,  0x0D,  0x0D }[PUS_select];

            // Flags
            FlagZ = new bool[7] { false, true,  true,  false, false, true,  false }[PUS_select];
            FlagN = new bool[7] { false, false, false, false, false, false, false }[PUS_select];
            FlagH = new bool[7] { false, HCn0,  HCn0,  false, false, false, false }[PUS_select];
            FlagC = new bool[7] { false, HCn0,  HCn0,  false, false, false, false }[PUS_select];

            // Hardware registers
            P1    = new byte[7] { 0xCF,  0xCF,  0xCF,  0xC7,  0xCF,  0xC7,  0xCF }[PUS_select];
            SB    = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            SC    = new byte[7] { 0x7E,  0x7E,  0x7E,  0x7E,  0x7E,  0x7F,  0x7F }[PUS_select];
            DIV   = new byte[7] { 0x18,  0xAB,  0xAB,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            TIMA  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            TMA   = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            TAC   = new byte[7] { 0xF8,  0xF8,  0xF8,  0xF8,  0xF8,  0xF8,  0xF8 }[PUS_select];
            IF    = new byte[7] { 0xE1,  0xE1,  0xE1,  0xE1,  0xE1,  0xE1,  0xE1 }[PUS_select];
            NR10  = new byte[7] { 0x80,  0x80,  0x80,  0x80,  0x80,  0x80,  0x80 }[PUS_select];
            NR11  = new byte[7] { 0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF }[PUS_select];
            NR12  = new byte[7] { 0xF3,  0xF3,  0xF3,  0xF3,  0xF3,  0xF3,  0xF3 }[PUS_select];
            NR13  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            NR14  = new byte[7] { 0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF }[PUS_select];
            NR21  = new byte[7] { 0x3F,  0x3F,  0x3F,  0x3F,  0x3F,  0x3F,  0x3F }[PUS_select];
            NR22  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            NR23  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            NR24  = new byte[7] { 0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF }[PUS_select];
            NR30  = new byte[7] { 0x7F,  0x7F,  0x7F,  0x7F,  0x7F,  0x7F,  0x7F }[PUS_select];
            NR31  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            NR32  = new byte[7] { 0x9F,  0x9F,  0x9F,  0x9F,  0x9F,  0x9F,  0x9F }[PUS_select];
            NR33  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            NR34  = new byte[7] { 0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF }[PUS_select];
            NR41  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            NR42  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            NR43  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            NR44  = new byte[7] { 0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF,  0xBF }[PUS_select];
            NR50  = new byte[7] { 0x77,  0x77,  0x77,  0x77,  0x77,  0x77,  0x77 }[PUS_select];
            NR51  = new byte[7] { 0xF3,  0xF3,  0xF3,  0xF3,  0xF3,  0xF3,  0xF3 }[PUS_select];
            NR52  = new byte[7] { 0xF1,  0xF1,  0xF1,  0xF0,  0xF0,  0xF1,  0xF1 }[PUS_select];
            LCDC  = new byte[7] { 0x91,  0x91,  0x91,  0x91,  0x91,  0x91,  0x91 }[PUS_select];
            STAT  = new byte[7] { 0x81,  0x85,  0x85,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            SCY   = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            SCX   = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            LY    = new byte[7] { 0x91,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            LYC   = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            DMA   = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0x00,  0x00 }[PUS_select];
            BGP   = new byte[7] { 0xFC,  0xFC,  0xFC,  0xFC,  0xFC,  0xFC,  0xFC }[PUS_select];
            OPB0  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            OBP1  = new byte[7] { 0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF,  0xFF }[PUS_select];
            WY    = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            WX    = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            KEY1  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x7E,  0x7E }[PUS_select];
            VBK   = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFE,  0xFE }[PUS_select];
            HDMA1 = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            HDMA2 = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            HDMA3 = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            HDMA4 = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            HDMA5 = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0xFF,  0xFF }[PUS_select];
            RP    = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x3E,  0x3E }[PUS_select];
            BCPS  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            BCPD  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            OCPS  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            OCPD  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            SVBK  = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
            IE    = new byte[7] { 0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00 }[PUS_select];
        }
    }

    #endregion
}