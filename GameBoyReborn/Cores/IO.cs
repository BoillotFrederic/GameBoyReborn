// ---------
// I/O Ports
// ---------
public class IO
{
    //JOYP	Joypad
    public byte P1 = 0xCF;

    // Serial transfer
    public byte SB = 0x00;
    public byte SC = 0x7E;

    // Timer
    public byte DIV = 0xAC;
    public byte TIMA = 0x00;
    public byte TMA = 0x00;
    public byte TAC = 0xF8;

    // Interrupt
    public byte IF = 0xE1;
    public byte IE = 0x00;

    // Sound
    public byte NR10;
    public byte NR11;
    public byte NR12;
    public byte NR13;
    public byte NR14;
    public byte NR21;
    public byte NR22;
    public byte NR23;
    public byte NR24;
    public byte NR30;
    public byte NR31;
    public byte NR32; 
    public byte NR33;
    public byte NR34;
    public byte NR41;
    public byte NR42;
    public byte NR43;
    public byte NR44;
    public byte NR50;
    public byte NR51;
    public byte NR52;
    public byte PCM12;
    public byte PCM34;
    public byte[] WaveRAM = new byte[0x10];

    // Graphic
    public byte LCDC = 0x91;
    public byte STAT = 0x81;
    public byte SCY = 0x00;
    public byte SCX = 0x00;
    public byte LY = 0x00;
    public byte LYC = 0x00;
    public byte DMA = 0xFF;
    public byte BGP = 0xFC;
    public byte OBP0 = 0xFF;
    public byte OBP1 = 0xFF;
    public byte WY = 0x00;
    public byte WX = 0x00;
    public byte VBK;
    public byte HDMA1;
    public byte HDMA2;
    public byte HDMA3;
    public byte HDMA4;
    public byte HDMA5;
    public byte BCPS_BGPI;
    public byte BCPD_BGPD;
    public byte OCPS_OBPI;
    public byte OCPD_OBPD;
    public byte OPRI;

    // Other
    public byte KEY1; // Prepare speed switch
    public byte SVBK; // WRAM bank
    public byte RP; // Infrared communications port

    public byte Read(ushort at)
    {
        //Console.WriteLine(at);

        //JOYP	Joypad
        if (at == 0xFF00) return P1;

        // Serial transfer
        else if (at == 0xFF00) return SB;
        else if (at == 0xFF02) return SC;

        // Timer
        else if (at == 0xFF04) return DIV;
        else if (at == 0xFF05) return TIMA;
        else if (at == 0xFF06) return TMA;
        else if (at == 0xFF07) return TAC;

        // Interrupt
        else if (at == 0xFF0F) return IF;
        else if (at == 0xFFFF) return IE;

        // Sound
        else if (at == 0xFF10) return NR10;
        else if (at == 0xFF11) return NR11;
        else if (at == 0xFF12) return NR12;
        else if (at == 0xFF13) return NR13;
        else if (at == 0xFF14) return NR14;
        else if (at == 0xFF16) return NR21;
        else if (at == 0xFF17) return NR22;
        else if (at == 0xFF18) return NR23;
        else if (at == 0xFF19) return NR24;
        else if (at == 0xFF1A) return NR30;
        else if (at == 0xFF1B) return NR31;
        else if (at == 0xFF1C) return NR32;
        else if (at == 0xFF1D) return NR33;
        else if (at == 0xFF1E) return NR34;
        else if (at == 0xFF20) return NR41;
        else if (at == 0xFF21) return NR42;
        else if (at == 0xFF22) return NR43;
        else if (at == 0xFF23) return NR44;
        else if (at == 0xFF24) return NR50;
        else if (at == 0xFF25) return NR51;
        else if (at == 0xFF26) return NR52;
        else if (at == 0xFF76) return PCM12;
        else if (at == 0xFF77) return PCM34;
        else if (at >= 0xFF30 && at <= 0xFF3F) return WaveRAM[at - 0xFF3F];

        // Graphic
        else if (at == 0xFF40) return LCDC;
        else if (at == 0xFF41) return STAT;
        else if (at == 0xFF42) return SCY;
        else if (at == 0xFF43) return SCX;
        else if (at == 0xFF44) return LY;
        else if (at == 0xFF45) return LYC;
        else if (at == 0xFF46) return DMA;
        else if (at == 0xFF47) return BGP;
        else if (at == 0xFF48) return OBP0;
        else if (at == 0xFF49) return OBP1;
        else if (at == 0xFF4A) return WY;
        else if (at == 0xFF4B) return WX;
        else if (at == 0xFF4F) return VBK;
        else if (at == 0xFF51) return HDMA1;
        else if (at == 0xFF52) return HDMA2;
        else if (at == 0xFF53) return HDMA3;
        else if (at == 0xFF54) return HDMA4;
        else if (at == 0xFF55) return HDMA5;
        else if (at == 0xFF68) return BCPS_BGPI;
        else if (at == 0xFF69) return BCPD_BGPD;
        else if (at == 0xFF6A) return OCPS_OBPI;
        else if (at == 0xFF6B) return OCPD_OBPD;
        else if (at == 0xFF6C) return OPRI;

        // Other
        else if (at == 0xFF4C) return KEY1;
        else if (at == 0xFF56) return RP;
        else if (at == 0xFF70) return SVBK;

        // None
        else return 0x00;
    }

