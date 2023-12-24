// ---
// CPU
// ---
public class CPU
{
    // Handle memory
    Memory Memory;
    public IO? IO;
    public delegate byte ReadDelegate(ushort at);
    public ReadDelegate Read;
    public delegate void WriteDelegate(ushort at, byte b);
    public WriteDelegate Write;

    // Cycles
    public int Cycles;

    // Program counter
    public ushort PC = 0x0000;

    // Stack pointer
    private ushort SP = 0xFFFE;

    // Registers
    private byte A = 0x01;
    private byte B = 0x00;
    private byte C = 0x13;
    private byte D = 0x00;
    private byte E = 0xD8;
    private byte H = 0x01;
    private byte L = 0x4D;

    // Interrupt
    private bool Stop = false;
    private bool Halt = false;
    public bool IME = false;
    public bool IME_scheduled = false;

    // Timer
    private ushort DivCycles = 0;
    private int TacCycles = 0;
    private readonly int[] ClockCPU = new int[4] { 4096, 262144, 65536, 16384 };

    // Handle flags
    private bool FlagZ = true;
    private bool FlagN = false;
    private bool FlagH = true;
    private bool FlagC = true;

    // Handle binary
    private byte Lsb(ushort u16) { return (byte)(u16 & 0xFF); } // Least significant bit
    private byte Msb(ushort u16) { return (byte)((u16 >> 8) & 0xFF); } // Most significant bit
    private ushort u16(byte lsb, byte msb) { return (ushort)((msb << 8) | lsb); } // Create unsigned 16
    private bool ReadBit(byte data, byte pos) { return ((data >> pos) & 0x01) == 1; } // From right to left
    private void SetBit(ref byte data, byte pos, bool set) { if(set) data |= (byte)(1 << pos); else data &= (byte)~(1 << pos); } // Set/unset bit

    // Execution
    // ---------
    public void Execution()
    {
        int LastCycles = Cycles;

        // Enable CPU
        if (IME) CPUWakeUp();

        // Run instructions
        if (!Halt && !Stop)
        Instructions[Read(PC++)]?.Invoke();
        else
        {
            PC++;
            Cycles++;
        }

        // Set timer
        Timer(LastCycles);

        // Enable CPU
        if (IME_scheduled) CPUWakeUp();
    }

    // Init and instructions
    // ---------------------
    private delegate void InstructionDelegate();
    private InstructionDelegate[] Instructions;
    private InstructionDelegate[] CB_Instructions;

