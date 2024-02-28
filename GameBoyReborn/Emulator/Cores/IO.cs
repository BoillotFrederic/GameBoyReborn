// ---------
// I/O Ports
// ---------
using GameBoyReborn;

namespace Emulator
{
    public class IO
    {
        // Construct
        public PPU PPU;
        public APU APU;
        public Timer Timer;
        public Cartridge Cartridge;

        public IO(Emulation Emulation)
        {
            PPU = Emulation.PPU;
            APU = Emulation.APU;
            Timer = Emulation.Timer;
            Cartridge = Emulation.Cartridge;
        }

        //JOYP	Joypad
        public byte P1 = 0xCF;

        // Serial transfer
        public byte SB = 0x00;
        public byte SC = 0x7E;

        // Timer
        public byte DIV = 0xAB;
        public byte TIMA = 0x00;
        public byte TMA = 0x00;
        public byte TAC = 0xF8;

        // Interrupt
        public byte IF = 0xE1;
        public byte IE = 0x00;

        // Sound
        public byte NR10 = 0x80;
        public byte NR11 = 0xBF;
        public byte NR12 = 0xF3;
        public byte NR13 = 0xFF;
        public byte NR14 = 0xBF;
        public byte NR21 = 0x3F;
        public byte NR22 = 0x00;
        public byte NR23 = 0xFF;
        public byte NR24 = 0xBF;
        public byte NR30 = 0x7F;
        public byte NR31 = 0xFF;
        public byte NR32 = 0x9F;
        public byte NR33 = 0xFF;
        public byte NR34 = 0xBF;
        public byte NR41 = 0xFF;
        public byte NR42 = 0x00;
        public byte NR43 = 0x00;
        public byte NR44 = 0xBF;
        public byte NR50 = 0x77;
        public byte NR51 = 0xF3;
        public byte NR52 = 0xF1;
        public byte PCM12;
        public byte PCM34;
        public byte[] WaveRAM = new byte[0x10];

        // Graphic
        public byte LCDC = 0x91;
        public byte STAT = 0x81;
        public byte SCY = 0x00;
        public byte SCX = 0x00;
        public byte LY = 0x91;
        public byte LYC = 0x00;
        public byte DMA = 0xFF;
        public byte BGP = 0xFC;
        public byte OBP0 = 0x00;
        public byte OBP1 = 0x00;
        public byte WY = 0x00;
        public byte WX = 0x00;
        public byte VBK = 0xFF;
        public byte HDMA1 = 0xFF;
        public byte HDMA2 = 0xFF;
        public byte HDMA3 = 0xFF;
        public byte HDMA4 = 0xFF;
        public byte HDMA5 = 0xFF;
        public byte BCPS_BGPI = 0xFF;
        public byte BCPD_BGPD = 0xFF;
        public byte OCPS_OBPI = 0xFF;
        public byte OCPD_OBPD = 0xFF;
        public byte OPRI;

        // Other
        public byte KEY1 = 0xFF; // Prepare speed switch
        public byte SVBK = 0xFF; // WRAM bank
        public byte RP = 0xFF; // Infrared communications port

        public byte Read(byte at)
        {
            //JOYP	Joypad
            if (at == 0x00) return Input.InputToByteGB(P1);

            // Serial transfer
            else if (at == 0x01) return SB;
            else if (at == 0x02) return SC;

            // Timer
            else if (at == 0x04) return DIV;
            else if (at == 0x05) return TIMA;
            else if (at == 0x06) return TMA;
            else if (at == 0x07) return TAC;

            // Interrupt
            else if (at == 0x0F) return IF;
            else if (at == 0xFF) return IE;

            // Sound
            else if (at == 0x10) return NR10;
            else if (at == 0x11) return NR11;
            else if (at == 0x12) return NR12;
            else if (at == 0x13) return NR13;
            else if (at == 0x14) return NR14;
            else if (at == 0x16) return NR21;
            else if (at == 0x17) return NR22;
            else if (at == 0x18) return NR23;
            else if (at == 0x19) return NR24;
            else if (at == 0x1A) return NR30;
            else if (at == 0x1B) return NR31;
            else if (at == 0x1C) return NR32;
            else if (at == 0x1D) return NR33;
            else if (at == 0x1E) return NR34;
            else if (at == 0x20) return NR41;
            else if (at == 0x21) return NR42;
            else if (at == 0x22) return NR43;
            else if (at == 0x23) return NR44;
            else if (at == 0x24) return NR50;
            else if (at == 0x25) return NR51;
            else if (at == 0x26) return NR52;
            else if (at == 0x76) return PCM12;
            else if (at == 0x77) return PCM34;
            else if (at >= 0x30 && at <= 0x3F) return WaveRAM[at - 0x30];

            // Graphic
            else if (at == 0x40) return LCDC;
            else if (at == 0x41) return STAT;
            else if (at == 0x42) return SCY;
            else if (at == 0x43) return SCX;
            else if (at == 0x44) return LY;
            else if (at == 0x45) return LYC;
            else if (at == 0x46) return DMA;
            else if (at == 0x47) return BGP;
            else if (at == 0x48) return OBP0;
            else if (at == 0x49) return OBP1;
            else if (at == 0x4A) return WY;
            else if (at == 0x4B) return WX;
            else if (at == 0x4F) return VBK;
            else if (at == 0x51) return HDMA1;
            else if (at == 0x52) return HDMA2;
            else if (at == 0x53) return HDMA3;
            else if (at == 0x54) return HDMA4;
            else if (at == 0x55) return HDMA5;
            else if (at == 0x68) return BCPS_BGPI;
            else if (at == 0x69) return BCPD_BGPD;
            else if (at == 0x6A) return OCPS_OBPI;
            else if (at == 0x6B) return OCPD_OBPD;
            else if (at == 0x6C) return OPRI;

            // Other
            else if (at == 0x4D) return KEY1;
            else if (at == 0x56) return RP;
            else if (at == 0x70) return SVBK;

            // None
            else return 0xFF;
        }

