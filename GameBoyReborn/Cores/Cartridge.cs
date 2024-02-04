// ---------
// Cartridge
// ---------
using System.Text;
using Raylib_cs;

namespace GameBoyReborn
{
    public class Cartridge
    {
        // Attribute of Cartridge
        public string Title;
        public string ManufacturerCode;
        public byte CGB_Flag;
        public string CGBDescription;
        public string Licensee;
        public byte SGB_Flag;
        public string SGBDescription;
        public byte Type;
        public string TypeDescription;
        public string SizeDescription;
        public SizeStruct Size;
        public string RamSizeDescription;
        public int RamSize;
        public string DestinationCode;
        public struct SizeStruct
        {
            public int Byte { get; set; }
            public ushort Bank { get; set; }
        }

        public Cartridge(byte[] header)
        {
            byte[] TitleRange = new byte[11];
            byte[] ManufacturerCodeRange = new byte[4];
            byte[] LicenseeRange = new byte[2];

            Array.Copy(header, 0, TitleRange, 0, 11);
            Array.Copy(header, 0x0B, ManufacturerCodeRange, 0, 4);
            Array.Copy(header, 0x0D, LicenseeRange, 0, 2);

            ManufacturerCode = Encoding.ASCII.GetString(ManufacturerCodeRange).TrimEnd('\0');
            Title = Encoding.ASCII.GetString(TitleRange).TrimEnd('\0');
            CGB_Flag = header[0x0C];
            CGBDescription = GetCGBDescription(header[0x0C]);
            Licensee = GetLicense(BitConverter.ToUInt16(LicenseeRange));
            SGB_Flag = header[0x0F];
            SGBDescription = GetSGBDescription(header[0x0F]);
            Type = header[0x10];
            TypeDescription = GetTypeDescription(header[0x10]);
            SizeDescription = GetSizeDescription(header[0x11]);
            RamSizeDescription = GetRamSizeDescription(header[0x12]);
            RamSize = SetRamSize(header[0x12]);
            DestinationCode = header[0x13] == 0x00 ? "Japanese" : "Non-Japanese";

            SetSize(header[0x11], header[0x10]);
        }

        private static string GetLicense(ushort hex)
        {
            Dictionary<ushort, string> licences = new()
            {
                { 0x00, "none" },
                { 0x01, "Nintendo R&D1" },
                { 0x08, "Capcom" },
                { 0x13, "Electronic Arts" },
                { 0x18, "Hudson Soft" },
                { 0x19, "b-ai" },
                { 0x20, "kss" },
                { 0x22, "pow" },
                { 0x24, "PCM Complete" },
                { 0x25, "san-x" },
                { 0x28, "Kemco Japan" },
                { 0x29, "seta" },
                { 0x30, "Viacom" },
                { 0x31, "Nintendo" },
                { 0x32, "Bandai" },
                { 0x33, "Ocean/Acclaim" },
                { 0x34, "Konami" },
                { 0x35, "Hector" },
                { 0x37, "Taito" },
                { 0x38, "Hudson" },
                { 0x39, "Banpresto" },
                { 0x41, "Ubi Soft" },
                { 0x42, "Atlus" },
                { 0x44, "Malibu" },
                { 0x46, "angel" },
                { 0x47, "Bullet-Proof" },
                { 0x49, "irem" },
                { 0x50, "Absolute" },
                { 0x51, "Acclaim" },
                { 0x52, "Activision" },
                { 0x53, "American sammy" },
                { 0x54, "Konami" },
                { 0x55, "Hi tech entertainment" },
                { 0x56, "LJN" },
                { 0x57, "Matchbox" },
                { 0x58, "Mattel" },
                { 0x59, "Milton Bradley" },
                { 0x60, "Titus" },
                { 0x61, "Virgin" },
                { 0x64, "LucasArts" },
                { 0x67, "Ocean" },
                { 0x69, "Electronic Arts" },
                { 0x70, "Infogrames" },
                { 0x71, "Interplay" },
                { 0x72, "Broderbund" },
                { 0x73, "sculptured" },
                { 0x75, "sci" },
                { 0x78, "THQ" },
                { 0x79, "Accolade" },
                { 0x80, "misawa" },
                { 0x83, "lozc" },
                { 0x86, "tokuma shoten i*" },
                { 0x87, "tsukuda ori*" },
                { 0x91, "Chunsoft" },
                { 0x92, "Video system" },
                { 0x93, "Ocean/Acclaim" },
                { 0x95, "Varie" },
                { 0x96, "Yonezawa/s'pal" },
                { 0x97, "Kaneko" },
                { 0x99, "Pack in soft" },
                { 0xA4, "Konami (Yu-Gi-Oh!)" }
            };

            if (licences.ContainsKey(hex)) return licences[hex];
            else return "";
        }

