// ---------
// I/O Ports
// ---------
using GameBoyReborn;

namespace Emulator
{
    public class IO
    {
        #region Construct

        public PPU PPU;
        public APU APU;
        public Timer Timer;
        public Cartridge Cartridge;

        public IO(Emulation Emulation)
        {
            // Relation
            PPU = Emulation.PPU;
            APU = Emulation.APU;
            Timer = Emulation.Timer;
            Cartridge = Emulation.Cartridge;
        }

        #endregion

        #region IO operating variables

        //JOYP	Joypad
        public byte P1 = 0xCF;

        // Serial transfer
        public byte SB,SC;

        // Timer
        public byte DIV, TIMA, TMA, TAC;

        // Interrupt
        public byte IF, IE;

        // Sound
        public byte NR10, NR11, NR12, NR13, NR14, NR21, NR22, NR23, NR24, NR30, NR31, NR32, NR33, 
                    NR34, NR41, NR42, NR43, NR44, NR50, NR51, NR52, PCM12, PCM34;
        public byte[] WaveRAM = new byte[0x10];

        // Graphic
        public byte LCDC, STAT, SCY, SCX, LY, LYC, DMA, BGP, OBP0, OBP1, WY, WX, VBK, HDMA1, HDMA2, 
                    HDMA3, HDMA4, HDMA5, BCPS_BGPI, BCPD_BGPD, OCPS_OBPI, OCPD_OBPD, OPRI;

        // Other
        public byte KEY1; // Prepare speed switch
        public byte SVBK; // WRAM bank
        public byte RP; // Infrared communications port

        /// <summary>
        /// Init hardware registers
        /// </summary>
        public void Init()
        {
            P1 = Cartridge.PUS.P1;
            SB = Cartridge.PUS.SB;
            SC = Cartridge.PUS.SC;
            DIV = Cartridge.PUS.DIV;
            TIMA = Cartridge.PUS.TIMA;
            TMA = Cartridge.PUS.TMA;
            TAC = Cartridge.PUS.TAC;
            IF = Cartridge.PUS.IF;
            IE = Cartridge.PUS.IE;
            NR10 = Cartridge.PUS.NR10;
            NR11 = Cartridge.PUS.NR11;
            NR12 = Cartridge.PUS.NR12;
            NR13 = Cartridge.PUS.NR13;
            NR14 = Cartridge.PUS.NR14;
            NR21 = Cartridge.PUS.NR21;
            NR22 = Cartridge.PUS.NR22;
            NR23 = Cartridge.PUS.NR23;
            NR24 = Cartridge.PUS.NR24;
            NR30 = Cartridge.PUS.NR30;
            NR31 = Cartridge.PUS.NR31;
            NR32 = Cartridge.PUS.NR32;
            NR33 = Cartridge.PUS.NR33;
            NR34 = Cartridge.PUS.NR34;
            NR41 = Cartridge.PUS.NR41;
            NR42 = Cartridge.PUS.NR42;
            NR43 = Cartridge.PUS.NR43;
            NR44 = Cartridge.PUS.NR44;
            NR50 = Cartridge.PUS.NR50;
            NR51 = Cartridge.PUS.NR51;
            NR52 = Cartridge.PUS.NR52;
            LCDC = Cartridge.PUS.LCDC;
            STAT = Cartridge.PUS.STAT;
            SCY = Cartridge.PUS.SCY;
            SCX = Cartridge.PUS.SCX;
            LY = Cartridge.PUS.LY;
            LYC = Cartridge.PUS.LYC;
            DMA = Cartridge.PUS.DMA;
            BGP = Cartridge.PUS.BGP;
            OBP0 = Cartridge.PUS.OPB0;
            OBP1 = Cartridge.PUS.OBP1;
            WY = Cartridge.PUS.WY;
            WX = Cartridge.PUS.WX;
            VBK = Cartridge.PUS.VBK;
            HDMA1 = Cartridge.PUS.HDMA1;
            HDMA2 = Cartridge.PUS.HDMA2;
            HDMA3 = Cartridge.PUS.HDMA3;
            HDMA4 = Cartridge.PUS.HDMA4;
            HDMA5 = Cartridge.PUS.HDMA5;
            BCPS_BGPI = Cartridge.PUS.BCPS;
            BCPD_BGPD = Cartridge.PUS.BCPD;
            OCPS_OBPI = Cartridge.PUS.OCPS;
            OCPD_OBPD = Cartridge.PUS.OCPD;
            KEY1 = Cartridge.PUS.KEY1;
            RP = Cartridge.PUS.RP;
        }

