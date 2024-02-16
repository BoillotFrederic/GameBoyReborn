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

        // Attribute of Cartridge
        private byte[] RomData;
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
        private byte HeaderChecksum;
        private ushort GlobalChecksum;
        public bool HeaderChecksumTest;
        public bool GlobalChecksumTest;

        // Rom/ram set
        private int RomLength;
        private ushort RomBankCount;
        private int RomBankMask;
        private int RamLength;
        public ushort RamBankCount;
        private int RamBankMask;

        public Cartridge(Emulation Emulation)
        {
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

            // Check if MBC1M
            if((Type == 0x01 || Type == 0x02 || Type == 0x03) && RomBankCount > 0x10)
            {
                byte[] Data0x10 = new byte[0x2F];
                Array.Copy(RomData, 0x10 * 0x4000 + 0x104, Data0x10, 0, 0x2F);

                if (NintendoLogo.SequenceEqual(Data0x10))
                {
                    MBC1_M_LowRegisterMask = 0xF;
                    MBC1_M_Shift = 4;
                }
            }

            // Delegate read/write
            MBCs();
        }

        private string GetLicense(ushort hex)
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

        private string GetTypeDescription(byte hex)
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


        private string GetCGBDescription(byte hex)
        {
            return hex switch
            {
                0x80 => "Game supports CGB functions, but works on old gameboys also.",
                0xC0 => "Game works on CGB only.",
                0x00 => "No CGB and no CGB functions supported",
                _ => "No CGB and no CGB functions supported",
            };
        }

        private string GetSGBDescription(byte hex)
        {
            return hex switch
            {
                0x00 => "No SGB functions (Normal Gameboy or CGB only game)",
                0x03 => "Game supports SGB functions",
                _ => "Only DMG",
            };
        }

        private string GetRamSizeDescription(byte hex)
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

        private string GetSizeDescription(byte hex)
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

        private int GetRamLength(byte hex)
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

        private int GetRomLength(byte hex)
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

        private ushort GetRomBankCount(byte hex)
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

        // Rom/ram set
        private ushort selectedRomBank00 = 0;
        private ushort selectedRomBank = 1;
        public byte selectedRamBank = 0;
        public byte[][] ExternalRam;

        // Delegate
        public delegate byte ReadDelegate(ushort at);
        public ReadDelegate Read;
        public delegate void WriteDelegate(ushort at, byte b);
        public WriteDelegate Write;

        // MBCs
        private void MBCs()
        {
            ExternalRam = new byte[RamBankCount][];

            for (byte i = 0; i < RamBankCount; i++)
            ExternalRam[i] = new byte[0x2000];

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

        // MBC 1
        // -----

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

        // MBC 1
        // -----

        // MBCs Registers
        private byte MBC1_LowRegister = 0;
        private byte MBC1_HighRegister = 0;
        private bool MBC1_RamEnable = false;
        private bool MBC1_BankingModeSelect = false;

        // MBC1 or MBC1M
        private byte MBC1_M_Shift = 5;
        private byte MBC1_M_LowRegisterMask = 0x1F;

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
                {
                    Console.WriteLine("Invalid rom bank : #" + selectedRomBank);
                    return 0;
                }
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
            else if (at >= 0x2000 && at <= 0x3FFF && RomBankCount > 2)
            MBC1_LowRegister = (byte)(b & 0x1F);

            // ROM Bank Number - High bits
            else if (at >= 0x4000 && at <= 0x5FFF && RomBankCount > 2)
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
    }
}