    public CPU(Memory _Memory)
    {
        // Init memory
        Memory = _Memory;
        Read = Memory.Read;
        Write = Memory.Write;

        // Instructions
        Instructions = new InstructionDelegate[]
        {   /*          [0]                           [1]                             [2]                           [3]                           [4]                             [5]                        [6]                      [7]                        [8]                          [9]                                  [A]                          [B]                           [C]                            [D]                        [E]                      [F]      */
            /* [0] */   ()=> NOP(),                   ()=> LD_RRw_nn(ref B, ref C),   ()=> LD_RRn_A(B, C),          ()=> INC_RRw(ref B, ref C),   ()=> INC_Rw(ref B),             ()=> DEC_Rw(ref B),        ()=> LD_Rw_n(ref B),     ()=> RLCA(),               ()=> LD_Nnn_SP(),            ()=> ADD_HL_RRr(B, C),               ()=> LD_A_RRn(B, C),         ()=> DEC_RRw(ref B, ref C),   ()=> INC_Rw(ref C),            ()=> DEC_Rw(ref C),        ()=> LD_Rw_n(ref C),     ()=> RRCA(),
            /* [1] */   ()=> STOP(),                  ()=> LD_RRw_nn(ref D, ref E),   ()=> LD_RRn_A(D, E),          ()=> INC_RRw(ref D, ref E),   ()=> INC_Rw(ref D),             ()=> DEC_Rw(ref D),        ()=> LD_Rw_n(ref D),     ()=> RLA(),                ()=> JR_en(),                ()=> ADD_HL_RRr(D, E),               ()=> LD_A_RRn(D, E),         ()=> DEC_RRw(ref D, ref E),   ()=> INC_Rw(ref E),            ()=> DEC_Rw(ref E),        ()=> LD_Rw_n(ref E),     ()=> RRA(),
            /* [2] */   ()=> JR_CC_en(!FlagZ),        ()=> LD_RRw_nn(ref H, ref L),   ()=> LD_HLnp_A(),             ()=> INC_RRw(ref H, ref L),   ()=> INC_Rw(ref H),             ()=> DEC_Rw(ref H),        ()=> LD_Rw_n(ref H),     ()=> DAA(),                ()=> JR_CC_en(FlagZ),        ()=> ADD_HL_RRr(H, L),               ()=> LD_A_HLnp(),            ()=> DEC_RRw(ref H, ref L),   ()=> INC_Rw(ref L),            ()=> DEC_Rw(ref L),        ()=> LD_Rw_n(ref L),     ()=> CPL(),
            /* [3] */   ()=> JR_CC_en(!FlagC),        ()=> LD_RRw_nn(ref SP),         ()=> LD_HLnm_A(),             ()=> INC_RRw(ref SP),         ()=> INC_HLn(),                 ()=> DEC_HLn(),            ()=> LD_HLn_n(),         ()=> SCF(),                ()=> JR_CC_en(FlagC),        ()=> ADD_HL_SP(),                    ()=> LD_A_HLnm(),            ()=> DEC_RRw(ref SP),         ()=> INC_Rw(ref A),            ()=> DEC_Rw(ref A),        ()=> LD_Rw_n(ref A),     ()=> CCF(),
            /* [4] */   ()=> LD_Rw_Rr(ref B, B),      ()=> LD_Rw_Rr(ref B, C),        ()=> LD_Rw_Rr(ref B, D),      ()=> LD_Rw_Rr(ref B, E),      ()=> LD_Rw_Rr(ref B, H),        ()=> LD_Rw_Rr(ref B, L),   ()=> LD_Rw_HLn(ref B),   ()=> LD_Rw_Rr(ref B, A),   ()=> LD_Rw_Rr(ref C, B),     ()=> LD_Rw_Rr(ref C, C),             ()=> LD_Rw_Rr(ref C, D),     ()=> LD_Rw_Rr(ref C, E),      ()=> LD_Rw_Rr(ref C, H),       ()=> LD_Rw_Rr(ref C, L),   ()=> LD_Rw_HLn(ref C),   ()=> LD_Rw_Rr(ref C, A),
            /* [5] */   ()=> LD_Rw_Rr(ref D, B),      ()=> LD_Rw_Rr(ref D, C),        ()=> LD_Rw_Rr(ref D, D),      ()=> LD_Rw_Rr(ref D, E),      ()=> LD_Rw_Rr(ref D, H),        ()=> LD_Rw_Rr(ref D, L),   ()=> LD_Rw_HLn(ref D),   ()=> LD_Rw_Rr(ref D, A),   ()=> LD_Rw_Rr(ref E, B),     ()=> LD_Rw_Rr(ref E, C),             ()=> LD_Rw_Rr(ref E, D),     ()=> LD_Rw_Rr(ref E, E),      ()=> LD_Rw_Rr(ref E, H),       ()=> LD_Rw_Rr(ref E, L),   ()=> LD_Rw_HLn(ref E),   ()=> LD_Rw_Rr(ref E, A),
            /* [6] */   ()=> LD_Rw_Rr(ref H, B),      ()=> LD_Rw_Rr(ref H, C),        ()=> LD_Rw_Rr(ref H, D),      ()=> LD_Rw_Rr(ref H, E),      ()=> LD_Rw_Rr(ref H, H),        ()=> LD_Rw_Rr(ref H, L),   ()=> LD_Rw_HLn(ref H),   ()=> LD_Rw_Rr(ref H, A),   ()=> LD_Rw_Rr(ref L, B),     ()=> LD_Rw_Rr(ref L, C),             ()=> LD_Rw_Rr(ref L, D),     ()=> LD_Rw_Rr(ref L, E),      ()=> LD_Rw_Rr(ref L, H),       ()=> LD_Rw_Rr(ref L, L),   ()=> LD_Rw_HLn(ref L),   ()=> LD_Rw_Rr(ref L, A),
            /* [7] */   ()=> LD_HLn_Rr(B),            ()=> LD_HLn_Rr(C),              ()=> LD_HLn_Rr(D),            ()=> LD_HLn_Rr(E),            ()=> LD_HLn_Rr(H),              ()=> LD_HLn_Rr(L),         ()=> HALT(),             ()=> LD_HLn_Rr(A),         ()=> LD_Rw_Rr(ref A, B),     ()=> LD_Rw_Rr(ref A, C),             ()=> LD_Rw_Rr(ref A, D),     ()=> LD_Rw_Rr(ref A, E),      ()=> LD_Rw_Rr(ref A, H),       ()=> LD_Rw_Rr(ref A, L),   ()=> LD_Rw_HLn(ref A),   ()=> LD_Rw_Rr(ref A, A),
            /* [8] */   ()=> ADD_Rr(B),               ()=> ADD_Rr(C),                 ()=> ADD_Rr(D),               ()=> ADD_Rr(E),               ()=> ADD_Rr(H),                 ()=> ADD_Rr(L),            ()=> ADD_HLn(),          ()=> ADD_Rr(A),            ()=> ADC_Rr(B),              ()=> ADC_Rr(C),                      ()=> ADC_Rr(D),              ()=> ADC_Rr(E),               ()=> ADC_Rr(H),                ()=> ADC_Rr(L),            ()=> ADC_HLn(),          ()=> ADC_Rr(A),
            /* [9] */   ()=> SUB_Rr(B),               ()=> SUB_Rr(C),                 ()=> SUB_Rr(D),               ()=> SUB_Rr(E),               ()=> SUB_Rr(H),                 ()=> SUB_Rr(L),            ()=> SUB_HLn(),          ()=> SUB_Rr(A),            ()=> SBC_Rr(B),              ()=> SBC_Rr(C),                      ()=> SBC_Rr(D),              ()=> SBC_Rr(E),               ()=> SBC_Rr(H),                ()=> SBC_Rr(L),            ()=> SBC_HLn(),          ()=> SBC_Rr(A),
            /* [A] */   ()=> AND_Rr(B),               ()=> AND_Rr(C),                 ()=> AND_Rr(D),               ()=> AND_Rr(E),               ()=> AND_Rr(H),                 ()=> AND_Rr(L),            ()=> AND_HLn(),          ()=> AND_Rr(A),            ()=> XOR_Rr(B),              ()=> XOR_Rr(C),                      ()=> XOR_Rr(D),              ()=> XOR_Rr(E),               ()=> XOR_Rr(H),                ()=> XOR_Rr(L),            ()=> XOR_HLn(),          ()=> XOR_Rr(A),
            /* [B] */   ()=> OR_Rr(B),                ()=> OR_Rr(C),                  ()=> OR_Rr(D),                ()=> OR_Rr(E),                ()=> OR_Rr(H),                  ()=> OR_Rr(L),             ()=> OR_HLn(),           ()=> OR_Rr(A),             ()=> CP_Rr(B),               ()=> CP_Rr(C),                       ()=> CP_Rr(D),               ()=> CP_Rr(E),                ()=> CP_Rr(H),                 ()=> CP_Rr(L),             ()=> CP_HLn(),           ()=> CP_Rr(A),
            /* [C] */   ()=> RET_CC(!FlagZ),          ()=> POP_RR(ref B, ref C),      ()=> JP_CC_nn(!FlagZ),        ()=> JP_nn(),                 ()=> CALL_CC_nn(!FlagZ),        ()=> PUSH_RR(B, C),        ()=> ADD_n(),            ()=> RST_n(0x00),          ()=> RET_CC(FlagZ),          ()=> RET(),                          ()=> JP_CC_nn(FlagZ),        ()=> CB_op(),                 ()=> CALL_CC_nn(FlagZ),        ()=> CALL_nn(),            ()=> ADC_n(),            ()=> RST_n(0x08),
            /* [D] */   ()=> RET_CC(!FlagC),          ()=> POP_RR(ref D, ref E),      ()=> JP_CC_nn(!FlagC),        ()=> unknown(),               ()=> CALL_CC_nn(!FlagC),        ()=> PUSH_RR(D, E),        ()=> SUB_n(),            ()=> RST_n(0x10),          ()=> RET_CC(FlagC),          ()=> RETI(),                         ()=> JP_CC_nn(FlagC),        ()=> unknown(),               ()=> CALL_CC_nn(FlagC),        ()=> unknown(),            ()=> SBC_n(),            ()=> RST_n(0x18),
            /* [E] */   ()=> LDH_Nn_A(),              ()=> POP_RR(ref H, ref L),      ()=> LDH_Cn_A(),              ()=> unknown(),               ()=> unknown(),                 ()=> PUSH_RR(H, L),        ()=> AND_n(),            ()=> RST_n(0x20),          ()=> ADD_SP_en(),            ()=> JP_HL(),                        ()=> LD_nn_A(),              ()=> unknown(),               ()=> unknown(),                ()=> unknown(),            ()=> XOR_n(),            ()=> RST_n(0x28),
            /* [F] */   ()=> LDH_A_Nn(),              ()=> POP_AF(),                  ()=> LDH_A_Cn(),              ()=> DI(),                    ()=> unknown(),                 ()=> PUSH_AF(),            ()=> OR_n(),             ()=> RST_n(0x30),          ()=> LD_HL_SP_en(),          ()=> LD_SP_HL(),                     ()=> LD_A_nn(),              ()=> EI(),                    ()=> unknown(),                ()=> unknown(),            ()=> CP_n(),             ()=> RST_n(0x38)
        };

        // CB Instructions
        CB_Instructions = new InstructionDelegate[]
        {
            /*          [0]                       [1]                       [2]                       [3]                       [4]                       [5]                       [6]                  [7]                       [8]                       [9]                       [A]                       [B]                       [C]                       [D]                       [E]                    [F]             */
            /* [0] */   ()=> RLC_Rw(ref B),       ()=> RLC_Rw(ref C),       ()=> RLC_Rw(ref D),       ()=> RLC_Rw(ref E),       ()=> RLC_Rw(ref H),       ()=> RLC_Rw(ref L),       ()=> RLC_HLn(),      ()=> RLC_Rw(ref A),       ()=> RRC_Rw(ref B),       ()=> RRC_Rw(ref C),       ()=> RRC_Rw(ref D),       ()=> RRC_Rw(ref E),       ()=> RRC_Rw(ref H),       ()=> RRC_Rw(ref L),       ()=> RRC_HLn(),        ()=> RRC_Rw(ref A),
            /* [1] */   ()=> RL_Rw(ref B),        ()=> RL_Rw(ref C),        ()=> RL_Rw(ref D),        ()=> RL_Rw(ref E),        ()=> RL_Rw(ref H),        ()=> RL_Rw(ref L),        ()=> RL_HLn(),       ()=> RL_Rw(ref A),        ()=> RR_Rw(ref B),        ()=> RR_Rw(ref C),        ()=> RR_Rw(ref D),        ()=> RR_Rw(ref E),        ()=> RR_Rw(ref H),        ()=> RR_Rw(ref L),        ()=> RR_HLn(),         ()=> RR_Rw(ref A),
            /* [2] */   ()=> SLA_Rw(ref B),       ()=> SLA_Rw(ref C),       ()=> SLA_Rw(ref D),       ()=> SLA_Rw(ref E),       ()=> SLA_Rw(ref H),       ()=> SLA_Rw(ref L),       ()=> SLA_HLn(),      ()=> SLA_Rw(ref A),       ()=> SRA_Rw(ref B),       ()=> SRA_Rw(ref C),       ()=> SRA_Rw(ref D),       ()=> SRA_Rw(ref E),       ()=> SRA_Rw(ref H),       ()=> SRA_Rw(ref L),       ()=> SRA_HLn(),        ()=> SRA_Rw(ref A),
            /* [3] */   ()=> SWAP_Rw(ref B),      ()=> SWAP_Rw(ref C),      ()=> SWAP_Rw(ref D),      ()=> SWAP_Rw(ref E),      ()=> SWAP_Rw(ref H),      ()=> SWAP_Rw(ref L),      ()=> SWAP_HLn(),     ()=> SWAP_Rw(ref A),      ()=> SRL_Rw(ref B),       ()=> SRL_Rw(ref C),       ()=> SRL_Rw(ref D),       ()=> SRL_Rw(ref E),       ()=> SRL_Rw(ref H),       ()=> SRL_Rw(ref L),       ()=> SRL_HLn(),        ()=> SRL_Rw(ref A),
            /* [4] */   ()=> BIT_n_r(0, B),       ()=> BIT_n_r(0, C),       ()=> BIT_n_r(0, D),       ()=> BIT_n_r(0, E),       ()=> BIT_n_r(0, H),       ()=> BIT_n_r(0, L),       ()=> BIT_n_HLn(0),   ()=> BIT_n_r(0, A),       ()=> BIT_n_r(1, B),       ()=> BIT_n_r(1, C),       ()=> BIT_n_r(1, D),       ()=> BIT_n_r(1, E),       ()=> BIT_n_r(1, H),       ()=> BIT_n_r(1, L),       ()=> BIT_n_HLn(1),     ()=> BIT_n_r(1, A),
            /* [5] */   ()=> BIT_n_r(2, B),       ()=> BIT_n_r(2, C),       ()=> BIT_n_r(2, D),       ()=> BIT_n_r(2, E),       ()=> BIT_n_r(2, H),       ()=> BIT_n_r(2, L),       ()=> BIT_n_HLn(2),   ()=> BIT_n_r(2, A),       ()=> BIT_n_r(3, B),       ()=> BIT_n_r(3, C),       ()=> BIT_n_r(3, D),       ()=> BIT_n_r(3, E),       ()=> BIT_n_r(3, H),       ()=> BIT_n_r(3, L),       ()=> BIT_n_HLn(3),     ()=> BIT_n_r(3, A),
            /* [6] */   ()=> BIT_n_r(4, B),       ()=> BIT_n_r(4, C),       ()=> BIT_n_r(4, D),       ()=> BIT_n_r(4, E),       ()=> BIT_n_r(4, H),       ()=> BIT_n_r(4, L),       ()=> BIT_n_HLn(4),   ()=> BIT_n_r(4, A),       ()=> BIT_n_r(5, B),       ()=> BIT_n_r(5, C),       ()=> BIT_n_r(5, D),       ()=> BIT_n_r(5, E),       ()=> BIT_n_r(5, H),       ()=> BIT_n_r(5, L),       ()=> BIT_n_HLn(5),     ()=> BIT_n_r(5, A),
            /* [7] */   ()=> BIT_n_r(6, B),       ()=> BIT_n_r(6, C),       ()=> BIT_n_r(6, D),       ()=> BIT_n_r(6, E),       ()=> BIT_n_r(6, H),       ()=> BIT_n_r(6, L),       ()=> BIT_n_HLn(6),   ()=> BIT_n_r(6, A),       ()=> BIT_n_r(7, B),       ()=> BIT_n_r(7, C),       ()=> BIT_n_r(7, D),       ()=> BIT_n_r(7, E),       ()=> BIT_n_r(7, H),       ()=> BIT_n_r(7, L),       ()=> BIT_n_HLn(7),     ()=> BIT_n_r(7, A),
            /* [8] */   ()=> RES_n_r(0, ref B),   ()=> RES_n_r(0, ref C),   ()=> RES_n_r(0, ref D),   ()=> RES_n_r(0, ref E),   ()=> RES_n_r(0, ref H),   ()=> RES_n_r(0, ref L),   ()=> RES_n_HLn(0),   ()=> RES_n_r(0, ref A),   ()=> RES_n_r(1, ref B),   ()=> RES_n_r(1, ref C),   ()=> RES_n_r(1, ref D),   ()=> RES_n_r(1, ref E),   ()=> RES_n_r(1, ref H),   ()=> RES_n_r(1, ref L),   ()=> RES_n_HLn(1),     ()=> RES_n_r(1, ref A),
            /* [9] */   ()=> RES_n_r(2, ref B),   ()=> RES_n_r(2, ref C),   ()=> RES_n_r(2, ref D),   ()=> RES_n_r(2, ref E),   ()=> RES_n_r(2, ref H),   ()=> RES_n_r(2, ref L),   ()=> RES_n_HLn(2),   ()=> RES_n_r(2, ref A),   ()=> RES_n_r(3, ref B),   ()=> RES_n_r(3, ref C),   ()=> RES_n_r(3, ref D),   ()=> RES_n_r(3, ref E),   ()=> RES_n_r(3, ref H),   ()=> RES_n_r(3, ref L),   ()=> RES_n_HLn(3),     ()=> RES_n_r(3, ref A),
            /* [A] */   ()=> RES_n_r(4, ref B),   ()=> RES_n_r(4, ref C),   ()=> RES_n_r(4, ref D),   ()=> RES_n_r(4, ref E),   ()=> RES_n_r(4, ref H),   ()=> RES_n_r(4, ref L),   ()=> RES_n_HLn(4),   ()=> RES_n_r(4, ref A),   ()=> RES_n_r(5, ref B),   ()=> RES_n_r(5, ref C),   ()=> RES_n_r(5, ref D),   ()=> RES_n_r(5, ref E),   ()=> RES_n_r(5, ref H),   ()=> RES_n_r(5, ref L),   ()=> RES_n_HLn(5),     ()=> RES_n_r(5, ref A),
            /* [B] */   ()=> RES_n_r(6, ref B),   ()=> RES_n_r(6, ref C),   ()=> RES_n_r(6, ref D),   ()=> RES_n_r(6, ref E),   ()=> RES_n_r(6, ref H),   ()=> RES_n_r(6, ref L),   ()=> RES_n_HLn(6),   ()=> RES_n_r(6, ref A),   ()=> RES_n_r(7, ref B),   ()=> RES_n_r(7, ref C),   ()=> RES_n_r(7, ref D),   ()=> RES_n_r(7, ref E),   ()=> RES_n_r(7, ref H),   ()=> RES_n_r(7, ref L),   ()=> RES_n_HLn(7),     ()=> RES_n_r(7, ref A),
            /* [4] */   ()=> SET_n_r(0, ref B),   ()=> SET_n_r(0, ref C),   ()=> SET_n_r(0, ref D),   ()=> SET_n_r(0, ref E),   ()=> SET_n_r(0, ref H),   ()=> SET_n_r(0, ref L),   ()=> SET_n_HLn(0),   ()=> SET_n_r(0, ref A),   ()=> SET_n_r(1, ref B),   ()=> SET_n_r(1, ref C),   ()=> SET_n_r(1, ref D),   ()=> SET_n_r(1, ref E),   ()=> SET_n_r(1, ref H),   ()=> SET_n_r(1, ref L),   ()=> SET_n_HLn(1),     ()=> SET_n_r(1, ref A),
            /* [5] */   ()=> SET_n_r(2, ref B),   ()=> SET_n_r(2, ref C),   ()=> SET_n_r(2, ref D),   ()=> SET_n_r(2, ref E),   ()=> SET_n_r(2, ref H),   ()=> SET_n_r(2, ref L),   ()=> SET_n_HLn(2),   ()=> SET_n_r(2, ref A),   ()=> SET_n_r(3, ref B),   ()=> SET_n_r(3, ref C),   ()=> SET_n_r(3, ref D),   ()=> SET_n_r(3, ref E),   ()=> SET_n_r(3, ref H),   ()=> SET_n_r(3, ref L),   ()=> SET_n_HLn(3),     ()=> SET_n_r(3, ref A),
            /* [6] */   ()=> SET_n_r(4, ref B),   ()=> SET_n_r(4, ref C),   ()=> SET_n_r(4, ref D),   ()=> SET_n_r(4, ref E),   ()=> SET_n_r(4, ref H),   ()=> SET_n_r(4, ref L),   ()=> SET_n_HLn(4),   ()=> SET_n_r(4, ref A),   ()=> SET_n_r(5, ref B),   ()=> SET_n_r(5, ref C),   ()=> SET_n_r(5, ref D),   ()=> SET_n_r(5, ref E),   ()=> SET_n_r(5, ref H),   ()=> SET_n_r(5, ref L),   ()=> SET_n_HLn(5),     ()=> SET_n_r(5, ref A),
            /* [7] */   ()=> SET_n_r(6, ref B),   ()=> SET_n_r(6, ref C),   ()=> SET_n_r(6, ref D),   ()=> SET_n_r(6, ref E),   ()=> SET_n_r(6, ref H),   ()=> SET_n_r(6, ref L),   ()=> SET_n_HLn(6),   ()=> SET_n_r(6, ref A),   ()=> SET_n_r(7, ref B),   ()=> SET_n_r(7, ref C),   ()=> SET_n_r(7, ref D),   ()=> SET_n_r(7, ref E),   ()=> SET_n_r(7, ref H),   ()=> SET_n_r(7, ref L),   ()=> SET_n_HLn(7),     ()=> SET_n_r(7, ref A),
        };
    }