        public void Write(byte at, byte b)
        {
            bool CGB_Support = Cartridge != null && (Cartridge.CGB_Flag != 0x80 || Cartridge.CGB_Flag != 0xC0);

            //JOYP	Joypad
            if (at == 0x00) { P1 = (byte)((P1 & 0xCF) | (b & 0x30)); Input.InputToByteGB(P1); }

            // Serial transfer
            else if (at == 0x01) SB = b;
            else if (at == 0x02) SC = 0x7E;

            // Timer
            else if (at == 0x04) Timer.DIV(b);
            else if (at == 0x05) TIMA = b;
            else if (at == 0x06) TMA = b;
            else if (at == 0x07) Timer.TAC((byte)(b | 0xF8));

            // Interrupt
            else if (at == 0x0F) IF = (byte)(b | 0xE0);
            else if (at == 0xFF) IE = b;

            // Sound
            else if (at == 0x10) APU.CH1_WriteNR10((byte)(b | 0x80));
            else if (at == 0x11) APU.CH1_WriteNR11(b);
            else if (at == 0x12) APU.CH1_WriteNR12(b);
            else if (at == 0x13) APU.CH1_WriteNR13(b);
            else if (at == 0x14) APU.CH1_WriteNR14(b);
            else if (at == 0x16) APU.CH2_WriteNR21(b);
            else if (at == 0x17) APU.CH2_WriteNR22(b);
            else if (at == 0x18) APU.CH2_WriteNR23(b);
            else if (at == 0x19) APU.CH2_WriteNR24(b);
            else if (at == 0x1A) APU.CH3_WriteNR30((byte)(b | 0x7F));
            else if (at == 0x1B) APU.CH3_WriteNR31(b);
            else if (at == 0x1C) APU.CH3_WriteNR32((byte)(b | 0x9F));
            else if (at == 0x1D) APU.CH3_WriteNR33(b);
            else if (at == 0x1E) APU.CH3_WriteNR34(b);
            else if (at == 0x20) NR41 = (byte)(b | 0xC0);
            else if (at == 0x21) NR42 = b;
            else if (at == 0x22) NR43 = b;
            else if (at == 0x23) NR44 = (byte)(b | 0x3F);
            else if (at == 0x24) NR50 = b;
            else if (at == 0x25) NR51 = b;
            else if (at == 0x26) NR52 = (byte)(b | 0x70);
            else if (at == 0x76) PCM12 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x77) PCM34 = (byte)(CGB_Support ? 0xFF : b);
            else if (at >= 0x30 && at <= 0x3F) WaveRAM[at - 0x30] = b;

            // Graphic
            else if (at == 0x40) PPU.LCDC(b);
            else if (at == 0x41) PPU.STAT((byte)(b | 0x80));
            else if (at == 0x42) SCY = b;
            else if (at == 0x43) SCX = b;
            else if (at == 0x44) LY = b;
            else if (at == 0x45) LYC = b;
            else if (at == 0x46) PPU.DMATransfer(b);
            else if (at == 0x47) PPU.BGP(b);
            else if (at == 0x48) PPU.OBP0(b);
            else if (at == 0x49) PPU.OBP1(b);
            else if (at == 0x4A) WY = b;
            else if (at == 0x4B) WX = b;
            else if (at == 0x4F) VBK = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x51) HDMA1 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x52) HDMA2 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x53) HDMA3 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x54) HDMA4 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x55) HDMA5 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x68) BCPS_BGPI = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x69) BCPD_BGPD = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x6A) OCPS_OBPI = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x6B) OCPD_OBPD = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x6C) OPRI = (byte)(CGB_Support ? 0xFF : b);

            // Other
            else if (at == 0x4D) KEY1 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x56) RP = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x70) SVBK = (byte)(CGB_Support ? 0xFF : b);
        }
    }
}
