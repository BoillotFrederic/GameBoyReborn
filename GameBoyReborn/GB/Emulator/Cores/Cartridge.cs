﻿// ---------
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
            byte pusSelect;

            if (SGB_Flag == 0x03)
            pusSelect = 4;
            else if (/*CGB_Flag == 0x80 || */CGB_Flag == 0xC0)
            pusSelect = 5;
            else
            pusSelect = 1;

            PUS = new(pusSelect, HeaderChecksum != 0);

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

            // Delegate read/write
            MBCs();

            // Load external
            LoadExternalRam();
            LoadExternalTimer();
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
                0x11 => "MBC3",
                0x12 => "MBC3+RAM",
                0x13 => "MBC3+RAM+BATTERY",
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
                _ => "No SGB functions (Normal Gameboy or CGB only game)",
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
            private int ExternalRamSize;

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
                    ExternalRam[i] = new byte[ExternalRamSize];

                    if (checkExternalRam && bytesExternalRam != null)
                    Array.Copy(bytesExternalRam, ExternalRamSize * i, ExternalRam[i], 0, ExternalRamSize);
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
                    byte[] externalRamBytes = new byte[ExternalRamSize * RamBankCount];

                    for (byte i = 0; i < RamBankCount; i++)
                    Array.Copy(ExternalRam[i], 0, externalRamBytes, ExternalRamSize * i, ExternalRamSize);

                    File.WriteAllBytes(pathExternalRam, externalRamBytes);
                }
            }

            #endregion

            #region External timer handle

            // External timer
            private long ExternalTimer;

            /// <summary>
            /// Load external timer if it exist (directory = ExternalTimer)
            /// </summary>
            private void LoadExternalTimer()
            {
                string pathExternalTimer = AppDomain.CurrentDomain.BaseDirectory + "ExternalTimer/" + FileName + ".et";
                bool checkExternalTimer = File.Exists(pathExternalTimer);
                long ExternalTimerGet = long.Parse(checkExternalTimer ? File.ReadAllText(pathExternalTimer) : "0");

                ExternalTimer = ExternalTimerGet;

                if (ExternalTimerGet == 0)
                ExternalTimer = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            }

            /// <summary>
            /// Save external timer (directory = ExternalTimer)
            /// </summary>
            public void SaveExternalTimer()
            {
                string pathExternalTimer = AppDomain.CurrentDomain.BaseDirectory + "ExternalTimer/" + FileName + ".et";

                if (Regex.Match(TypeDescription, "TIMER", RegexOptions.IgnoreCase).Success)
                File.WriteAllText(pathExternalTimer, ExternalTimer.ToString());
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
                        ExternalRamSize = 0x2000;
                        Read = NO_MBC_read;
                        Write = NO_MBC_write;
                        break;
                    }

                    // MBC1
                    // ----
                    case 0x01: case 0x02: case 0x03:
                    {
                        ExternalRamSize = 0x2000;
                        Read = MBC1_read;
                        Write = MBC1_write;
                        break;
                    }

                    // MBC2
                    // ----
                    case 0x05: case 0x06:
                    {
                        RamBankCount = 1;
                        ExternalRamSize = 0x200;
                        Read = MBC2_read;
                        Write = MBC2_write;
                        break;
                    }

                    // MBC3
                    // ----
                    case 0x0F: case 0x10: case 0x11: case 0x12: case 0x13:
                    {
                        ExternalRamSize = 0x2000;
                        Read = MBC3_read;
                        Write = MBC3_write;
                        break;
                    }

                    // MBC5
                    // ----
                    case 0x19: case 0x1A: case 0x1B: case 0x1C: case 0x1D: case 0x1E:
                    {
                        ExternalRamSize = 0x2000;
                        Read = MBC5_read;
                        Write = MBC5_write;
                        break;
                    }

                    // MBC6
                    // ----
                    case 0x20:
                    {
                        for(int i = 0; i < 64; i++)
                        {
                            MBC6_RomFlashA[i] = new byte[0x2000];
                            MBC6_RomFlashB[i] = new byte[0x2000];
                        }

                        ExternalRamSize = 0x2000;
                        Read = MBC6_read;
                        Write = MBC6_write;
                        break;
                    }

                    // MBC7
                    // ----
                    case 0x22:
                    {
                        ExternalRamSize = 0x2000;
                        Read = MBC7_read;
                        Write = MBC7_write;
                        break;
                    }

                    // MMM01
                    // -----
                    case 0x0B: case 0x0C: case 0x0D:
                    {
                        ExternalRamSize = 0x2000;
                        Read = MMM01_read;
                        Write = MMM01_write;
                        break;
                    }

                    // HuC1
                    // ----
                    case 0xFF:
                    {
                        ExternalRamSize = 0x2000;
                        Read = HuC1_read;
                        Write = HuC1_write;
                        break;
                    }

                    // HuC3
                    // ----
                    case 0xFE:
                    {
                        ExternalRamSize = 0x2000;
                        Read = HuC3_read;
                        Write = HuC3_write;
                        break;
                    }

                    // Other
                    // -----
                    default:
                    {
                        ExternalRamSize = 0x2000;
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

                #region MBC2

                // MBCs Registers
                private bool MBC2_RamEnable = false;

                // Read
                private byte MBC2_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // 00
                        if (selectedRomBank == 0)
                        selectedRomBank |= 1;

                        // Max banks
                        selectedRomBank = (byte)(selectedRomBank & RomBankMask);

                        int atInBank = 0x4000 * selectedRomBank + at - 0x4000;

                        if (RomData.Length > atInBank)
                        return RomData[atInBank];

                        else
                        return 0;
                    }

                    // Built-in RAM
                    else if (at >= 0xA000 && at <= 0xA1FF)
                    {
                        if (MBC2_RamEnable)
                        return (byte)(0xF0 | (ExternalRam[selectedRamBank][at - 0xA000] & 0xF));
                        else
                        return 0xFF;
                    }

                    // Echoes” of A000–A1FF
                    else
                    {
                        if (MBC2_RamEnable)
                        return (byte)(0xF0 | (ExternalRam[selectedRamBank][at & 0x1FF] & 0xF));
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MBC2_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x3FFF)
                    {
                        if(((at >> 8) & 1) == 1)
                        selectedRomBank = (byte)(b & 0xF);
                        else
                        MBC2_RamEnable = (b & 0xF) == 0xA;
                    }

                    // Built-in RAM
                    else if (at >= 0xA000 && at <= 0xA1FF && MBC2_RamEnable)
                    ExternalRam[selectedRamBank][at - 0xA000] = (byte)(0xF0 | (b & 0xF));

                    // Echoes” of A000–A1FF
                    else if (at >= 0xA200 && at <= 0xBFFF && MBC2_RamEnable)
                    ExternalRam[selectedRamBank][at & 0x1FF] = (byte)(0xF0 | (b & 0xF));
                }

                #endregion 

                #region MBC3

                // MBCs Registers
                private bool MBC3_RamEnable = false;
                private byte MBC3_LatchClockDataLastWriteValue = 0;

                // TIMER Registers
                private readonly byte[] MBC3_RTC = new byte[5];

                // Read
                private byte MBC3_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // Max banks
                        selectedRomBank = (byte)(selectedRomBank & RomBankMask);

                        // 00
                        if (selectedRomBank == 0)
                        selectedRomBank |= 1;

                        int atInBank = 0x4000 * selectedRomBank + at - 0x4000;

                        if (RomData.Length > atInBank)
                        return RomData[atInBank];

                        else
                        return 0;
                    }

                    // External RAM or RTC Register
                    else
                    {
                        if (MBC3_RamEnable)
                        {
                            if(selectedRamBank <= (selectedRamBank & RamBankMask))
                            return ExternalRam[selectedRamBank][at - 0xA000];

                            else if (selectedRamBank >= 0x08 && selectedRamBank <= 0x0C)
                            return MBC3_RTC[selectedRamBank - 0x08];

                            else
                            return 0xFF;
                        }
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MBC3_write(ushort at, byte b)
                {
                    // Ram and timer enable
                    if (at >= 0 && at <= 0x1FFF)
                    MBC3_RamEnable = (b & 0x0F) == 0x0A;

                    // ROM Bank Number
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    selectedRomBank = (byte)(b & 0x7F);

                    // RAM Bank Number or RTC Register Select
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    selectedRamBank = b;

                    // Latch Clock Data
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    {
                        // Latched
                        if(MBC3_LatchClockDataLastWriteValue == 0x00 && b == 0x01)
                        {
                            long CurrentTimestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                            long timeElapsed = CurrentTimestamp - ExternalTimer;

                            MBC3_RTC[0] = (byte)(timeElapsed % 60); // Seconds
                            MBC3_RTC[1] = (byte)(timeElapsed / 60 % 60); // Minutes
                            MBC3_RTC[2] = (byte)(timeElapsed / 3600 % 24); // Hours
                            MBC3_RTC[3] = (byte)(timeElapsed / 86400); // Lower days
                            MBC3_RTC[4] = (byte)(((timeElapsed / 86400) >> 8) & 1); // Upper days

                            Binary.SetBit(ref MBC3_RTC[4], 6, false);
                            Binary.SetBit(ref MBC3_RTC[4], 7, timeElapsed / 86400 > 511);

                            if ((timeElapsed / 86400) > 511)
                            ExternalTimer = CurrentTimestamp;
                        }

                        MBC3_LatchClockDataLastWriteValue = b;
                    }

                    // Exteral ram
                    else if (at >= 0xA000 && at <= 0xBFFF && MBC3_RamEnable)
                    {
                        if(selectedRamBank <= (selectedRamBank & RamBankMask))
                        ExternalRam[selectedRamBank][at - 0xA000] = b;
                    }
                }

                #endregion

                #region MBC5

                // MBCs Registers
                private byte MBC5_LowRegister = 1;
                private byte MBC5_HighRegister = 0;
                private bool MBC5_RamEnable = false;

                // Read
                private byte MBC5_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NNN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // Set
                        selectedRomBank = (byte)((MBC5_HighRegister << 8) | MBC5_LowRegister);

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
                        if (MBC5_RamEnable)
                        return ExternalRam[selectedRamBank][at - 0xA000];
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MBC5_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    MBC5_RamEnable = (b & 0x0F) == 0x0A;

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x2FFF)
                    MBC5_LowRegister = b;

                    // ROM Bank Number - High bits
                    else if (at >= 0x3000 && at <= 0x3FFF)
                    MBC5_HighRegister = (byte)(b & 1);

                    // RAM Bank Number
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        selectedRamBank = (byte)(b & 0x0F);

                        // Rumble (Not yet tested)
                        // Input.Vibration(Binary.ReadBit(selectedRamBank, 3));

                        // Max banks
                        selectedRamBank = (byte)(selectedRamBank & RamBankMask);
                    }

                    // Exteral ram
                    else if (at >= 0xA000 && at <= 0xBFFF && MBC5_RamEnable)
                    ExternalRam[selectedRamBank][at - 0xA000] = b;
                }

                #endregion

                #region MBC6

                // MBCs Registers
                private byte MBC6_RomFBankA = 0;
                private byte MBC6_RomFBankB = 0;
                private byte MBC6_RamFBankA = 0;
                private byte MBC6_RamFBankB = 0;
                private bool MBC6_FlashEnable = false;
                private bool MBC6_FlashWriteEnable = false;
                private bool MBC6_RomOrFlashASelect = false;
                private bool MBC6_RomOrFlashBSelect = false;
                private bool MBC6_RamEnable = false;

                // Flash
                private readonly byte[][] MBC6_RomFlashA = new byte[64][];
                private readonly byte[][] MBC6_RomFlashB = new byte[64][];

                // Read
                private byte MBC6_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // ROM/Flash Bank A 00-7F
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        // Max banks
                        MBC6_RomFBankA = (byte)(MBC6_RomFBankA & 0x7F);

                        int atInBank = 0x4000 * MBC6_RomFBankA + at - 0x4000;

                        if (RomData.Length > atInBank)
                        {
                            if(!MBC6_RomOrFlashASelect)
                            return RomData[atInBank];
                            else if(MBC6_FlashEnable)
                            return MBC6_RomFlashA[MBC6_RomFBankA][at - 0x4000];
                            else
                            return 0;
                        }

                        else
                        return 0;
                    }

                    // ROM/Flash Bank B 00-7F
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    {
                        // Max banks
                        MBC6_RomFBankB = (byte)(MBC6_RomFBankB & 0x7F);

                        int atInBank = 0x6000 * MBC6_RomFBankB + at - 0x6000;

                        if (RomData.Length > atInBank)
                        {
                            if(!MBC6_RomOrFlashBSelect)
                            return RomData[atInBank];
                            else if(MBC6_FlashEnable)
                            return MBC6_RomFlashB[MBC6_RomFBankB][at - 0x4000];
                            else
                            return 0;
                        }

                        else
                        return 0;
                    }

                    // RAM Bank A 00-07
                    else if(at >= 0xA000 && at <= 0xAFFF)
                    {
                        if (MBC6_RamEnable)
                        return ExternalRam[MBC6_RamFBankA][at - 0xA000];
                        else
                        return 0xFF;
                    }

                    // RAM Bank B 00-07
                    else
                    {
                        if (MBC6_RamEnable)
                        return ExternalRam[MBC6_RamFBankB][at - 0xA000];
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MBC6_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x03FF)
                    MBC6_RamEnable = (b & 0x0F) == 0x0A;

                    // RAM Bank A Number
                    else if (at >= 0x0400 && at <= 0x07FF)
                    MBC6_RamFBankA = (byte)(b & 0x7 & 0x07);

                    // RAM Bank B Number
                    else if (at >= 0x0800 && at <= 0x0BFF)
                    MBC6_RamFBankB = (byte)(b & 0x7 & 0x07);

                    // Flash Enable
                    else if (at >= 0x0800 && at <= 0x0BFF && MBC6_FlashWriteEnable)
                    MBC6_FlashEnable = (b & 1) == 1;

                    // Flash Write Enable
                    else if (at == 0x1000)
                    MBC6_FlashWriteEnable = (b & 1) == 1;

                    // ROM/Flash Bank A Number
                    else if (at >= 0x2000 && at <= 0x27FF)
                    MBC6_RomFBankA = b;

                    // ROM/Flash Bank A Select
                    else if (at >= 0x2800 && at <= 0x2FFF)
                    if(b == 0) MBC6_RomOrFlashASelect = false;
                    else if(b == 0x08) MBC6_RomOrFlashASelect = true;

                    // ROM/Flash Bank B Number
                    else if (at >= 0x3000 && at <= 0x37FF)
                    MBC6_RomFBankB = b;

                    // ROM/Flash Bank B Select
                    else if (at >= 0x3800 && at <= 0x3FFF)
                    if(b == 0) MBC6_RomOrFlashBSelect = false;
                    else if(b == 0x08) MBC6_RomOrFlashBSelect = true;
                }

                #endregion

                #region MBC7

                // MBCs Registers
                private bool MBC7_RamEnable1 = false;
                private bool MBC7_RamEnable2 = false;
                private ushort MBC7_X = 0;
                private ushort MBC7_Y = 0;
                private ushort MBC7_LockX = 0;
                private ushort MBC7_LockY = 0;

                // Read
                private byte MBC7_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // Max banks
                        selectedRomBank = (byte)(selectedRomBank & RomBankMask);

                        int atInBank = 0x4000 * selectedRomBank + at - 0x4000;

                        if (RomData.Length > atInBank)
                        return RomData[atInBank];

                        else
                        return 0;
                    }

                    // Registers
                    else if(at >= 0xA000 && at <= 0xAFFF && MBC7_RamEnable1 && MBC7_RamEnable2)
                    {
                        if((at & 0xF0F0) == 0xA000 || (at & 0xF0F0) == 0xA010)
                        return 0xFF;

                        else if((at & 0xF0F0) == 0xA020)
                        return Binary.Lsb(MBC7_LockX);
                        
                        else if((at & 0xF0F0) == 0xA030)
                        return Binary.Msb(MBC7_LockX);

                        else if((at & 0xF0F0) == 0xA040)
                        return Binary.Lsb(MBC7_LockY);
                        
                        else if((at & 0xF0F0) == 0xA050)
                        return Binary.Msb(MBC7_LockY);

                        else if((at & 0xF0F0) == 0xA060)
                        return 0;
                        
                        else if((at & 0xF0F0) == 0xA070)
                        return 0xFF;

                        else if((at & 0xF0F0) == 0xA080)
                        return 0xFF;

                        else if((at & 0xF0F0) == 0xA090 || (at & 0xF0F0) == 0xA0F0)
                        return 0xFF;

                        else
                        return 0xFF;
                    }
                    else if(at >= 0xB000 && at <= 0xBFFF)
                    return 0xFF;
                    else
                    return 0xFF;
                }

                // Write
                private void MBC7_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    MBC7_RamEnable1 = (b & 0x0F) == 0x0A;

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    selectedRomBank = b;

                    // ROM Bank Number - High bits
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    MBC7_RamEnable2 = (b & 0x0F) == 0x40;

                    // Registers
                    else if (at >= 0xA000 && at <= 0xAFFF)
                    {
                        if((at & 0xF0F0) == 0xA000 || (at & 0xF0F0) == 0xA010)
                        {
                            if(b == 0x55)
                            {
                                MBC7_X = 0x8000;
                                MBC7_Y = 0x8000;
                            }
                            else if (b == 0xAA)
                            {
                                MBC7_LockX = MBC7_X;
                                MBC7_LockY = MBC7_Y;
                            }
                        }
                    }
                }

                #endregion

                #region MMM01

                // MBCs Registers
                private byte MMM01_LowRegister = 0;
                private byte MMM01_HighRegister = 0;
                private bool MMM01_RamEnable = false;
                private bool MMM01_BankingModeSelect = false;
                private bool MMM01_Multiplex = false;
                private byte MMM01_RamBankMask = 0;

                // MMM01 or MMM01M
                private readonly byte MMM01_M_Shift = 5;
                private readonly byte MMM01_M_LowRegisterMask = 0x1F;

                // Read
                private byte MMM01_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    {
                        int atInBank = 0x4000 * (MMM01_Multiplex ? selectedRomBank00 : 0) + at;
                        return RomData[atInBank];
                    }

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // 00
                        if (MMM01_Multiplex && MMM01_LowRegister == 0)
                        MMM01_LowRegister |= 1;

                        // Set
                        selectedRomBank = (byte)((MMM01_HighRegister << MMM01_M_Shift) | (MMM01_LowRegister & MMM01_M_LowRegisterMask));

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
                        if (MMM01_RamEnable)
                        return ExternalRam[MMM01_Multiplex ? (selectedRamBank & MMM01_RamBankMask) : selectedRamBank][at - 0xA000];
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void MMM01_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    {
                        MMM01_RamEnable = (b & 0x0F) == 0x0A;
                        MMM01_RamBankMask = (byte)((b >> 4) & 3);
                    }

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    MMM01_LowRegister = (byte)(b & 0x1F);

                    // ROM Bank Number - High bits
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        if (!MMM01_BankingModeSelect)
                        {
                            MMM01_HighRegister = (byte)(b & 3);

                            // RomBank00 and RamBank = 0
                            selectedRomBank00 = 0;
                            selectedRamBank = 0;
                        }

                        else
                        {
                            // Set RomBank00
                            selectedRomBank00 = (byte)((b & 3) << MMM01_M_Shift);

                            // Max banks
                            selectedRomBank00 = (byte)(selectedRomBank00 & RomBankMask);

                            MMM01_HighRegister = (byte)(selectedRomBank00 >> MMM01_M_Shift);

                            // Set RamBank
                            selectedRamBank = (byte)(b & 3);
                            selectedRamBank = (byte)(selectedRamBank & RamBankMask);
                        }
                    }

                    // Banking Mode Select
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    MMM01_BankingModeSelect = Binary.ReadBit(b, 0);

                    // Exteral ram
                    else if (at >= 0xA000 && at <= 0xBFFF && MMM01_RamEnable)
                    ExternalRam[selectedRamBank][at - 0xA000] = b;
                }

                #endregion

                #region HuC1

                // MBCs Registers
                private byte HuC1_LowRegister = 0;
                private byte HuC1_HighRegister = 0;
                private bool HuC1_RamEnable = false;
                private bool HuC1_BankingModeSelect = false;

                // Read
                private byte HuC1_read(ushort at)
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
                        if (HuC1_LowRegister == 0)
                        HuC1_LowRegister |= 1;

                        // Set
                        selectedRomBank = (byte)((HuC1_HighRegister << 6) | (HuC1_LowRegister & 0x3F));

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
                        if (HuC1_RamEnable)
                        return ExternalRam[selectedRamBank][at - 0xA000];
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void HuC1_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    HuC1_RamEnable = (b & 0x0F) == 0x0A;

                    // ROM Bank Number - Low bits
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    HuC1_LowRegister = (byte)(b & 0x1F);

                    // ROM Bank Number - High bits
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    {
                        if (!HuC1_BankingModeSelect)
                        {
                            HuC1_HighRegister = (byte)(b & 3);

                            // RomBank00 and RamBank = 0
                            selectedRomBank00 = 0;
                            selectedRamBank = 0;
                        }

                        else
                        {
                            // Set RomBank00
                            selectedRomBank00 = (byte)((b & 3) << 6);

                            // Max banks
                            selectedRomBank00 = (byte)(selectedRomBank00 & RomBankMask);

                            HuC1_HighRegister = (byte)(selectedRomBank00 >> 6);

                            // Set RamBank
                            selectedRamBank = (byte)(b & 3);
                            selectedRamBank = (byte)(selectedRamBank & RamBankMask);
                        }
                    }

                    // Banking Mode Select
                    else if (at >= 0x6000 && at <= 0x7FFF)
                    HuC1_BankingModeSelect = Binary.ReadBit(b, 0);

                    // Exteral ram
                    else if (at >= 0xA000 && at <= 0xBFFF && HuC1_RamEnable)
                    ExternalRam[selectedRamBank][at - 0xA000] = b;
                }

                #endregion

                #region HuC3

                // MBCs Registers
                private byte HuC3_1FFFEnable = 0;

                // Read
                private byte HuC3_read(ushort at)
                {
                    // Rom bank 00
                    if (at >= 0 && at <= 0x3FFF)
                    return RomData[at];

                    // Rom bank 01~NN
                    else if (at >= 0x4000 && at <= 0x7FFF)
                    {
                        // Max banks
                        selectedRomBank = (byte)(selectedRomBank & 0x7F);

                        int atInBank = 0x4000 * selectedRomBank + at - 0x4000;

                        if (RomData.Length > atInBank)
                        return RomData[atInBank];

                        else
                        return 0;
                    }

                    // I/O Registers
                    else
                    {
                        // Ram
                        if (HuC3_1FFFEnable == 0 || HuC3_1FFFEnable == 0xA)
                        return ExternalRam[selectedRamBank][at - 0xA000];
                        // RTC command/response
                        else if(HuC3_1FFFEnable == 0xC)
                        return 0xFF;
                        // RTC semaphore
                        else if(HuC3_1FFFEnable == 0xD)
                        return 0xFF;
                        // IR
                        else if(HuC3_1FFFEnable == 0xE)
                        return 0xFF;
                        else
                        return 0xFF;
                    }
                }

                // Write
                private void HuC3_write(ushort at, byte b)
                {
                    // Ram enable
                    if (at >= 0 && at <= 0x1FFF)
                    HuC3_1FFFEnable = (byte)(b & 0x0F);

                    // ROM Bank Number
                    else if (at >= 0x2000 && at <= 0x3FFF)
                    selectedRomBank = (byte)(b & 0x7F);

                    // RAM Bank Number
                    else if (at >= 0x4000 && at <= 0x5FFF)
                    selectedRamBank = (byte)(b & 3);

                    // I/O Registers
                    else if (at >= 0xA000 && at <= 0xBFFF)
                    {
                        // Ram
                        if (HuC3_1FFFEnable == 0xA)
                        ExternalRam[selectedRamBank][at - 0xA000] = b;
                        // RTC Command/Argument
                        else if (HuC3_1FFFEnable == 0xB) { }
                        // RTC Semaphore
                        else if (HuC3_1FFFEnable == 0xD) { }
                        // IR
                        else if (HuC3_1FFFEnable == 0xE) { }
                    }
                }

                #endregion

            #endregion

        #endregion
    }

    #region Power up sequence

    public class PUS
    {
        // GameBoy generation
        public byte GameBoyGen = 0;

        // Rom boot
        public byte RomBoot = 0;

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
            // Gameboy generation set
            if(PUS_select >= 0 && PUS_select <= 2)
            GameBoyGen = 0;
            else if(PUS_select >= 3 && PUS_select <= 4)
            GameBoyGen = 1;
            else if(PUS_select >= 5 && PUS_select <= 6)
            GameBoyGen = 2;

            // Rom boot set
            RomBoot = PUS_select;

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