    // Enable CPU
    // ----------
    private void CPUWakeUp()
    {
        Halt = false;
        Stop = false;
        IME = false;
        IME_scheduled = false;
    }

    // Timer set
    // ---------
    private void Timer(int LastCycles)
    {
        if (IO != null)
        {
            // DIV
            DivCycles += (ushort)(Cycles - LastCycles);

            if (Cycles == 0 || Stop)
            {
                IO.DIV = 0;
                DivCycles = 0;
            }
            else if (DivCycles >= 256)
            {
                IO.DIV++;
                DivCycles -= 256;
            }

            // TIMA and TMA
            TacCycles += Cycles - LastCycles;
            byte ClockSelect = (byte)(IO.TAC & 3);

            if (ReadBit(IO.TAC, 2))
            {
                if(TacCycles > ClockCPU[ClockSelect])
                {
                    ushort NewTima = (ushort)(IO.TIMA + 1);

                    if (NewTima > 0xFF)
                    {
                        NewTima = 0;
                        IO.TMA = 0;
                        IME_scheduled = true;
                    }

                    IO.TIMA = (byte)NewTima;

                    if (IO.TMA == 0xFF || (IO.TIMA % (0xFF - IO.TMA) == 0))
                    IME_scheduled = true;

                    TacCycles -= ClockCPU[ClockSelect];
                }
            }
        }
    }

