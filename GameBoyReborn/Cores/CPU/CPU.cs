// ---
// CPU
// ---
public class CPU
{
    // Cycles
    public int Cycles;

    // Program counter
    public ushort PC;

    // Stack pointer
    private ushort SP;

    // Registers
    private byte A;
    private byte B;
    private byte C;
    private byte D;
    private byte E;
    private byte F;
    private byte H;
    private byte L;

    // Interrupt
    private bool IME;
    private bool IME_scheduled;

    // Handle flags
    private void Flags(bool Z, bool N, bool H, bool C)
    {
        byte _f = F;

        _f = Z ? (byte)(_f | 0b00010000) : (byte)(_f ^ 0b11101111);
        _f = N ? (byte)(_f | 0b00001000) : (byte)(_f ^ 0b11110111);
        _f = H ? (byte)(_f | 0b00000100) : (byte)(_f ^ 0b11111011);
        _f = C ? (byte)(_f | 0b00000010) : (byte)(_f ^ 0b11111101);

        F = _f;
    }
    /* Also to be tested : H => result > 0b1111 | C => result > 0b11111111 */
    private bool SetFlagZ(ushort result) { return result == 0; }
    private bool SetFlagH(ushort result) { return ReadBit(result, 3); }
    private bool SetFlagC(ushort result) { return ReadBit(result, 7); }
    private bool GetFlagZ() { return ReadBit(F, 4); }
    private bool GetFlagN() { return ReadBit(F, 3); }
    private bool GetFlagH() { return ReadBit(F, 2); }
    private bool GetFlagC() { return ReadBit(F, 1); }

    // Handle memory
    private byte Read(ushort at) { return 0xff; }
    private void Write(ushort at, byte b) { }

    // Handle binary
    private byte Lsb(ushort u16) { return (byte)(u16 & 0xFF); } // Least significant bit
    private byte Msb(ushort u16) { return (byte)((u16 >> 8) & 0xFF); } // Most significant bit
    private ushort u16(byte lsb, byte msb) { return (ushort)((msb << 8) | lsb); } // Create unsigned 16
    private bool ReadBit(byte data, byte pos) { return ((data >> pos) & 0x01) == 1; } // From right to left
    private bool ReadBit(ushort data, byte pos) { return ((data >> pos) & 0x01) == 1; } // Same

    // Execution
    // ---------
    public void Execution()
    {
        Instructions[Read(++PC)]?.Invoke();
    }

    // Dispatcher
    // ----------
    private delegate void InstructionDelegate();
    private InstructionDelegate[] Instructions;
    private InstructionDelegate[] CB_Instructions;

