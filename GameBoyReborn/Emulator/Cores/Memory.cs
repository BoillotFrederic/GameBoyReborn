﻿// ------
// Memory
// ------
using GameBoyReborn;

namespace Emulator
{
    public class Memory
    {
        public byte[][] VideoRam_nn;
        public byte[] ExternalRam = new byte[0x2000];
        public byte[] WorkRam = new byte[0x1000];
        public byte[][] WorkRamCGB;
        public byte[] EchoRam = new byte[0x1E00];
        public byte[] OAM = new byte[0xA0];
        public byte[] NotUsable = new byte[0x60];
        public byte[] HighRAM = new byte[0x7F];

        public ushort selectedRomBank00 = 0;
        public ushort selectedRomBank = 1;
        public byte selectedRamBank = 0;
        public byte selectedVideoBank = 0;
        public byte selectedWorkBank = 0;

        private byte[] RomBoot = new byte[256];
        public bool booting = false;

        private readonly IO IO;
        private readonly Cartridge Cartridge;

        // Init ram
        public Memory(Emulation Emulation)
        {
            // Relation
            IO = Emulation.IO;
            Cartridge = Emulation.Cartridge;

            // Load rom boot
            LoadRomBoot();

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
            return Cartridge.Read(at);

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
            Cartridge.Write(at, b);

            // Video RAM
            else if (at >= 0x8000 && at <= 0x9FFF)
            VideoRam_nn[selectedVideoBank][at - 0x8000] = b;

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
            else if ((at >= 0xFF00 && at <= 0xFF7F || at == 0xFFFF))
            IO.Write((byte)(at - 0xFF00), b);

            // High RAM (HRAM)
            else if (at >= 0xFF80 && at <= 0xFFFE)
            HighRAM[at - 0xFF80] = b;
        }

        // Rom boot
        private void LoadRomBoot()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Boot/DMG_ROM.bin"))
            RomBoot = File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "Boot/DMG_ROM.bin");

            else
            booting = false;
        }
    }
}