    // ####################
    // # Instructions set #
    // ####################

    // LD r, r’: Load register (register)
    // ----------------------------------
    // 8-bit load instructions transfer one byte of data between two 8-bit registers,
    // or between one 8-bit register and location in memory.
    private void LD_Rw_Rr(ref byte Rw, byte Rr)
    {
        Cycles++;
        Rw = Rr;
    }

    // LD r, n: Load register (immediate)
    // ----------------------------------
    // Load to the 8-bit register r, the immediate data n.
    private void LD_Rw_n(ref byte Rw)
    {
        Cycles += 2;
        Rw = Read(PC++);
    }

    // LD r, (HL): Load register (indirect HL)
    // ---------------------------------------
    // Load to the 8-bit register r, data from the absolute address specified by the
    // 16-bit register HL
    private void LD_Rw_HLn(ref byte Rw)
    {
        Cycles += 2;
        Rw = Read(u16(L, H));
    }

    //LD (HL), r: Load from register (indirect HL)
    //--------------------------------------------
    //Load to the absolute address specified by the 16-bit register HL, data from
    //the 8-bit register r.
    private void LD_HLn_Rr(byte Rr)
    {
        Cycles += 2;
        Write(u16(L, H), Rr);
    }

    // LD (HL), n: Load from immediate data (indirect HL)
    // --------------------------------------------------
    // Load to the absolute address specified by the 16-bit register HL,
    // the immediate data n.
    private void LD_HLn_n()
    {
        Cycles += 3;
        Write(u16(L, H), Read(PC++));
    }

    // LD A, (RR): Load accumulator (indirect BC)
    // ------------------------------------------
    // Load to the 8-bit A register, data from the absolute address specified by
    // the 16-bit register BC.
    private void LD_A_RRn(byte Rmr, byte Rlr)
    {
        Cycles += 2;
        A = Read(u16(Rlr, Rmr));
    }

    // LD (RR), A: Load from accumulator (indirect BC)
    // -----------------------------------------------
    // Load to the absolute address specified by the 16-bit register BC, data
    // from the 8-bit A register
    private void LD_RRn_A(byte Rmr, byte Rlr)
    {
        Cycles += 2;
        Write(u16(Rlr, Rmr), A);
    }

    // LD A, (nn): Load accumulator (direct)
    // -------------------------------------
    // Load to the 8-bit A register, data from the absolute address
    // specified by the 16-bit operand nn.
    private void LD_A_nn()
    {
        Cycles += 4;
        A = Read(u16(Read(PC++), Read(PC++)));
    }

    // LD (nn), A: Load from accumulator (direct)
    // ------------------------------------------
    // Load to the absolute address specified by the 16-bit operand nn, data
    // from the 8-bit A register.
    private void LD_nn_A()
    {
        Cycles += 4;
        Write(u16(Read(PC++), Read(PC++)), A);
    }

    // LDH A, (C): Load accumulator (indirect 0xFF00+C)
    // ------------------------------------------------
    // Load to the 8-bit A register, data from the address specified by the 8-bit C
    // register. The full 16-bit absolute address is obtained by setting the most
    // significant byte to 0xFF and the least significant byte to the value of C,
    // so the possible range is 0xFF00-0xFFFF.
    private void LDH_A_Cn()
    {
        Cycles += 2;
        A = Read(u16(C, 0xFF));
    }

    // LDH (C), A: Load from accumulator (indirect 0xFF00+C)
    // -----------------------------------------------------
    // Load to the address specified by the 8-bit C register, data from the 8-bit
    // A register. The full 16-bit absolute address is obtained by setting the most
    // significant byte to 0xFF and the least significant byte to the value of C,
    // so the possible range is 0xFF00-0xFFFF.
    private void LDH_Cn_A()
    {
        Cycles += 2;
        Write(u16(C, 0xFF), A);
    }