    public CPU()
    {
        Instructions = new InstructionDelegate[]
        {   /*          [0]                        [1]                             [2]                        [3]                           [4]                        [5]                        [6]                      [7]                        [8]                        [9]                                         [A]                        [B]                           [C]                        [D]                        [E]                      [F]       */
            /* [0] */   ()=> NOP(),                ()=> LD_RRw_nn(ref B, ref C),   ()=> LD_BCn_A(),           ()=> INC_RRw(ref B, ref C),   ()=> INC_Rw(ref B),        ()=> DEC_Rw(ref B),        ()=> LD_Rw_n(ref B),     ()=> RLCA(),               ()=> LD_Nnn_SP(),          ()=> ADD_RRw_RRr(ref H, ref L, B, ref C),   ()=> LD_A_BCn(),           ()=> DEC_RRw(ref B, ref C),   ()=> INC_Rw(ref C),        ()=> DEC_Rw(ref C),        ()=> LD_Rw_n(ref C),     ()=> RRCA(),
            /* [1] */   ()=> STOP(),               ()=> LD_RRw_nn(ref D, ref E),   ()=> LD_DEn_A(),           ()=> INC_RRw(ref D, ref E),   ()=> INC_Rw(ref D),        ()=> DEC_Rw(ref D),        ()=> LD_Rw_n(ref D),     ()=> RLA(),                ()=> JR_e(),               ()=> ADD_RRw_RRr(ref H, ref L, D, ref E),   ()=> LD_A_DEn(),           ()=> DEC_RRw(ref D, ref E),   ()=> INC_Rw(ref E),        ()=> DEC_Rw(ref E),        ()=> LD_Rw_n(ref E),     ()=> RRA(),
            /* [2] */   ()=> JR_NZ_ne(),           ()=> LD_RRw_nn(ref H, ref L),   ()=> LD_HLnp_A(),          ()=> INC_RRw(ref H, ref L),   ()=> INC_Rw(ref H),        ()=> DEC_Rw(ref H),        ()=> LD_Rw_n(ref H),     ()=> DAA(),                ()=> JR_Z_ne(),            ()=> ADD_RRw_RRr(ref H, ref L, H, ref L),   ()=> LD_A_HLnp(),          ()=> DEC_RRw(ref H, ref L),   ()=> INC_Rw(ref L),        ()=> DEC_Rw(ref L),        ()=> LD_Rw_n(ref L),     ()=> CPL(),
            /* [3] */   ()=> JR_NC_ne(),           ()=> LD_RRw_nn(ref SP),         ()=> LD_HLnm_A(),          ()=> INC_RRw(ref SP),         ()=> INC_HLn(H, L),        ()=> DEC_HLn(H, L),        ()=> LD_HLn_n(),         ()=> SCF(),                ()=> JR_C_ne(),            ()=> ADD_HLSP(ref H, ref L),                ()=> LD_A_HLnm(),          ()=> DEC_RRw(ref SP),         ()=> INC_Rw(ref A),        ()=> DEC_Rw(ref A),        ()=> LD_Rw_n(ref A),     ()=> CCF(),
            /* [4] */   ()=> LD_Rw_Rr(ref B, B),   ()=> LD_Rw_Rr(ref B, C),        ()=> LD_Rw_Rr(ref B, D),   ()=> LD_Rw_Rr(ref B, E),      ()=> LD_Rw_Rr(ref B, H),   ()=> LD_Rw_Rr(ref B, L),   ()=> LD_Rw_HLn(ref B),   ()=> LD_Rw_Rr(ref B, A),   ()=> LD_Rw_Rr(ref C, B),   ()=> LD_Rw_Rr(ref C, C),                    ()=> LD_Rw_Rr(ref C, D),   ()=> LD_Rw_Rr(ref C, E),      ()=> LD_Rw_Rr(ref C, H),   ()=> LD_Rw_Rr(ref C, L),   ()=> LD_Rw_HLn(ref C),   ()=> LD_Rw_Rr(ref C, A),
            /* [5] */   ()=> LD_Rw_Rr(ref D, B),   ()=> LD_Rw_Rr(ref D, C),        ()=> LD_Rw_Rr(ref D, D),   ()=> LD_Rw_Rr(ref D, E),      ()=> LD_Rw_Rr(ref D, H),   ()=> LD_Rw_Rr(ref D, L),   ()=> LD_Rw_HLn(ref D),   ()=> LD_Rw_Rr(ref D, A),   ()=> LD_Rw_Rr(ref E, B),   ()=> LD_Rw_Rr(ref E, C),                    ()=> LD_Rw_Rr(ref E, D),   ()=> LD_Rw_Rr(ref E, E),      ()=> LD_Rw_Rr(ref E, H),   ()=> LD_Rw_Rr(ref E, L),   ()=> LD_Rw_HLn(ref E),   ()=> LD_Rw_Rr(ref E, A),
            /* [6] */   ()=> LD_Rw_Rr(ref H, B),   ()=> LD_Rw_Rr(ref H, C),        ()=> LD_Rw_Rr(ref H, D),   ()=> LD_Rw_Rr(ref H, E),      ()=> LD_Rw_Rr(ref H, H),   ()=> LD_Rw_Rr(ref H, L),   ()=> LD_Rw_HLn(ref H),   ()=> LD_Rw_Rr(ref H, A),   ()=> LD_Rw_Rr(ref L, B),   ()=> LD_Rw_Rr(ref L, C),                    ()=> LD_Rw_Rr(ref L, D),   ()=> LD_Rw_Rr(ref L, E),      ()=> LD_Rw_Rr(ref L, H),   ()=> LD_Rw_Rr(ref L, L),   ()=> LD_Rw_HLn(ref L),   ()=> LD_Rw_Rr(ref L, A),
            /* [7] */   ()=> LD_HLn_Rr(B),         ()=> LD_HLn_Rr(C),              ()=> LD_HLn_Rr(D),         ()=> LD_HLn_Rr(E),            ()=> LD_HLn_Rr(H),         ()=> LD_HLn_Rr(L),         ()=> HALT(),             ()=> LD_HLn_Rr(A),         ()=> LD_Rw_Rr(ref A, B),   ()=> LD_Rw_Rr(ref A, C),                    ()=> LD_Rw_Rr(ref A, D),   ()=> LD_Rw_Rr(ref A, E),      ()=> LD_Rw_Rr(ref A, H),   ()=> LD_Rw_Rr(ref A, L),   ()=> LD_Rw_HLn(ref A),   ()=> LD_Rw_Rr(ref A, A),
            /* [8] */   ()=> ADD_Rr(B),            ()=> ADD_Rr(C),                 ()=> ADD_Rr(D),            ()=> ADD_Rr(E),               ()=> ADD_Rr(H),            ()=> ADD_Rr(L),            ()=> ADD_HLn(),          ()=> ADD_Rr(A),            ()=> ADC_Rr(B),            ()=> ADC_Rr(C),                             ()=> ADC_Rr(D),            ()=> ADC_Rr(E),               ()=> ADC_Rr(H),            ()=> ADC_Rr(L),            ()=> ADC_HLn(),          ()=> ADC_Rr(A),
            /* [9] */   ()=> SUB_Rr(B),            ()=> SUB_Rr(C),                 ()=> SUB_Rr(D),            ()=> SUB_Rr(E),               ()=> SUB_Rr(H),            ()=> SUB_Rr(L),            ()=> SUB_HLn(),          ()=> SUB_Rr(A),            ()=> SBC_Rr(B),            ()=> SBC_Rr(C),                             ()=> SBC_Rr(D),            ()=> SBC_Rr(E),               ()=> SBC_Rr(H),            ()=> SBC_Rr(L),            ()=> SBC_HLn(),          ()=> SBC_Rr(A),
            /* [A] */   ()=> AND_Rr(B),            ()=> AND_Rr(C),                 ()=> AND_Rr(D),            ()=> AND_Rr(E),               ()=> AND_Rr(H),            ()=> AND_Rr(L),            ()=> AND_HLn(),          ()=> AND_Rr(A),            ()=> XOR_Rr(B),            ()=> XOR_Rr(C),                             ()=> XOR_Rr(D),            ()=> XOR_Rr(E),               ()=> XOR_Rr(H),            ()=> XOR_Rr(L),            ()=> XOR_HLn(),          ()=> XOR_Rr(A),
            /* [B] */   ()=> OR_Rr(B),             ()=> OR_Rr(C),                  ()=> OR_Rr(D),             ()=> OR_Rr(E),                ()=> OR_Rr(H),             ()=> OR_Rr(L),             ()=> OR_HLn(),           ()=> OR_Rr(A),             ()=> CP_Rr(B),             ()=> CP_Rr(C),                              ()=> CP_Rr(D),             ()=> CP_Rr(E),                ()=> CP_Rr(H),             ()=> CP_Rr(L),             ()=> CP_HLn(),           ()=> CP_Rr(A),
            /* [C] */   ()=> RET_NZ(),             ()=> POP_RR(ref B, ref C),      ()=> JP_NZ_nn(),           ()=> JP_nn(),                 ()=> CALL_NZ_nn(),         ()=> PUSH_RR(B, C),        ()=> ADD_n(),            ()=> RST(0x00),            ()=> RET_Z(),              ()=> RET(),                                 ()=> JP_Z_nn(),            ()=> CB_op(),                 ()=> CALL_Z_nn(),          ()=> CALL_nn(),            ()=> ADC_n(),            ()=> RST(0x08),
            /* [D] */   ()=> RET_NC(),             ()=> POP_RR(ref D, ref E),      ()=> JP_NC_nn(),           ()=> unknown(),               ()=> CALL_NC_nn(),         ()=> PUSH_RR(D, E),        ()=> SUB_n(),            ()=> RST(0x10),            ()=> RET_C(),              ()=> RETI(),                                ()=> JP_C_nn(),            ()=> unknown(),               ()=> CALL_C_nn(),          ()=> unknown(),            ()=> SBC_n(),            ()=> RST(0x18),
            /* [E] */   ()=> LDH_Nn_A(),           ()=> POP_RR(ref H, ref L),      ()=> LDH_Cn_A(),           ()=> unknown(),               ()=> unknown(),            ()=> PUSH_RR(H, L),        ()=> AND_n(),            ()=> RST(0x20),            ()=> ADD_SP_Ne(),          ()=> JP_HL(),                               ()=> LD_nn_A(),            ()=> unknown(),               ()=> unknown(),            ()=> unknown(),            ()=> XOR_n(),            ()=> RST(0x28),
            /* [F] */   ()=> LDH_A_Nn(),           ()=> POP_RR(ref A, ref F),      ()=> LDH_A_Cn(),           ()=> DI(),                    ()=> unknown(),            ()=> PUSH_RR(A, F),        ()=> OR_n(),             ()=> RST(0x30),            ()=> LD_HL_SP_ne(),        ()=> LD_SP_HL(),                            ()=> LD_A_nn(),            ()=> EI(),                    ()=> unknown(),            ()=> unknown(),            ()=> CP_n(),             ()=> RST(0x38)
        };

        CB_Instructions = new InstructionDelegate[]
        {
            /*          [0]                   [1]                   [2]                   [3]                   [4]                   [5]                   [6]                [7]                   [8]                   [9]                   [A]                   [B]                   [C]                   [D]                   [E]                [F]          */
            /* [0] */   ()=> RLC(ref B),      ()=> RLC(ref C),      ()=> RLC(ref D),      ()=> RLC(ref E),      ()=> RLC(ref H),      ()=> RLC(ref L),      ()=> RLC_HLn(),    ()=> RLC(ref A),      ()=> RRC(ref B),      ()=> RRC(ref C),      ()=> RRC(ref D),      ()=> RRC(ref E),      ()=> RRC(ref H),      ()=> RRC(ref L),      ()=> RRC_HLn(),    ()=> RRC(ref A),
            /* [1] */   ()=> RL(ref B),       ()=> RL(ref C),       ()=> RL(ref D),       ()=> RL(ref E),       ()=> RL(ref H),       ()=> RL(ref L),       ()=> RL_HLn(),     ()=> RL(ref A),       ()=> RR(ref B),       ()=> RR(ref C),       ()=> RR(ref D),       ()=> RR(ref E),       ()=> RR(ref H),       ()=> RR(ref L),       ()=> RR_HLn(),     ()=> RR(ref A),
            /* [2] */   ()=> SLA(ref B),      ()=> SLA(ref C),      ()=> SLA(ref D),      ()=> SLA(ref E),      ()=> SLA(ref H),      ()=> SLA(ref L),      ()=> SLA_HLn(),    ()=> SLA(ref A),      ()=> SRA(ref B),      ()=> SRA(ref C),      ()=> SRA(ref D),      ()=> SRA(ref E),      ()=> SRA(ref H),      ()=> SRA(ref L),      ()=> SRA_HLn(),    ()=> SRA(ref A),
            /* [3] */   ()=> SWAP(ref B),     ()=> SWAP(ref C),     ()=> SWAP(ref D),     ()=> SWAP(ref E),     ()=> SWAP(ref H),     ()=> SWAP(ref L),     ()=> SWAP_HLn(),   ()=> SWAP(ref A),     ()=> SRL(ref B),      ()=> SRL(ref C),      ()=> SRL(ref D),      ()=> SRL(ref E),      ()=> SRL(ref H),      ()=> SRL(ref L),      ()=> SRL_HLn(),    ()=> SRL(ref A),
            /* [4] */   ()=> BIT(0, ref B),   ()=> BIT(0, ref C),   ()=> BIT(0, ref D),   ()=> BIT(0, ref E),   ()=> BIT(0, ref H),   ()=> BIT(0, ref L),   ()=> BIT_HLn(0),   ()=> BIT(0, ref A),   ()=> BIT(1, ref B),   ()=> BIT(1, ref C),   ()=> BIT(1, ref D),   ()=> BIT(1, ref E),   ()=> BIT(1, ref H),   ()=> BIT(1, ref L),   ()=> BIT_HLn(1),   ()=> BIT(1, ref A),
            /* [5] */   ()=> BIT(2, ref B),   ()=> BIT(2, ref C),   ()=> BIT(2, ref D),   ()=> BIT(2, ref E),   ()=> BIT(2, ref H),   ()=> BIT(2, ref L),   ()=> BIT_HLn(2),   ()=> BIT(2, ref A),   ()=> BIT(3, ref B),   ()=> BIT(3, ref C),   ()=> BIT(3, ref D),   ()=> BIT(3, ref E),   ()=> BIT(3, ref H),   ()=> BIT(3, ref L),   ()=> BIT_HLn(3),   ()=> BIT(3, ref A),
            /* [6] */   ()=> BIT(4, ref B),   ()=> BIT(4, ref C),   ()=> BIT(4, ref D),   ()=> BIT(4, ref E),   ()=> BIT(4, ref H),   ()=> BIT(4, ref L),   ()=> BIT_HLn(4),   ()=> BIT(4, ref A),   ()=> BIT(5, ref B),   ()=> BIT(5, ref C),   ()=> BIT(5, ref D),   ()=> BIT(5, ref E),   ()=> BIT(5, ref H),   ()=> BIT(5, ref L),   ()=> BIT_HLn(5),   ()=> BIT(5, ref A),
            /* [7] */   ()=> BIT(6, ref B),   ()=> BIT(6, ref C),   ()=> BIT(6, ref D),   ()=> BIT(6, ref E),   ()=> BIT(6, ref H),   ()=> BIT(6, ref L),   ()=> BIT_HLn(6),   ()=> BIT(6, ref A),   ()=> BIT(7, ref B),   ()=> BIT(7, ref C),   ()=> BIT(7, ref D),   ()=> BIT(7, ref E),   ()=> BIT(7, ref H),   ()=> BIT(7, ref L),   ()=> BIT_HLn(7),   ()=> BIT(7, ref A),
            /* [8] */   ()=> RES(0, ref B),   ()=> RES(0, ref C),   ()=> RES(0, ref D),   ()=> RES(0, ref E),   ()=> RES(0, ref H),   ()=> RES(0, ref L),   ()=> RES_HLn(0),   ()=> RES(0, ref A),   ()=> RES(1, ref B),   ()=> RES(1, ref C),   ()=> RES(1, ref D),   ()=> RES(1, ref E),   ()=> RES(1, ref H),   ()=> RES(1, ref L),   ()=> RES_HLn(1),   ()=> RES(1, ref A),
            /* [9] */   ()=> RES(2, ref B),   ()=> RES(2, ref C),   ()=> RES(2, ref D),   ()=> RES(2, ref E),   ()=> RES(2, ref H),   ()=> RES(2, ref L),   ()=> RES_HLn(2),   ()=> RES(2, ref A),   ()=> RES(3, ref B),   ()=> RES(3, ref C),   ()=> RES(3, ref D),   ()=> RES(3, ref E),   ()=> RES(3, ref H),   ()=> RES(3, ref L),   ()=> RES_HLn(3),   ()=> RES(3, ref A),
            /* [A] */   ()=> RES(4, ref B),   ()=> RES(4, ref C),   ()=> RES(4, ref D),   ()=> RES(4, ref E),   ()=> RES(4, ref H),   ()=> RES(4, ref L),   ()=> RES_HLn(4),   ()=> RES(4, ref A),   ()=> RES(5, ref B),   ()=> RES(5, ref C),   ()=> RES(5, ref D),   ()=> RES(5, ref E),   ()=> RES(5, ref H),   ()=> RES(5, ref L),   ()=> RES_HLn(5),   ()=> RES(5, ref A),
            /* [B] */   ()=> RES(6, ref B),   ()=> RES(6, ref C),   ()=> RES(6, ref D),   ()=> RES(6, ref E),   ()=> RES(6, ref H),   ()=> RES(6, ref L),   ()=> RES_HLn(6),   ()=> RES(6, ref A),   ()=> RES(7, ref B),   ()=> RES(7, ref C),   ()=> RES(7, ref D),   ()=> RES(7, ref E),   ()=> RES(7, ref H),   ()=> RES(7, ref L),   ()=> RES_HLn(7),   ()=> RES(7, ref A),
            /* [4] */   ()=> SET(0, ref B),   ()=> SET(0, ref C),   ()=> SET(0, ref D),   ()=> SET(0, ref E),   ()=> SET(0, ref H),   ()=> SET(0, ref L),   ()=> SET_HLn(0),   ()=> SET(0, ref A),   ()=> SET(1, ref B),   ()=> SET(1, ref C),   ()=> SET(1, ref D),   ()=> SET(1, ref E),   ()=> SET(1, ref H),   ()=> SET(1, ref L),   ()=> SET_HLn(1),   ()=> SET(1, ref A),
            /* [5] */   ()=> SET(2, ref B),   ()=> SET(2, ref C),   ()=> SET(2, ref D),   ()=> SET(2, ref E),   ()=> SET(2, ref H),   ()=> SET(2, ref L),   ()=> SET_HLn(2),   ()=> SET(2, ref A),   ()=> SET(3, ref B),   ()=> SET(3, ref C),   ()=> SET(3, ref D),   ()=> SET(3, ref E),   ()=> SET(3, ref H),   ()=> SET(3, ref L),   ()=> SET_HLn(3),   ()=> SET(3, ref A),
            /* [6] */   ()=> SET(4, ref B),   ()=> SET(4, ref C),   ()=> SET(4, ref D),   ()=> SET(4, ref E),   ()=> SET(4, ref H),   ()=> SET(4, ref L),   ()=> SET_HLn(4),   ()=> SET(4, ref A),   ()=> SET(5, ref B),   ()=> SET(5, ref C),   ()=> SET(5, ref D),   ()=> SET(5, ref E),   ()=> SET(5, ref H),   ()=> SET(5, ref L),   ()=> SET_HLn(5),   ()=> SET(5, ref A),
            /* [7] */   ()=> SET(6, ref B),   ()=> SET(6, ref C),   ()=> SET(6, ref D),   ()=> SET(6, ref E),   ()=> SET(6, ref H),   ()=> SET(6, ref L),   ()=> SET_HLn(6),   ()=> SET(6, ref A),   ()=> SET(7, ref B),   ()=> SET(7, ref C),   ()=> SET(7, ref D),   ()=> SET(7, ref E),   ()=> SET(7, ref H),   ()=> SET(7, ref L),   ()=> SET_HLn(7),   ()=> SET(7, ref A),
        };
    }