        #endregion

        #region IO read

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

        #endregion

        #region IO write

        public void Write(byte at, byte b)
        {
            bool CGB_Support = Cartridge != null && Cartridge.PUS.GameBoyGen == 2;

            //JOYP	Joypad
            if (at == 0x00) { P1 = (byte)((P1 & 0xCF) | (b & 0x30)); Input.InputToByteGB(P1); }

            // Serial transfer
            else if (at == 0x01) SB = b;
            else if (at == 0x02) SC = 0x7E;

            // Timer
            else if (at == 0x04) Timer.DIV(b);
            else if (at == 0x05) TIMA = b;
            else if (at == 0x06) TMA = b;
            else if (at == 0x07) { b = (byte)(b | 0xF8); TAC = b;  Timer.TAC(b); }

            // Interrupt
            else if (at == 0x0F) IF = (byte)(b | 0xE0);
            else if (at == 0xFF) IE = b;

            // Sound
            else if (at == 0x10) { b = (byte)(b | 0x80); NR10 = b; APU.NR10(b); }
            else if (at == 0x11) { NR11 = b; APU.NR11(b); }
            else if (at == 0x12) { NR12 = b; APU.NR12(b); }
            else if (at == 0x13) { NR13 = b; APU.NR13(b); }
            else if (at == 0x14) { NR14 = b; APU.NR14(b); }
            else if (at == 0x16) { NR21 = b; APU.NR21(b); }
            else if (at == 0x17) { NR22 = b; APU.NR22(b); }
            else if (at == 0x18) { NR23 = b; APU.NR23(b); }
            else if (at == 0x19) { NR24 = b; APU.NR24(b); }
            else if (at == 0x1A) { b = (byte)(b | 0x7F); NR30 = b; APU.NR30(b); }
            else if (at == 0x1B) { NR31 = b; APU.NR31(b); }
            else if (at == 0x1C) { b = (byte)(b | 0x9F); NR32 = b; APU.NR32(b); }
            else if (at == 0x1D) { NR33 = b; APU.NR33(b); }
            else if (at == 0x1E) { NR34 = b; APU.NR34(b); }
            else if (at == 0x20) { b = (byte)(b | 0xC0); NR41 = b; APU.NR41(b); }
            else if (at == 0x21) { NR42 = b; APU.NR42(b); }
            else if (at == 0x22) { NR43 = b; APU.NR43(b); }
            else if (at == 0x23) { b = (byte)(b | 0x3F); NR44 = b; APU.NR44(b); }
            else if (at == 0x24) NR50 = b;
            else if (at == 0x25) NR51 = b;
            else if (at == 0x26) NR52 = (byte)(b | 0x70);
            else if (at == 0x76) PCM12 = (byte)(CGB_Support ? 0xFF : b);
            else if (at == 0x77) PCM34 = (byte)(CGB_Support ? 0xFF : b);
            else if (at >= 0x30 && at <= 0x3F) WaveRAM[at - 0x30] = b;

            // Graphic
            else if (at == 0x40) { LCDC = b; PPU.LCDC(b); }
            else if (at == 0x41) { b = (byte)(b | 0x80); STAT = b; PPU.STAT(b); }
            else if (at == 0x42) SCY = b;
            else if (at == 0x43) SCX = b;
            else if (at == 0x44) LY = b;
            else if (at == 0x45) LYC = b;
            else if (at == 0x46) { DMA = b; PPU.DMATransfer(b); }
            else if (at == 0x47) { BGP = b; PPU.BGP(b); }
            else if (at == 0x48) { OBP0 = b; PPU.OBP0(b); }
            else if (at == 0x49) { OBP1 = b; PPU.OBP1(b); }
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

        #endregion
    }
}