    // LDH A, (n): Load accumulator (direct 0xFF00+n)
    // ----------------------------------------------
    // Load to the 8-bit A register, data from the address specified by the 8-bit
    // immediate data n. The full 16-bit absolute address is obtained by setting
    // the most significant byte to 0xFF and the least significant byte to the
    // value of n, so the possible range is 0xFF00-0xFFFF.
    private void LDH_A_Nn()
    {
        Cycles += 3;
        A = Read(u16(Read(PC++), 0xFF));
    }

    // LDH (n), A: Load from accumulator (direct 0xFF00+n)
    // ----------------------------------------------------
    // Load to the address specified by the 8-bit immediate data n, data from the
    // 8-bit A register. The full 16-bit absolute address is obtained by
    // the setting the most significant byte to 0xFF and the least significant byte
    // to value of n, so the possible range is 0xFF00-0xFFFF.
    private void LDH_Nn_A()
    {
        Cycles += 3;
        Write(u16(Read(PC++), 0xFF), A);
    }

    // LD A, (HL-): Load accumulator (indirect HL, decrement)
    // ------------------------------------------------------
    // Load to the 8-bit A register, data from the absolute address specified by the
    // 16-bit register HL. The value of HL is decremented after the memory read.
    private void LD_A_HLnm()
    {
        Cycles += 2;

        ushort HL = u16(L, H);

        A = Read(HL--);
        H = Msb(HL);
        L = Lsb(HL);
    }

    // LD (HL-), A: Load from accumulator (indirect HL, decrement)
    // ------------------------------------------------------------
    // Load to the absolute address specified by the 16-bit register HL, data from
    // the 8-bit A register. The value ofHL is decremented after the memory write.
    private void LD_HLnm_A()
    {
        Cycles += 2;

        ushort HL = u16(L, H);

        Write(HL--, A);
        H = Msb(HL);
        L = Lsb(HL);
    }

    // LD A, (HL+): Load accumulator (indirect HL, increment)
    // --------------------------------------------------------
    // Load to the 8-bit A register, data from the absolute address specified by
    // the 16-bit register HL. The value of HL is incremented after the memory read.
    private void LD_A_HLnp()
    {
        Cycles += 2;

        ushort HL = u16(L, H);

        A = Read(HL++);
        H = Msb(HL);
        L = Lsb(HL);
    }

    // LD (HL+), A: Load from accumulator (indirect HL, increment)
    // -----------------------------------------------------------
    // Load to the absolute address specified by the 16-bit register HL, data from
    // the 8-bit A register. The value of HL is incremented after the memory write.
    private void LD_HLnp_A()
    {
        Cycles += 2;

        ushort HL = u16(L, H);

        Write(HL++, A);
        H = Msb(HL);
        L = Lsb(HL);
    }

    // LD rr, nn: Load 16-bit register / register pair
    // ------------------------------------------------
    // Load to the 16-bit register rr, the immediate 16-bit data nn.
    private void LD_RRw_nn(ref byte Rmw, ref byte Rlw)
    {
        Cycles += 3;

        Rlw = Read(PC++);
        Rmw = Read(PC++);
    }
    private void LD_RRw_nn(ref ushort RRw)
    {
        Cycles += 3;

        RRw = u16(Read(PC++), Read(PC++));
    }

    // LD (nn), SP: Load from stack pointer (direct)
    // ---------------------------------------------
    // Load to the absolute address specified by the 16-bit operand nn, data from the 16-bit SP register.
    private void LD_Nnn_SP()
    {
        Cycles += 5;

        ushort nn = u16(Read(PC++), Read(PC++));


        Write(nn, Lsb(SP));
        Write(++nn, Msb(SP));
    }

    // LD SP, HL: Load stack pointer from HL
    // -------------------------------------
    // Load to the 16-bit SP register, data from the 16-bit HL register.
    private void LD_SP_HL()
    {
        Cycles += 2;
        SP = u16(L, H);
    }

    // LD HL, SP e
    // -----------
    // Add the signed value e8 to SP and store the result in HL.
    private void LD_HL_SP_en()
    {
        Cycles += 3;
        sbyte e = unchecked((sbyte)Read(PC++));

        ushort _SP = (ushort)(SP + e);

        H = Msb(_SP);
        L = Lsb(_SP);

        FlagZ = false;
        FlagN = false;
        FlagH = (_SP & 0x08) != 0;
        FlagC = (_SP & 0x80) != 0;
    }

    // PUSH rr: Push to stack
    // ----------------------
    // Push to the stack memory, data from the 16-bit register rr.
    private void PUSH_RR(byte Rmr, byte Rlr)
    {
        Cycles += 4;

        Write(--SP, Rmr);
        Write(--SP, Rlr);
    }
    private void PUSH_AF()
    {
        Cycles += 4;

        byte F = 0;

        if (FlagZ) SetBit(ref F, 7, true);
        if (FlagN) SetBit(ref F, 6, true);
        if (FlagH) SetBit(ref F, 5, true);
        if (FlagC) SetBit(ref F, 4, true);

        Write(--SP, F);
        Write(--SP, A);
    }

    // POP rr: Pop from stack
    // ----------------------
    // Pops to the 16-bit register rr, data from the stack memory.This instruction
    // does not do calculations that affect flags, but POP AF completely replaces the
    // F register value, so all flags are changed based on the 8-bit data that is read
    // from memory.
    private void POP_RR(ref byte Rmw, ref byte Rlw)
    {
        Cycles += 3;

        Rlw = Read(SP++);
        Rmw = Read(SP++);
    }
    private void POP_AF()
    {
        Cycles += 3;

        byte F = Read(SP++);

        FlagZ = ReadBit(F, 7);
        FlagN = ReadBit(F, 6);
        FlagH = ReadBit(F, 5);
        FlagC = ReadBit(F, 4);

        A = Read(SP++);
    }