    // All set
    // -------

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

    // LD A, (BC): Load accumulator (indirect BC)
    // ------------------------------------------
    // Load to the 8-bit A register, data from the absolute address specified by
    // the 16-bit register BC.
    private void LD_A_BCn()
    {
        Cycles += 2;
        A = Read(u16(C, B));
    }

    // LD A, (DE): Load accumulator (indirect DE)
    // ------------------------------------------
    // Load to the 8-bit A register, data from the absolute address specified by the
    // 16-bit register DE.
    private void LD_A_DEn()
    {
        Cycles += 2;
        A = Read(u16(E, D));
    }

    // LD (BC), A: Load from accumulator (indirect BC)
    // -----------------------------------------------
    // Load to the absolute address specified by the 16-bit register BC, data
    // from the 8-bit A register
    private void LD_BCn_A()
    {
        Cycles += 2;
        Write(u16(C, B), A);
    }

    // LD (DE), A: Load from accumulator (indirect DE)
    // -----------------------------------------------
    // Load to the absolute address specified by the 16-bit register DE, data
    // from the 8-bit A register
    private void LD_DEn_A()
    {
        Cycles += 2;
        Write(u16(E, D), A);
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
        Write(nn++, Msb(SP));
    }

    // LD SP, HL: Load stack pointer from HL
    // -------------------------------------
    // Load to the 16-bit SP register, data from the 16-bit HL register.
    private void LD_SD_HL()
    {
        Cycles += 2;
        SP = u16(L, H);
    }

