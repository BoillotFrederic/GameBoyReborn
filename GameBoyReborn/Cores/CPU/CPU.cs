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

    // Read memory
    private byte Read(ushort at)
    {
        return 0xff;
    }

    // Write memory
    private void Write(ushort at, byte b)
    {
    }

    // Least and most significant bit
    private byte lsb(ushort u16)
    {
        return (byte)(u16 & 0xFF);
    }
    private byte msb(ushort u16)
    {
        return (byte)((u16 >> 8) & 0xFF);
    }

    // Create unsigned 16
    private ushort u16(byte lsb, byte msb)
    {
        return (ushort)((msb << 8) | lsb);
    }

    // Execution
    // ---------
    public void Execution()
    {
        Dispatcher[Read(++PC)]?.Invoke();
    }

    // Dispatcher
    // ----------
    private delegate void InstructionDelegate();
    private InstructionDelegate[] Dispatcher;

    public CPU()
    {
        Dispatcher = new InstructionDelegate[]
        {
            /*0*/ (/*0*/) => NOP(),  (/*1*/) => LD_RRw_nn(ref B, ref C), (/*2*/) => LD_BCn_A(), (/*3*/) => INC_RRw(ref B, ref C), (/*4*/) => INC_Rw(ref B), (/*5*/) => DEC_Rw(ref B), (/*6*/) => LD_Rw_n(ref B), (/*7*/) => RLCA(), (/*8*/) => LD_Nnn_SP(), (/*9*/) => ADD_RRw_RRr(ref HL, B, ref C), (/*A*/) => LD_A_BCn(), (/*B*/) => DEC_RRw(ref B, ref C), (/*C*/) => INC_Rw(ref C), (/*D*/) => DEC_Rw(ref C), (/*E*/) => LD_Rw_n(ref C), (/*F*/) => RRCA(),
            /*1*/ (/*0*/) => STOP(), (/*1*/) => LD_RRw_nn(ref D, ref E), (/*2*/) => LD_DEn_A(), (/*3*/) => INC_RRw(ref D, ref E), (/*4*/) => INC_Rw(ref D), (/*5*/) => DEC_Rw(ref D), (/*6*/) => LD_Rw_n(ref D), (/*7*/) => RLA(),  (/*8*/) => JR_e(),      (/*9*/) => ADD_RRw_RRr(ref HL, D, ref E),
            /*2*/
            /*3*/
            /*4*/
            /*5*/
            /*6*/
            /*7*/
            /*8*/
            /*9*/
            /*A*/
            /*B*/
            /*C*/
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
        H = msb(HL);
        L = lsb(HL);
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
        H = msb(HL);
        L = lsb(HL);
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
        H = msb(HL);
        L = lsb(HL);
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
        H = msb(HL);
        L = lsb(HL);
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

    // LD (nn), SP: Load from stack pointer (direct)
    // ---------------------------------------------
    // Load to the absolute address specified by the 16-bit operand nn, data from the 16-bit SP register.
    private void LD_Nnn_SP()
    {
        Cycles += 5;

        ushort nn = u16(Read(PC++), Read(PC++));
        Write(nn, lsb(SP));
        Write(nn++, msb(SP));
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
    private void PUSH_RR(ushort RR)
    {
        Cycles += 4;

        SP--;
        Write(SP--, lsb(RR));
        Write(SP, msb(RR));
    }

    // POP rr: Pop from stack
    // ----------------------
    // Pops to the 16-bit register rr, data from the stack memory.This instruction
    // does not do calculations that affect flags, but POP AF completely replaces the
    // F register value, so all flags are changed based on the 8-bit data that is read
    // from memory.
    private void POP_RR(ref ushort RR)
    {
        Cycles += 3;
        RR = u16(Read(SP++), Read(SP++));
    }


}
