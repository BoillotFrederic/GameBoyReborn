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

    public CPU()
    {
        Instructions = new InstructionDelegate[]
        {
            /*0*/ (/*0*/) => NOP(), (/*1*/) => LD_RRw_nn(ref B, ref C), (/*2*/) => LD_BCn_A(), (/*3*/) => INC_RRw(ref B, ref C), (/*4*/) => INC_Rw(ref B), (/*5*/) => DEC_Rw(ref B), (/*6*/) => LD_Rw_n(ref B), (/*7*/) => RLCA(), (/*8*/) => LD_Nnn_SP(), (/*9*/) => ADD_RRw_RRr(ref H, ref L, B, ref C), (/*A*/) => LD_A_BCn(), (/*B*/) => DEC_RRw(ref B, ref C), (/*C*/) => INC_Rw(ref C), (/*D*/) => DEC_Rw(ref C), (/*E*/) => LD_Rw_n(ref C), (/*F*/) => RRCA(),
            /*1*/ (/*0*/) => STOP(), (/*1*/) => LD_RRw_nn(ref D, ref E), (/*2*/) => LD_DEn_A(), (/*3*/) => INC_RRw(ref D, ref E), (/*4*/) => INC_Rw(ref D), (/*5*/) => DEC_Rw(ref D), (/*6*/) => LD_Rw_n(ref D), (/*7*/) => RLA(),  (/*8*/) => JR_e(), (/*9*/) => ADD_RRw_RRr(ref H, ref L, D, ref E), (/*A*/) => LD_A_DEn(), (/*B*/) => DEC_RRw(ref D, ref E), (/*C*/) => INC_Rw(ref E), (/*D*/) => DEC_Rw(ref E), (/*E*/) => LD_Rw_n(ref E), (/*F*/) => RRA(),
            /*2*/ (/*0*/) => JR_NZ_ne(), (/*1*/) => LD_RRw_nn(ref H, ref L), (/*2*/) => LD_HLnp_A(), (/*3*/) => INC_RRw(ref H, ref L), (/*4*/) => INC_Rw(ref H), (/*5*/) => DEC_Rw(ref H), (/*6*/) => LD_Rw_n(ref H), (/*7*/) => DAA(), (/*8*/) => JR_Z_ne(), (/*9*/) => ADD_RRw_RRr(ref H, ref L, H, ref L), (/*A*/) => LD_A_HLnp(), (/*B*/) => DEC_RRw(ref H, ref L), (/*C*/) => INC_Rw(ref L), (/*D*/) => DEC_Rw(ref L), (/*E*/) => LD_Rw_n(ref L), (/*F*/) => CPL(),
            /*3*/ (/*0*/) => JR_NC_ne(), (/*1*/) => LD_RRw_nn(ref SP), (/*2*/) => LD_HLnm_A(), (/*3*/) => INC_RRw(ref SP), (/*4*/) => INC_HLn(H, L), (/*5*/) => DEC_HLn(H, L), (/*6*/) => LD_HLn_n(), (/*7*/) => SCF(), (/*8*/) => JR_C_ne(), (/*9*/) => ADD_HLSP(ref H, ref L), (/*A*/) => LD_A_HLnm(), (/*B*/) => DEC_RRw(ref SP), (/*C*/) => INC_Rw(ref A), (/*D*/) => DEC_Rw(ref A), (/*E*/) => LD_Rw_n(ref A), (/*F*/) => CCF(),
            /*4*/ (/*0*/) => LD_Rw_Rr(ref B, B), (/*1*/) => LD_Rw_Rr(ref B, C), (/*2*/) => LD_Rw_Rr(ref B, D), (/*3*/) => LD_Rw_Rr(ref B, E), (/*4*/) => LD_Rw_Rr(ref B, H), (/*5*/) => LD_Rw_Rr(ref B, L), (/*6*/) => LD_Rw_HLn(ref B), (/*7*/) => LD_Rw_Rr(ref B, A), (/*8*/) => LD_Rw_Rr(ref C, B), (/*9*/) => LD_Rw_Rr(ref C, C), (/*A*/) => LD_Rw_Rr(ref C, D), (/*B*/) => LD_Rw_Rr(ref C, E), (/*C*/) => LD_Rw_Rr(ref C, H), (/*D*/) => LD_Rw_Rr(ref C, L), (/*E*/) => LD_Rw_HLn(ref C), (/*F*/) => LD_Rw_Rr(ref C, A),
            /*5*/ (/*0*/) => LD_Rw_Rr(ref D, B), (/*1*/) => LD_Rw_Rr(ref D, C), (/*2*/) => LD_Rw_Rr(ref D, D), (/*3*/) => LD_Rw_Rr(ref D, E), (/*4*/) => LD_Rw_Rr(ref D, H), (/*5*/) => LD_Rw_Rr(ref D, L), (/*6*/) => LD_Rw_HLn(ref D), (/*7*/) => LD_Rw_Rr(ref D, A), (/*8*/) => LD_Rw_Rr(ref E, B), (/*9*/) => LD_Rw_Rr(ref E, C), (/*A*/) => LD_Rw_Rr(ref E, D), (/*B*/) => LD_Rw_Rr(ref E, E), (/*C*/) => LD_Rw_Rr(ref E, H), (/*D*/) => LD_Rw_Rr(ref E, L), (/*E*/) => LD_Rw_HLn(ref E), (/*F*/) => LD_Rw_Rr(ref E, A),
            /*6*/ (/*0*/) => LD_Rw_Rr(ref H, B), (/*1*/) => LD_Rw_Rr(ref H, C), (/*2*/) => LD_Rw_Rr(ref H, D), (/*3*/) => LD_Rw_Rr(ref H, E), (/*4*/) => LD_Rw_Rr(ref H, H), (/*5*/) => LD_Rw_Rr(ref H, L), (/*6*/) => LD_Rw_HLn(ref H), (/*7*/) => LD_Rw_Rr(ref H, A), (/*8*/) => LD_Rw_Rr(ref L, B), (/*9*/) => LD_Rw_Rr(ref L, C), (/*A*/) => LD_Rw_Rr(ref L, D), (/*B*/) => LD_Rw_Rr(ref L, E), (/*C*/) => LD_Rw_Rr(ref L, H), (/*D*/) => LD_Rw_Rr(ref L, L), (/*E*/) => LD_Rw_HLn(ref L), (/*F*/) => LD_Rw_Rr(ref L, A),
            /*7*/ (/*0*/) => LD_HLn_Rr(B), (/*1*/) => LD_HLn_Rr(C), (/*2*/) => LD_HLn_Rr(D), (/*3*/) => LD_HLn_Rr(E), (/*4*/) => LD_HLn_Rr(H), (/*5*/) => LD_HLn_Rr(L), (/*6*/) => HALT(), (/*7*/) => LD_HLn_Rr(A), (/*8*/) => LD_Rw_Rr(ref A, B), (/*9*/) => LD_Rw_Rr(ref A, C), (/*A*/) => LD_Rw_Rr(ref A, D), (/*B*/) => LD_Rw_Rr(ref A, E), (/*C*/) => LD_Rw_Rr(ref A, H), (/*D*/) => LD_Rw_Rr(ref A, L), (/*E*/) => LD_Rw_HLn(ref A), (/*F*/) => LD_Rw_Rr(ref A, A),
            /*8*/ (/*0*/) => ADD_Rr(B), (/*1*/) => ADD_Rr(C), (/*2*/) => ADD_Rr(D), (/*3*/) => ADD_Rr(E), (/*4*/) => ADD_Rr(H), (/*5*/) => ADD_Rr(L), (/*6*/) => ADD_HLn(), (/*7*/) => ADD_Rr(A), (/*8*/) => ADC_Rr(B), (/*9*/) => ADC_Rr(C), (/*A*/) => ADC_Rr(D), (/*B*/) => ADC_Rr(E), (/*C*/) => ADC_Rr(H), (/*D*/) => ADC_Rr(L), (/*E*/) => ADC_HLn(), (/*D*/) => ADC_Rr(A),
            /*9*/ (/*0*/) => SUB_Rr(B), (/*1*/) => SUB_Rr(C), (/*2*/) => SUB_Rr(D), (/*3*/) => SUB_Rr(E), (/*4*/) => SUB_Rr(H), (/*5*/) => SUB_Rr(L), (/*6*/) => SUB_HLn(), (/*7*/) => SUB_Rr(A), (/*8*/) => SBC_Rr(B), (/*9*/) => SBC_Rr(C), (/*A*/) => SBC_Rr(D), (/*B*/) => SBC_Rr(E), (/*C*/) => SBC_Rr(H), (/*D*/) => SBC_Rr(L), (/*E*/) => SBC_HLn(), (/*D*/) => SBC_Rr(A),
            /*A*/ (/*0*/) => AND_Rr(B), (/*1*/) => AND_Rr(C), (/*2*/) => AND_Rr(D), (/*3*/) => AND_Rr(E), (/*4*/) => AND_Rr(H), (/*5*/) => AND_Rr(L), (/*6*/) => AND_HLn(), (/*7*/) => AND_Rr(A), (/*8*/) => XOR_Rr(B), (/*9*/) => XOR_Rr(C), (/*A*/) => XOR_Rr(D), (/*B*/) => XOR_Rr(E), (/*C*/) => XOR_Rr(H), (/*D*/) => XOR_Rr(L), (/*E*/) => XOR_HLn(), (/*D*/) => XOR_Rr(A),
            /*B*/ (/*0*/) => OR_Rr(B), (/*1*/) => OR_Rr(C), (/*2*/) => OR_Rr(D), (/*3*/) => OR_Rr(E), (/*4*/) => OR_Rr(H), (/*5*/) => OR_Rr(L), (/*6*/) => OR_HLn(), (/*7*/) => OR_Rr(A), (/*8*/) => CP_Rr(B), (/*9*/) => CP_Rr(C), (/*A*/) => CP_Rr(D), (/*B*/) => CP_Rr(E), (/*C*/) => CP_Rr(H), (/*D*/) => CP_Rr(L), (/*E*/) => CP_HLn(), (/*D*/) => CP_Rr(A),
            /*C*/ (/*0*/) => RET_NZ(), (/*1*/) => POP_RR(ref B, ref C), (/*2*/) => JP_NZ_nn(), (/*3*/) => JP_nn(), (/*4*/) => CALL_NZ_nn(), (/*5*/) => PUSH_RR(B, C), (/*6*/) => ADD_n(), (/*7*/) => RST(0x00), (/*8*/) => RET_Z(), (/*9*/) => RET(), (/*A*/) => JP_Z_nn(), (/*B*/) => CB_op(), (/*C*/) => CALL_Z_nn(), (/*D*/) => CALL_nn(), (/*E*/) => ADC_n(), (/*F*/) => RST(0x08),
            /*D*/
            /*E*/
            /*F*/
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