    // ADD r: Add (register)
    // ---------------------
    // Adds to the 8-bit A register, the 8-bit register r, and stores the result
    // back into the A register.
    private void ADD_Rr(byte Rr)
    {
        Cycles += 1;

        A += Rr;

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // ADD (HL): Add (indirect HL)
    // ---------------------------
    // Adds to the 8-bit A register, data from the absolute address specified by the
    // 16-bit register HL, and stores the result back into the A register.
    private void ADD_HLn()
    {
        Cycles += 2;

        A += Read(u16(L, H));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // ADD n: Add (immediate)
    // ----------------------
    // Adds to the 8-bit A register, the immediate data n, and stores the result
    // back into the A register
    private void ADD_n()
    {
        Cycles += 2;

        A += Read(PC++);

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // ADD SP e
    // --------
    // Add the signed value e8 to SP.
    private void ADD_SP_en()
    {
        Cycles += 4;

        sbyte e = unchecked((sbyte)Read(PC++));
        int result = SP + e;

        SP = (ushort)result;

        FlagZ = false;
        FlagN = false;
        FlagH = (SP & 0x08) != 0;
        FlagC = (SP & 0x80) != 0;
    }

    // ADD HL rr
    // ---------
    // Add the value in r16 to HL.
    private void ADD_HL_RRr(byte Rmr, byte Rlr)
    {
        Cycles += 2;
        ushort HL = (ushort)(u16(L, H) + u16(Rlr, Rmr));

        H = Msb(HL);
        L = Lsb(HL);

        FlagN = false;
        FlagH = ((HL & 0xFFF) + (u16(Rlr, Rmr) & 0xFFF)) > 0xFFF;
        FlagC = (HL & 0x10000) != 0;
    }
    private void ADD_HL_SP()
    {
        Cycles += 2;
        ushort HL = (ushort)(u16(L, H) + SP);

        H = Msb(HL);
        L = Lsb(HL);

        FlagN = false;
        FlagH = ((HL & 0xFFF) + (SP & 0xFFF)) > 0xFFF;
        FlagC = (HL & 0x10000) != 0;
    }

    // ADC r: Add with carry (register)
    // --------------------------------
    // Adds to the 8-bit A register, the carry flag and the 8-bit register r, and
    // stores the result back into the A register.
    private void ADC_Rr(byte Rr)
    {
        Cycles += 1;

        A += (byte)((FlagC ? 1 : 0) + Rr);

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // ADC (HL): Add with carry (indirect HL)
    // --------------------------------------
    // Adds to the 8-bit A register, the carry flag and data from the absolute address 
    // specified by the 16-bit register HL, and stores the result back into the A register

    private void ADC_HLn()
    {
        Cycles += 2;

        A += (byte)((FlagC ? 1 : 0) + Read(u16(L, H)));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // ADC n: Add with carry (immediate)
    // ---------------------------------
    // Adds to the 8-bit A register, the carry flag and the immediate data n, and
    // stores the result back into the A register.
    private void ADC_n()
    {
        Cycles += 2;

        A += (byte)((FlagC ? 1 : 0) + Read(PC++));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = (A & 0x08) != 0;
        FlagC = (A & 0x80) != 0;
    }

    // SUB r: Subtract (register)
    // --------------------------
    // Subtracts from the 8-bit A register, the 8-bit register r, and stores the
    // result back into the A register.
    private void SUB_Rr(byte Rr)
    {
        Cycles += 1;

        ushort result = (ushort)(A - Rr);
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = Rr > A;
    }

    // SUB (HL): Subtract (indirect HL)
    // --------------------------------
    // Subtracts from the 8-bit A register, data from the absolute address specified
    // by the 16-bit register HL, and stores the result back into the A register.
    private void SUB_HLn()
    {
        Cycles += 2;

        byte HL = Read(u16(L, H));
        ushort result = (ushort)(A - HL);
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = HL > A;
    }

    // SUB n: Subtract (immediate)
    // ---------------------------
    // Subtracts from the 8-bit A register, the immediate data n, and stores the result
    // back into the A register.
    private void SUB_n()
    {
        Cycles += 2;

        byte n = Read(PC++);
        ushort result = (ushort)(A - n);
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = n > A;
    }

    // SBC r: Subtract with carry (register)
    // -------------------------------------
    // Subtracts from the 8-bit A register, the carry flag and the 8-bit register r, and stores
    // the result back into the A register.
    private void SBC_Rr(byte Rr)
    {
        Cycles += 1;

        ushort result = (ushort)(A - (Rr + (FlagC ? 1 : 0)));
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = Rr + (FlagC ? 1 : 0) > A;
    }

    // SBC (HL): Subtract with carry (indirect HL)
    // -------------------------------------------
    // Subtracts from the 8-bit A register, the carry flag and data from the absolute address
    // specified by the 16-bit register HL, and stores the result back into the A register.
    private void SBC_HLn()
    {
        Cycles += 2;

        byte HL = Read(u16(L, H));
        ushort result = (ushort)(A - (HL + (FlagC ? 1 : 0)));
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = HL + (FlagC ? 1 : 0) > A;
    }

    // SBC n: Subtract with carry (immediate)
    // --------------------------------------
    // Subtracts from the 8-bit A register, the carry flag and the immediate data n, and stores
    // the result back into the A register.
    private void SBC_n()
    {
        Cycles += 2;

        byte n = Read(PC++);
        ushort result = (ushort)(A - (n + (FlagC ? 1 : 0)));
        bool borrowFromBit4 = (result & 0x10) != 0;

        A = (byte)result;

        FlagZ = A == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
        FlagC = n + (FlagC ? 1 : 0) > A;
    }

    // CP r: Compare (register)
    // ------------------------
    // Subtracts from the 8-bit A register, the 8-bit register r, and updates flags based on
    // the result. This instruction is basically identical to SUB r, but does not update the A register.
    private void CP_Rr(byte Rr)
    {
        Cycles += 1;

        ushort result = (ushort)(A - Rr);

        FlagZ = result == 0;
        FlagN = true;
        FlagH = (result & 0x10) != 0;
        FlagC = Rr > A;
    }

    // CP (HL): Compare (indirect HL)
    // ------------------------------
    // Subtracts from the 8-bit A register, data from the absolute address specified by the 16-bit
    // register HL, and updates flags based on the result.This instruction is basically identical to SUB
    // (HL), but does not update the A register.
    private void CP_HLn()
    {
        Cycles += 2;

        byte HL = Read(u16(L, H));
        ushort result = (ushort)(A - HL);

        FlagZ = result == 0;
        FlagN = true;
        FlagH = (result & 0x10) != 0;
        FlagC = HL > A;
    }

    // CP n: Compare (immediate)
    // -------------------------
    // Subtracts from the 8-bit A register, the immediate data n, and updates flags based on the result. This
    // instruction is basically identical to SUB n, but does not update the A register.
    private void CP_n()
    {
        Cycles += 2;

        byte n = Read(PC++);
        ushort result = (ushort)(A - n);

        FlagZ = result == 0;
        FlagN = true;
        FlagH = (result & 0x10) != 0;
        FlagC = n > A;
    }

    // INC r: Increment (register)
    // ---------------------------
    // Increments data in the 8-bit register r.
    private void INC_Rw(ref byte Rw)
    {
        Cycles += 1;

        Rw++;

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = (Rw & 0x08) != 0;
    }

    // INC (HL): Increment (indirect HL)
    // ---------------------------------
    // Increments data at the absolute address specified by the 16-bit register HL.
    private void INC_HLn()
    {
        Cycles += 3;

        byte data = (byte)(Read(u16(L, H)) + 1);
        Write(u16(L, H), data);

        FlagZ = data == 0;
        FlagN = false;
        FlagH = (data & 0x08) != 0;
    }

    // INC rr
    // ------
    // Increment value in register RR by 1.
    private void INC_RRw(ref byte Rmw, ref byte Rlw)
    {
        Cycles += 2;

        ushort RR = (ushort)(u16(Rlw, Rmw) + 1);

        Rmw = Msb(RR);
        Rlw = Lsb(RR);
    }
    private void INC_RRw(ref ushort RRw)
    {
        Cycles += 2;
        RRw++;
    }

    // DEC r: Decrement (register)
    // ---------------------------
    // Decrements data in the 8-bit register r.
    private void DEC_Rw(ref byte Rw)
    {
        Cycles += 1;

        bool borrowFromBit4 = (Rw & 0x10) != 0;
        Rw--;

        FlagZ = Rw == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
    }

    // DEC (HL): Decrement (indirect HL)
    // ---------------------------------
    // Decrements data at the absolute address specified by the 16-bit register HL.
    private void DEC_HLn()
    {
        Cycles += 3;

        byte HLn = Read(u16(L, H));
        bool borrowFromBit4 = (HLn & 0x10) != 0;

        byte data = (byte)(HLn - 1);
        Write(u16(L, H), data);

        FlagZ = data == 0;
        FlagN = true;
        FlagH = borrowFromBit4;
    }

    // DEC rr
    // ------
    // Decrement value in register rr by 1.
    private void DEC_RRw(ref byte Rmw, ref byte Rlw)
    {
        Cycles += 2;

        ushort RR = (ushort)(u16(Rlw, Rmw) - 1);

        Rmw = Msb(RR);
        Rlw = Lsb(RR);
    }
    private void DEC_RRw(ref ushort RRw)
    {
        Cycles += 2;
        RRw--;
    }

    // AND r: Bitwise AND (register)
    // -----------------------------
    // Performs a bitwise AND operation between the 8-bit A register and the 8-bit register r,
    // and stores the result back into the A register.
    private void AND_Rr(byte Rr)
    {
        Cycles += 1;

        A &= Rr;

        FlagZ = A == 0;
        FlagN = false;
        FlagH = true;
        FlagC = false;
    }

    // AND (HL): Bitwise AND (indirect HL)
    // -----------------------------------
    // Performs a bitwise AND operation between the 8-bit A register and data from the absolute
    // address specified by the 16-bit register HL, and stores the result back into the A register.
    private void AND_HLn()
    {
        Cycles += 2;

        A &= Read(u16(L, H));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = true;
        FlagC = false;
    }

    // AND n: Bitwise AND (immediate)
    // ------------------------------
    // Performs a bitwise AND operation between the 8-bit A register and immediate data n, and stores
    // the result back into the A register.
    private void AND_n()
    {
        Cycles += 2;

        A &= Read(PC++);

        FlagZ = A == 0;
        FlagN = false;
        FlagH = true;
        FlagC = false;
    }

    // OR r: Bitwise OR (register)
    // ---------------------------
    // Performs a bitwise OR operation between the 8-bit A register and the 8-bit register r, and
    // stores the result back into the A register
    private void OR_Rr(byte Rr)
    {
        Cycles += 1;

        A |= Rr;

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // OR (HL): Bitwise OR (indirect HL)
    // ---------------------------------
    // Performs a bitwise AND operation between the 8-bit A register and data from the absolute
    // address specified by the 16-bit register HL, and stores the result back into the A register.
    private void OR_HLn()
    {
        Cycles += 2;

        A |= Read(u16(L, H));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // OR n: Bitwise OR (immediate)
    // ----------------------------
    // Performs a bitwise OR operation between the 8-bit A register and immediate data n, and stores
    // the result back into the A register.
    private void OR_n()
    {
        Cycles += 2;

        A |= Read(PC++);

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // XOR r: Bitwise XOR (register)
    // -----------------------------
    // Performs a bitwise XOR operation between the 8-bit A register and the 8-bit register r, and
    // stores the result back into the A register
    private void XOR_Rr(byte Rr)
    {
        Cycles += 1;

        A ^= Rr;

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // XOR (HL): Bitwise XOR (indirect HL)
    // -----------------------------------
    // Performs a bitwise XOR operation between the 8-bit A register and data from the absolute
    // address specified by the 16-bit register HL, and stores the result back into the A register.
    private void XOR_HLn()
    {
        Cycles += 2;

        A ^= Read(u16(L, H));

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // XOR n: Bitwise XOR (immediate)
    // ------------------------------
    // Performs a bitwise XOR operation between the 8-bit A register and immediate data n, and stores
    // the result back into the A register.
    private void XOR_n()
    {
        Cycles += 2;

        A ^= Read(PC++);

        FlagZ = A == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // CCF: Complement carry flag
    // --------------------------
    // Flips the carry flag, and clears the N and H flags.
    private void CCF()
    {
        Cycles += 1;

        FlagN = false;
        FlagH = false;
        FlagC = !FlagC;
    }

    // SCF: Set carry flag
    // -------------------
    // Sets the carry flag, and clears the N and H flags.
    private void SCF()
    {
        Cycles += 1;

        FlagN = false;
        FlagH = false;
        FlagC = true;
    }

    // DAA: Decimal adjust accumulator
    // -------------------------------
    // Decimal Adjust Accumulator to get a correct BCD representation after an arithmetic instruction.
    private void DAA()
    {
        Cycles += 1;

        byte lo = (byte)(A & 0x0F);
        byte hi = (byte)(A >> 4);

        if (FlagN)
        {
            if (FlagH || (lo > 9))
            lo -= 6;
        }
        else
        {
            if ((lo > 9) || FlagH)
            lo += 6;
        }

        if (FlagC || (hi > 9))
        {
            hi += 6;
            FlagC = true;
        }
        else
        FlagC = false;

        A = (byte)((hi << 4) | (lo & 0x0F));
    }

    // CPL: Complement accumulator
    // ---------------------------
    // Flips all the bits in the 8-bit A register, and sets the N and H flags.
    private void CPL()
    {
        Cycles += 1;
        A = (byte)~A;

        FlagN = true;
        FlagH = true;
    }

    // JP nn: Jump
    // -----------
    // Unconditional jump to the absolute address specified by the 16-bit operand nn.
    private void JP_nn()
    {
        Cycles += 4;
        PC = u16(Lsb(Read(PC++)), Msb(Read(PC++)));
    }

    // JP HL: Jump to HL
    // -----------------
    // Unconditional jump to the absolute address specified by the 16-bit register HL.
    private void JP_HL()
    {
        Cycles += 1;
        PC = u16(L, H);
    }

    // JP cc, nn: Jump (conditional)
    // -----------------------------
    // Conditional jump to the absolute address specified by the 16-bit operand nn, depending on the
    // condition cc. Note that the operand(absolute address) is read even when the condition is false!
    private void JP_CC_nn(bool CC)
    {
        Cycles += CC ? 4 : 3;
        ushort nn = u16(Read(PC++), Read(PC++));

        if (CC)
        PC = nn;
    }

    // JR e: Relative jump
    // -------------------
    // Unconditional jump to the relative address specified by the signed 8-bit operand e.
    private void JR_en()
    {
        Cycles += 3;
        sbyte e = unchecked((sbyte)Read(PC++));
        PC = (ushort)(PC + e);
    }

    // JR cc, e: Relative jump (conditional)
    // -------------------------------------
    // Conditional jump to the relative address specified by the signed 8-bit operand e, depending on the condition cc.
    // Note that the operand(relative address offset) is read even when the condition is false!
    private void JR_CC_en(bool CC)
    {
        Cycles += CC ? 3 : 2;
        sbyte e = unchecked((sbyte)Read(PC++));

        if (CC)
        PC = (ushort)(PC + e);
        else
        PC++;
    }

    // CALL nn: Call function
    // ----------------------
    // Unconditional function call to the absolute address specified by the 16-bit operand nn.
    private void CALL_nn()
    {
        Cycles += 6;

        ushort nn = u16(Read(PC++), Read(PC++));

        Write(--SP, Msb(nn));
        Write(--SP, Lsb(nn));

        PC = nn;
    }

    // CALL cc, nn: Call function (conditional)
    // ----------------------------------------
    // Conditional function call to the absolute address specified by the 16-bit operand nn,
    // depending on the condition cc. Note that the operand(absolute address) is read even when the
    // condition is false!
    private void CALL_CC_nn(bool CC)
    {
        Cycles += CC ? 6 : 3;
        byte lsb = Read(PC++);
        byte msb = Read(PC++);

        if (CC)
        {
            Write(--SP, msb);
            Write(--SP, lsb);

            PC = u16(lsb, msb);
        }
    }

    // RET: Return from function
    // -------------------------
    // Unconditional return from a function.
    private void RET()
    {
        Cycles += 4;
        PC = u16(Read(SP++), Read(SP++));
    }

    // RET cc: Return from function (conditional)
    // ------------------------------------------
    // Conditional return from a function, depending on the condition cc.
    private void RET_CC(bool CC)
    {
        Cycles += CC ? 5 : 2;

        if (CC)
        PC = u16(Read(SP++), Read(SP++));
    }

    // RETI: Return from interrupt handler
    // -----------------------------------
    // Unconditional return from a function. Also enables interrupts by setting IME=1.
    private void RETI()
    {
        Cycles += 4;

        PC = u16(Read(SP++), Read(SP++));
        IME_scheduled = true;
    }

    // RST n: Restart / Call function (implied)
    // ----------------------------------------
    // Unconditional function call to the absolute fixed address defined by the opcode.
    private void RST_n(byte at)
    {
        Cycles += 4;

        byte n = Read(PC);

        Write(--SP, Msb(PC));
        Write(--SP, Lsb(PC));

        PC = u16(n, at);
    }

    // HALT: Halt system clock
    // -----------------------
    // Enter CPU low-power consumption mode until an interrupt occurs. The exact behavior of
    // this instruction depends on the state of the IME flag.
    private void HALT()
    {
        Halt = true;

        if (IME)
        {
            if (Memory.Read(0xFF0F) != 0)
            Halt = false;
            else
            {
                
            }
        }
        else
        Halt = false;
    }

    // STOP: Stop system and main clocks
    // ---------------------------------
    // -
    private void STOP()
    {
        Stop = true;
    }

    // DI: Disable interrupts
    // ----------------------
    // Disables interrupt handling by setting IME=0 and cancelling any scheduled effects of
    // the EI instruction if any.
    private void DI()
    {
        Cycles += 1;
        IME = false;
        IME_scheduled = false;
    }

    // EI: Enable interrupts
    // ---------------------
    // Schedules interrupt handling to be enabled after the next machine cycle.
    private void EI()
    {
        Cycles += 1;
        IME_scheduled = true;
    }

    // NOP: No operation
    // -----------------
    // No operation. This instruction doesn’t do anything, but can be used to add a delay of one
    // machine cycle and increment PC by one.
    private void NOP()
    {
        Cycles += 1;
    }

    // Unknown
    // -------
    // -
    private void unknown()
    {
    }

    // RLCA
    // ----
    // Rotate Left through Carry Accumulator : 000c rotate akku left
    private void RLCA()
    {
        Cycles += 1;

        byte _A = A;
        A = (byte)((A << 1) | (A >> 7));

        FlagZ = false;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_A, 7);
    }

    // RLA
    // ---
    // Rotate Left through Accumulator : 000c rotate akku left through carry
    private void RLA()
    {
        Cycles += 1;

        byte _A = A;
        A <<= 1;

        SetBit(ref A, 0, FlagC);

        FlagZ = false;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_A, 7);
    }

    // RRCA
    // ----
    // Rotate Right through Carry Accumulator : 000c rotate akku right
    private void RRCA()
    {
        Cycles += 1;

        byte _A = A;
        A = (byte)((A >> 1) | ((A & 0x01) << 7));

        FlagZ = false;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_A, 0);
    }

    // RRA
    // ----
    // Rotate Right through Accumulator : 000c rotate akku right through carry
    private void RRA()
    {
        Cycles += 1;

        byte _A = A;
        A >>= 1;

        SetBit(ref A, 7, FlagC);

        FlagZ = false;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_A, 0);
    }

    // CB operation
    // ------------
    // Execute opcode with CB prefix
    private void CB_op()
    {
        CB_Instructions[Read(++PC)]?.Invoke();
    }

    // ###############################
    // # Instructions set prefix CB  #
    // ###############################

    // RLC r
    // -----
    // Rotate Left through Carry
    private void RLC_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw = (byte)((Rw << 1) | (Rw >> 7));

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 7);
    }

    // RLC (HL)
    // --------
    // Rotate Left through Carry with direct memory at HL
    private void RLC_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)((data << 1) | (data >> 7));
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 7);
    }

    // RRC r
    // -----
    // Rotate Right through Carry
    private void RRC_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw = (byte)((Rw >> 1) | ((Rw & 0x01) << 7));

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 0);
    }