        private static string GetTypeDescription(byte hex)
        {
            Dictionary<byte, string> types = new()
            {
                { 0x00, "ROM ONLY" },
                { 0x01, "MBC1" },
                { 0x02, "MBC1+RAM" },
                { 0x03, "MBC1+RAM+BATTERY" },
                { 0x05, "MBC2" },
                { 0x06, "MBC2+BATTERY" },
                { 0x08, "ROM+RAM" },
                { 0x09, "ROM+RAM+BATTERY" },
                { 0x0B, "MMM01" },
                { 0x0C, "MMM01+RAM" },
                { 0x0D, "MMM01+RAM+BATTERY" },
                { 0x0F, "MBC3+TIMER+BATTERY" },
                { 0x10, "MBC3+TIMER+RAM+BATTERY" },
                { 0x19, "MBC5" },
                { 0x1A, "MBC5+RAM" },
                { 0x1B, "MBC5+RAM+BATTERY" },
                { 0x1C, "MBC5+RUMBLE" },
                { 0x1D, "MBC5+RUMBLE+RAM" },
                { 0x1E, "MBC5+RUMBLE+RAM+BATTERY" },
                { 0x20, "MBC6" },
                { 0x22, "MBC7+SENSOR+RUMBLE+RAM+BATTERY" },
                { 0xFC, "POCKET CAMERA" },
                { 0xFD, "BANDAI TAMA5" },
                { 0xFE, "HuC3" },
                { 0xFF, "HuC1+RAM+BATTERY" }
            };

            if (types.ContainsKey(hex)) return types[hex];
            else return "";
        }


        private static string GetCGBDescription(byte hex)
        {
            Dictionary<byte, string> CGB = new()
            {
                { 0x80, "Game supports CGB functions, but works on old gameboys also." },
                { 0xC0, "Game works on CGB only." },
                { 0x00, "No CGB and no CGB functions supported" }
            };

            if (CGB.ContainsKey(hex)) return CGB[hex];
            else return "";
        }

        private static string GetSGBDescription(byte hex)
        {
            Dictionary<byte, string> SGB = new()
            {
                { 0x00, "No SGB functions (Normal Gameboy or CGB only game)" },
                { 0x03, "Game supports SGB functions" }
            };

            if (SGB.ContainsKey(hex)) return SGB[hex];
            else return "";
        }

        private static string GetRamSizeDescription(byte hex)
        {
            Dictionary<byte, string> ramSize = new()
            {
                { 0x00, "None" },
                { 0x01, "2 KBytes" },
                { 0x02, "8 KBytes" },
                { 0x03, "32 KBytes (4 banks of 8KBytes each)" },
                { 0x04, "128 KBytes (16 banks of 8KBytes each)" },
                { 0x05, "64 KBytes (8 banks of 8KBytes each)" }
            };

            if (ramSize.ContainsKey(hex)) return ramSize[hex];
            else return "";
        }

        private static int SetRamSize(byte hex)
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

        private static string GetSizeDescription(byte hex)
        {
            Dictionary<byte, string> sizes = new()
            {
                { 0x00, "32KByte (no ROM banking)" },
                { 0x01, "64KByte (4 banks)" },
                { 0x02, "128KByte (8 banks)" },
                { 0x03, "256KByte (16 banks)" },
                { 0x04, "512KByte (32 banks)" },
                { 0x05, "1MByte (64 banks) - only 63 banks used by MBC1" },
                { 0x06, "2MByte (128 banks) - only 125 banks used by MBC1" },
                { 0x07, "4MByte (256 banks)" },
                { 0x08, "8MByte (512 banks)" },
                { 0x52, "1.1MByte (72 banks)" },
                { 0x53, "1.2MByte (80 banks)" },
                { 0x54, "1.5MByte (96 banks)" }
            };

            if (sizes.ContainsKey(hex)) return sizes[hex];
            else return "";
        }

        private void SetSize(byte hex, byte type)
        {
            switch (hex)
            {
                case 0x00:
                    Size.Byte = 32 * 1024;
                    Size.Bank = 1;
                break;
                case 0x01:
                    Size.Byte = 64 * 1024;
                    Size.Bank = 4;
                break;
                case 0x02:
                    Size.Byte = 128 * 1024;
                    Size.Bank = 8;
                break;
                case 0x03:
                    Size.Byte = 256 * 1024;
                    Size.Bank = 16;
                break;
                case 0x04:
                    Size.Byte = 512 * 1024;
                    Size.Bank = 32;
                break;
                case 0x05:
                    Size.Byte = 1 * 1024 * 1024;
                    Size.Bank = (ushort)((type == 0x01) ? 63 : 64);
                break;
                case 0x06:
                    Size.Byte = 2 * 1024 * 1024;
                    Size.Bank = (ushort)((type == 0x01) ? 125 : 128);
                break;
                case 0x07:
                    Size.Byte = 4 * 1024 * 1024;
                    Size.Bank = 256;
                break;
                case 0x08:
                    Size.Byte = 8 * 1024 * 1024;
                    Size.Bank = 512;
                break;
                case 0x52:
                    Size.Byte = 1_100_000;
                    Size.Bank = 72;
                break;
                case 0x53:
                    Size.Byte = 1_200_000;
                    Size.Bank = 80;
                break;
                case 0x54:
                    Size.Byte = 1_500_000;
                    Size.Bank = 96;
                break;
                default:
                    Size.Byte = 32 * 1024;
                    Size.Bank = 1;
                break;
            }
        }
    }
}