    public void Write(ushort at, byte b)
    {
        //JOYP	Joypad
        if (at == 0xFF00) P1 = b;

        // Serial transfer
        else if (at == 0xFF00) SB = b;
        else if (at == 0xFF02) SC = b;

        // Timer
        else if (at == 0xFF04) DIV = 0x00;
        else if (at == 0xFF05) TIMA = b;
        else if (at == 0xFF06) TMA = b;
        else if (at == 0xFF07) TAC = b;

        // Interrupt
        else if (at == 0xFF0F) IF = b;
        else if (at == 0xFFFF) IE = b;

        // Sound
        else if (at == 0xFF10) NR10 = b;
        else if (at == 0xFF11) NR11 = b;
        else if (at == 0xFF12) NR12 = b;
        else if (at == 0xFF13) NR13 = b;
        else if (at == 0xFF14) NR14 = b;
        else if (at == 0xFF16) NR21 = b;
        else if (at == 0xFF17) NR22 = b;
        else if (at == 0xFF18) NR23 = b;
        else if (at == 0xFF19) NR24 = b;
        else if (at == 0xFF1A) NR30 = b;
        else if (at == 0xFF1B) NR31 = b;
        else if (at == 0xFF1C) NR32 = b;
        else if (at == 0xFF1D) NR33 = b;
        else if (at == 0xFF1E) NR34 = b;
        else if (at == 0xFF20) NR41 = b;
        else if (at == 0xFF21) NR42 = b;
        else if (at == 0xFF22) NR43 = b;
        else if (at == 0xFF23) NR44 = b;
        else if (at == 0xFF24) NR50 = b;
        else if (at == 0xFF25) NR51 = b;
        else if (at == 0xFF26) NR52 = b;
        else if (at == 0xFF76) PCM12 = b;
        else if (at == 0xFF77) PCM34 = b;
        else if (at >= 0xFF30 && at <= 0xFF3F) WaveRAM[at - 0xFF3F] = b;

        // Graphic
        else if (at == 0xFF40) LCDC = b;
        else if (at == 0xFF41) STAT = b;
        else if (at == 0xFF42) SCY = b;
        else if (at == 0xFF43) SCX = b;
        else if (at == 0xFF44) LY = b;
        else if (at == 0xFF45) LYC = b;
        //else if (at == 0xFF46) DMA = b;
        else if (at == 0xFF47) BGP = b;
        else if (at == 0xFF48) OBP0 = b;
        else if (at == 0xFF49) OBP1 = b;
        else if (at == 0xFF4A) WY = b;
        else if (at == 0xFF4B) WX = b;
        else if (at == 0xFF4F) VBK = b;
        else if (at == 0xFF51) HDMA1 = b;
        else if (at == 0xFF52) HDMA2 = b;
        else if (at == 0xFF53) HDMA3 = b;
        else if (at == 0xFF54) HDMA4 = b;
        else if (at == 0xFF55) HDMA5 = b;
        else if (at == 0xFF68) BCPS_BGPI = b;
        else if (at == 0xFF69) BCPD_BGPD = b;
        else if (at == 0xFF6A) OCPS_OBPI = b;
        else if (at == 0xFF6B) OCPD_OBPD = b;
        else if (at == 0xFF6C) OPRI = b;

        // Other
        else if (at == 0xFF4C) KEY1 = b;
        else if (at == 0xFF56) RP = b;
        else if (at == 0xFF70) SVBK = b;
    }
}