    // PUSH rr: Push to stack
    // ----------------------
    // Push to the stack memory, data from the 16-bit register rr.
    private void PUSH_RR(byte Rmr, byte Rlr)
    {
        Cycles += 4;

        SP--;
        Write(SP--, Rlr);
        Write(SP, Rmr);
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

    // ADD r: Add (register)
    // ---------------------
    // Adds to the 8-bit A register, the 8-bit register r, and stores the result
    // back into the A register.
    private void ADD_Rr(byte Rr)
    {
        Cycles += 1;
        ushort result = (ushort)(A + Rr);
        ;
        A = (byte)result;
        Flags(SetFlagZ(result), false, SetFlagH(result), SetFlagC(result));
    }

    // ADD (HL): Add (indirect HL)
    // ---------------------------
    // Adds to the 8-bit A register, data from the absolute address specified by the
    // 16-bit register HL, and stores the result back into the A register.
    private void ADD_HLn()
    {
        Cycles += 2;
        byte n = Read(u16(L, H));
        ushort result = (ushort)(A + n);

        A = (byte)result;
        Flags(SetFlagZ(result), false, SetFlagH(result), SetFlagC(result));

    }

    // ADD n: Add (immediate)
    // ----------------------
    // Adds to the 8-bit A register, the immediate data n, and stores the result
    // back into the A register
    private void ADD_n()
    {
        Cycles += 2;
        byte n = Read(PC++);
        ushort result = (ushort)(A + n);

        A = (byte)result;
        Flags(SetFlagZ(result), false, SetFlagH(result), SetFlagC(result));
    }

    // ADC r: Add with carry (register)
    // --------------------------------
    // Adds to the 8-bit A register, the carry flag and the 8-bit register r, and
    // stores the result back into the A register.
    private void ADC_Rr(byte Rr)
    {
        Cycles += 1;
        ushort result = (ushort)(A + (GetFlagC() ? 1 : 0) + Rr);

        A = (byte)result;
        Flags(SetFlagZ(result), false, SetFlagH(result), SetFlagC(result));
    }

    // ADC (HL): Add with carry (indirect HL)
    // --------------------------------------
    // Adds to the 8-bit A register, the carry flag and data from the absolute address 
    // specified by the 16-bit register HL, and stores the result back into the A register

    private void ADC_HLn()
    {
        Cycles += 2;

        byte data = Read(u16(L, H));
        ushort result = (ushort)(A + (GetFlagC() ? 1 : 0) + data);

        A = (byte)result;
        Flags(SetFlagZ(result), false, SetFlagH(result), SetFlagC(result));
    }
}