    // RRC (HL)
    // --------
    // Rotate Right through Carry with direct memory at HL
    private void RRC_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)((data >> 1) | ((data & 0x01) << 7));
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 0);
    }

    // RL r
    // ----
    // Rotate Left through
    private void RL_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw <<= 1;

        SetBit(ref Rw, 0, FlagC);

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 7);
    }

    // RL (HL)
    // -------
    // Rotate Left through with direct memory at HL
    private void RL_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)(data << 1);

        SetBit(ref newData, 0, FlagC);
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 7);
    }

    // RR r
    // -----
    // Rotate Right through
    private void RR_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw >>= 1;

        SetBit(ref Rw, 7, FlagC);

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 0);
    }

    // RR (HL)
    // -------
    // Rotate Right through with direct memory at HL
    private void RR_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)(data >> 1);

        SetBit(ref newData, 7, FlagC);
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 0);
    }

    // SLA r
    // -----
    // Shift Left Arithmetic
    private void SLA_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw = (byte)(Rw << 1);

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 7);
    }

    // SLA (HL)
    // --------
    // Shift Left Arithmetic with direct memory at HL
    private void SLA_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)(data << 1);
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 7);
    }

    // SRA r
    // -----
    // Shift Right Arithmetic
    private void SRA_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw = (byte)((Rw >> 1) | (Rw & 0x80));

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 0);
    }

    // SRA (HL)
    // --------
    // Shift Right Arithmetic with direct memory at HL
    private void SRA_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)((data >> 1) | (data & 0x80));
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 0);
    }

    // SWAP r
    // ------
    // Exchange low/hi-nibble
    private void SWAP_Rw(ref byte Rw)
    {
        Cycles += 2;

        Rw = (byte)(((Rw & 0x0F) << 4) | ((Rw & 0xF0) >> 4));

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // SWAP (HL)
    // ---------
    // Exchange low/hi-nibble with direct memory at HL
    private void SWAP_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)(((data & 0x0F) << 4) | ((data & 0xF0) >> 4));
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = false;
    }

    // SRL r
    // -----
    // Shift right logical
    private void SRL_Rw(ref byte Rw)
    {
        Cycles += 2;

        byte _R = Rw;
        Rw = (byte)(Rw >> 1);

        FlagZ = Rw == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(_R, 0);
    }

    // SRL (HL)
    // --------
    // Shift right logical with direct memory at HL
    private void SRL_HLn()
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        byte newData = (byte)(data >> 1);
        Write(HL, newData);

        FlagZ = newData == 0;
        FlagN = false;
        FlagH = false;
        FlagC = ReadBit(data, 0);
    }

    // BIT n,r
    // -------
    // Test bit n
    private void BIT_n_r(byte n, byte Rr)
    {
        Cycles += 2;

        FlagZ = ReadBit(Rr, n);
        FlagN = false;
        FlagH = true;
    }

    // BIT n,(HL)
    // ----------
    // Test bit n with direct memory at HL
    private void BIT_n_HLn(byte n)
    {
        Cycles += 3;

        FlagZ = ReadBit(Read(u16(L, H)), n);
        FlagN = false;
        FlagH = true;
    }

    // SET n,r
    // -------
    // Set bit n
    private void SET_n_r(byte n, ref byte Rw)
    {
        Cycles += 2;
        SetBit(ref Rw, n, true);
    }

    // SET n,(HL)
    // ----------
    // Set bit n with direct memory at HL
    private void SET_n_HLn(byte n)
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        SetBit(ref data, n, true);
        Write(HL, data);
    }

    // RES n,r
    // -------
    // Set bit n
    private void RES_n_r(byte n, ref byte Rw)
    {
        Cycles += 2;
        SetBit(ref Rw, n, false);
    }

    // RES n,(HL)
    // ----------
    // Set bit n with direct memory at HL
    private void RES_n_HLn(byte n)
    {
        Cycles += 4;

        ushort HL = u16(L, H);
        byte data = Read(HL);
        SetBit(ref data, n, false);
        Write(HL, data);
    }
}
