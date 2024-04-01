// ---
// CPU
// ---
using GameBoyReborn;

#pragma warning disable CS0414

namespace Emulator
{
    public class CPU
    {
        #region Construct

        private readonly IO IO;
        private readonly Cartridge Cartridge;

        // Handle memory
        private readonly Memory Memory;
        private delegate byte ReadDelegate(ushort at);
        private readonly ReadDelegate Read;
        private delegate void WriteDelegate(ushort at, byte b);
        private readonly WriteDelegate Write;

        // Instructions delegate
        private delegate void InstructionDelegate();
        private readonly InstructionDelegate[] Instructions;
        private readonly InstructionDelegate[] CB_Instructions;

        public CPU(Emulation Emulation)
        {
            // Relation
            IO = Emulation.IO;
            Cartridge = Emulation.Cartridge;
            Memory = Emulation.Memory;
            Read = Memory.Read;
            Write = Memory.Write;

            // Init
            Init();

            // Instructions
            Instructions = new InstructionDelegate[]
            {   
                /*          [0]                           [1]                             [2]                           [3]                           [4]                             [5]                        [6]                      [7]                        [8]                          [9]                                  [A]                          [B]                           [C]                            [D]                        [E]                      [F]      */
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
                /* [D] */   ()=> RET_CC(!FlagC),          ()=> POP_RR(ref D, ref E),      ()=> JP_CC_nn(!FlagC),        ()=> Unknown(),               ()=> CALL_CC_nn(!FlagC),        ()=> PUSH_RR(D, E),        ()=> SUB_n(),            ()=> RST_n(0x10),          ()=> RET_CC(FlagC),          ()=> RETI(),                         ()=> JP_CC_nn(FlagC),        ()=> Unknown(),               ()=> CALL_CC_nn(FlagC),        ()=> Unknown(),            ()=> SBC_n(),            ()=> RST_n(0x18),
                /* [E] */   ()=> LDH_Nn_A(),              ()=> POP_RR(ref H, ref L),      ()=> LDH_Cn_A(),              ()=> Unknown(),               ()=> Unknown(),                 ()=> PUSH_RR(H, L),        ()=> AND_n(),            ()=> RST_n(0x20),          ()=> ADD_SP_en(),            ()=> JP_HL(),                        ()=> LD_nn_A(),              ()=> Unknown(),               ()=> Unknown(),                ()=> Unknown(),            ()=> XOR_n(),            ()=> RST_n(0x28),
                /* [F] */   ()=> LDH_A_Nn(),              ()=> POP_AF(),                  ()=> LDH_A_Cn(),              ()=> DI(),                    ()=> Unknown(),                 ()=> PUSH_AF(),            ()=> OR_n(),             ()=> RST_n(0x30),          ()=> LD_HL_SP_en(),          ()=> LD_SP_HL(),                     ()=> LD_A_nn(),              ()=> EI(),                    ()=> Unknown(),                ()=> Unknown(),            ()=> CP_n(),             ()=> RST_n(0x38)
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
                /* [C] */   ()=> SET_n_r(0, ref B),   ()=> SET_n_r(0, ref C),   ()=> SET_n_r(0, ref D),   ()=> SET_n_r(0, ref E),   ()=> SET_n_r(0, ref H),   ()=> SET_n_r(0, ref L),   ()=> SET_n_HLn(0),   ()=> SET_n_r(0, ref A),   ()=> SET_n_r(1, ref B),   ()=> SET_n_r(1, ref C),   ()=> SET_n_r(1, ref D),   ()=> SET_n_r(1, ref E),   ()=> SET_n_r(1, ref H),   ()=> SET_n_r(1, ref L),   ()=> SET_n_HLn(1),     ()=> SET_n_r(1, ref A),
                /* [D] */   ()=> SET_n_r(2, ref B),   ()=> SET_n_r(2, ref C),   ()=> SET_n_r(2, ref D),   ()=> SET_n_r(2, ref E),   ()=> SET_n_r(2, ref H),   ()=> SET_n_r(2, ref L),   ()=> SET_n_HLn(2),   ()=> SET_n_r(2, ref A),   ()=> SET_n_r(3, ref B),   ()=> SET_n_r(3, ref C),   ()=> SET_n_r(3, ref D),   ()=> SET_n_r(3, ref E),   ()=> SET_n_r(3, ref H),   ()=> SET_n_r(3, ref L),   ()=> SET_n_HLn(3),     ()=> SET_n_r(3, ref A),
                /* [E] */   ()=> SET_n_r(4, ref B),   ()=> SET_n_r(4, ref C),   ()=> SET_n_r(4, ref D),   ()=> SET_n_r(4, ref E),   ()=> SET_n_r(4, ref H),   ()=> SET_n_r(4, ref L),   ()=> SET_n_HLn(4),   ()=> SET_n_r(4, ref A),   ()=> SET_n_r(5, ref B),   ()=> SET_n_r(5, ref C),   ()=> SET_n_r(5, ref D),   ()=> SET_n_r(5, ref E),   ()=> SET_n_r(5, ref H),   ()=> SET_n_r(5, ref L),   ()=> SET_n_HLn(5),     ()=> SET_n_r(5, ref A),
                /* [F] */   ()=> SET_n_r(6, ref B),   ()=> SET_n_r(6, ref C),   ()=> SET_n_r(6, ref D),   ()=> SET_n_r(6, ref E),   ()=> SET_n_r(6, ref H),   ()=> SET_n_r(6, ref L),   ()=> SET_n_HLn(6),   ()=> SET_n_r(6, ref A),   ()=> SET_n_r(7, ref B),   ()=> SET_n_r(7, ref C),   ()=> SET_n_r(7, ref D),   ()=> SET_n_r(7, ref E),   ()=> SET_n_r(7, ref H),   ()=> SET_n_r(7, ref L),   ()=> SET_n_HLn(7),     ()=> SET_n_r(7, ref A),
            };
        }

        #endregion

        #region CPU operating varaibles

        // Frequency
        public int Frequency;

        // Cycles
        public byte Cycles;
        public long LoopAccumulator;

        // Program counter
        public ushort PC;

        // Stack pointer
        public ushort SP;

        // Registers
        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte H;
        public byte L;

        // Interrupt
        public bool Stop = false;
        public bool Halt = false;
        public sbyte IME = -1;

        // Handle flags
        public bool FlagZ;
        public bool FlagN;
        public bool FlagH;
        public bool FlagC;

        // Opcode
        public byte Opcode;
        public byte LastOpcode;

        private void Init()
        {
            Frequency = Cartridge.PUS.GameBoyGen switch
            {
                0 => 4194304,
                1 => 4295500,
                2 => 8388608,
                _ => 4295500,
            };

            PC = (ushort)(Memory.booting ? 0x0000 : 0x0100);
            SP = 0xFFFE;
            A = Cartridge.PUS.A;
            B = Cartridge.PUS.B;
            C = Cartridge.PUS.C;
            D = Cartridge.PUS.D;
            E = Cartridge.PUS.E;
            H = Cartridge.PUS.H;
            L = Cartridge.PUS.L;
            FlagZ = Cartridge.PUS.FlagZ;
            FlagN = Cartridge.PUS.FlagN;
            FlagH = Cartridge.PUS.FlagH;
            FlagC = Cartridge.PUS.FlagC;
        }

        #endregion

        #region CPU ticks

        // Execution
        // ---------
        bool HaltBug = false;
        public void Execution()
        {
            Cycles = 0;

            // Instructions
            if (!Halt && !Stop)
            {
                // Interrupts handle
                Interrupts();

                LoopAccumulator++;
                LastOpcode = Opcode;
                Opcode = Read(PC++);

                Instructions[Opcode]?.Invoke();
            }
/*            else if(!Stop)
            {
                Cycles++;

                if ((IO.IE & IO.IF) != 0)
                {
                    Halt = false;

*//*                    if (IME < 0)
                        PC--;*//*
                }

*//*                if (HaltBug)
                {
                    Halt = false;
                    //Console.WriteLine("test");
                    PC++;
                    HaltBug = false;
                }*//*
            }*/
            else
            {
                Cycles++;
            }

            // Halt
            if (Halt)
            {
                if ((IO.IE & IO.IF) != 0)
                {
                    Halt = false;

                    if (IME > 0)
                    PC--;
                }
            }

            // Quit boot rom
            if (Memory.booting && PC == 0x100)
            Memory.booting = false;
        }

        // Interrupts handle
        // -----------------
        private void Interrupts()
        {
            if (IME >= 0)
            {
                byte IEnIF = (byte)(IO.IE & IO.IF);

                if (IME == 0 && IEnIF != 0)
                {
                    // Push
                    bool intrCancel = false;
                    ushort pushMsbAdress = --SP;
                    byte pushMsb = Binary.Msb(PC);
                    Write(pushMsbAdress, pushMsb);
                    Write(--SP, Binary.Lsb(PC));
                    Cycles += 5;

                    // IE set when pushed
                    if(pushMsbAdress == 0xFFFF)
                    {
                        byte interruptCount = 0;

                        for (byte i = 0; i < 5; i++)
                        if (Binary.ReadBit(IO.IF, i))
                        interruptCount++;

                        if(interruptCount == 1)
                        intrCancel = true;

                        else
                        IEnIF = (byte)(IO.IE & IO.IF);
                    }

                    // Interrupt cancel
                    if(intrCancel)
                    PC = 0;

                    // VBlank interrupt
                    else if (Binary.ReadBit(IEnIF, 0))
                    {
                        Binary.SetBit(ref IO.IF, 0, false);
                        PC = 0x40;
                    }

                    // STAT interrupt
                    else if (Binary.ReadBit(IEnIF, 1))
                    {
                        Binary.SetBit(ref IO.IF, 1, false);
                        PC = 0x48;
                    }

                    // Timer interrupt
                    else if (Binary.ReadBit(IEnIF, 2))
                    {
                        Binary.SetBit(ref IO.IF, 2, false);
                        PC = 0x50;
                    }

                    // Serial interrupt
                    else if (Binary.ReadBit(IEnIF, 3))
                    {
                        Binary.SetBit(ref IO.IF, 3, false);
                        PC = 0x58;
                    }

                    // Joypad interrupt
                    else if (Binary.ReadBit(IEnIF, 4))
                    {
                        Binary.SetBit(ref IO.IF, 4, false);
                        PC = 0x60;
                    }

                    //Halt = false;
                    //Stop = false;
                    IME = -1;
                }

                if (IME > 0)
                IME--;
            }
        }

        #endregion

        #region Instructions set

            /* LEGEND
             * Rw = Write register 8 bit
             * Rr = Read register 8 bit
             * RRw = Write register 16 bit
             * RRr = Read register 16 bit
             * Rmr = Read register 8 bit most significant bit from 16 bits
             * Rlr = Read register 8 bit least significant bit from 16 bits
             * Rmw = Write register 8 bit most significant bit from 16 bits
             * Rlw = Write register 8 bit least significant bit from 16 bits
             * en = Immediate data signed (read/write 8 bit ram signed at PC++)
             * n = Immediate data (read/write 8 bit ram at PC++)
             * nn = Immediate data (read/write 16 bit ram ((PC+2) << 8 | (PC+1)))
             * HLn = HL data (read/write ram at HL)
             * RRn = Register 16 bit data (read/write ram at register 16 bit)
             * Cn = 0xFF00+C data (read/write ram at 0xFFCC)
             * Nn = 0xFF00+n data (read/write ram at 0xFFNN)
             * Nnn = Immediate data (read/write 8 bit ram at ((PC+2) << 8 | (PC+1)));
             * HLnm = HL data and decrement (read/write ram at HL then decrement HL)
             * HLnp = HL data and increment (read/write ram at HL then increment HL)
             * CC = Condition
             */

            #region Main instructions

            /// <summary>
            ///     <c>LD r, r’: Load register (register) | Cycle = 1, Byte = 1, Flag = no</c>
            ///     <para>8-bit load instructions transfer one byte of data between two 8-bit registers,
            ///     or between one 8-bit register and location in memory.</para>
            /// </summary>
            private void LD_Rw_Rr(ref byte Rw, byte Rr)
            {
                Cycles++;
                Rw = Rr;
            }

            /// <summary>
            ///     <c>LD r, n: Load register (immediate) | Cycle = 2, Byte = 2, Flag = no</c>
            ///     <para>Load to the 8-bit register r, the immediate data n.</para>
            /// </summary>
            private void LD_Rw_n(ref byte Rw)
            {
                Cycles += 2;
                Rw = Read(PC++);
            }

            /// <summary>
            ///     <c>LD r, (HL): Load register (indirect HL) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 8-bit register r, data from the absolute address specified by the 
            ///     16-bit register HL.</para>
            /// </summary>
            private void LD_Rw_HLn(ref byte Rw)
            {
                Cycles += 2;
                Rw = Read(Binary.U16(L, H));
            }

            /// <summary>
            ///     <c>LD (HL), r: Load from register (indirect HL) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit register HL, data from 
            ///     the 8-bit register r.</para>
            /// </summary>
            private void LD_HLn_Rr(byte Rr)
            {
                Cycles += 2;
                Write(Binary.U16(L, H), Rr);
            }

            /// <summary>
            ///     <c>LD (HL), n: Load from immediate data (indirect HL) | Cycle = 3, Byte = 2, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit register HL, 
            ///     the immediate data n.</para>
            /// </summary>
            private void LD_HLn_n()
            {
                Cycles += 3;
                Write(Binary.U16(L, H), Read(PC++));
            }

            /// <summary>
            ///     <c>LD A, (RR): Load accumulator (indirect BC) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the absolute address specified by 
            ///     the 16-bit register BC.</para>
            /// </summary>
            private void LD_A_RRn(byte Rmr, byte Rlr)
            {
                Cycles += 2;
                A = Read(Binary.U16(Rlr, Rmr));
            }

            /// <summary>
            ///     <c>LD (RR), A: Load from accumulator (indirect BC) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit register BC, data 
            ///     from the 8-bit A register.</para>
            /// </summary>
            private void LD_RRn_A(byte Rmr, byte Rlr)
            {
                Cycles += 2;
                Write(Binary.U16(Rlr, Rmr), A);
            }

            /// <summary>
            ///     <c>LD A, (nn): Load accumulator (direct) | Cycle = 4, Byte = 3, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the absolute address 
            ///     specified by the 16-bit operand nn.</para>
            /// </summary>
            private void LD_A_nn()
            {
                Cycles += 4;
                A = Read(Binary.U16(Read(PC++), Read(PC++)));
            }

            /// <summary>
            ///     <c>LD (nn), A: Load from accumulator (direct) | Cycle = 4, Byte = 3, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit operand nn, data 
            ///     from the 8-bit A register.</para>
            /// </summary>
            private void LD_nn_A()
            {
                Cycles += 4;
                Write(Binary.U16(Read(PC++), Read(PC++)), A);
            }

            /// <summary>
            ///     <c>LDH A, (C): Load accumulator (indirect 0xFF00+C) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the address specified by the 8-bit C</para>
            ///     <para>register. The full 16-bit absolute address is obtained by setting the most</para>
            ///     <para>significant byte to 0xFF and the least significant byte to the value of C,</para>
            ///     <para>so the possible range is 0xFF00-0xFFFF.</para>
            /// </summary>
            private void LDH_A_Cn()
            {
                Cycles += 2;
                A = Read(Binary.U16(C, 0xFF));
            }

            /// <summary>
            ///     <c>LDH (C), A: Load from accumulator (indirect 0xFF00+C) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the address specified by the 8-bit C register, data from the 8-bit</para>
            ///     <para>A register. The full 16-bit absolute address is obtained by setting the most</para>
            ///     <para>significant byte to 0xFF and the least significant byte to the value of C,</para>
            ///     <para>so the possible range is 0xFF00-0xFFFF.</para>
            /// </summary>
            private void LDH_Cn_A()
            {
                Cycles += 2;
                Write(Binary.U16(C, 0xFF), A);
            }

            /// <summary>
            ///     <c>LDH A, (n): Load accumulator (direct 0xFF00+n) | Cycle = 3, Byte = 2, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the address specified by the 8-bit</para>
            ///     <para>immediate data n. The full 16-bit absolute address is obtained by setting</para>
            ///     <para>the most significant byte to 0xFF and the least significant byte to the</para>
            ///     <para>value of n, so the possible range is 0xFF00-0xFFFF.</para>
            /// </summary>
            private void LDH_A_Nn()
            {
                Cycles += 3;
                A = Read(Binary.U16(Read(PC++), 0xFF));
            }

            /// <summary>
            ///     <c>LDH (n), A: Load from accumulator (direct 0xFF00+n) | Cycle = 3, Byte = 2, Flag = no</c>
            ///     <para>Load to the address specified by the 8-bit immediate data n, data from the</para>
            ///     <para>8-bit A register. The full 16-bit absolute address is obtained by</para>
            ///     <para>the setting the most significant byte to 0xFF and the least significant byte</para>
            ///     <para>to value of n, so the possible range is 0xFF00-0xFFFF.</para>
            /// </summary>
            private void LDH_Nn_A()
            {
                Cycles += 3;
                Write(Binary.U16(Read(PC++), 0xFF), A);
            }

            /// <summary>
            ///     <c>LD A, (HL-): Load accumulator (indirect HL, decrement) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the absolute address specified by the</para>
            ///     <para>16-bit register HL. The value of HL is decremented after the memory read.</para>
            /// </summary>
            private void LD_A_HLnm()
            {
                Cycles += 2;

                ushort HL = Binary.U16(L, H);

                A = Read(HL--);
                H = Binary.Msb(HL);
                L = Binary.Lsb(HL);
            }

            /// <summary>
            ///     <c>LD (HL-), A: Load from accumulator (indirect HL, decrement) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit register HL, data from</para>
            ///     <para>the 8-bit A register. The value ofHL is decremented after the memory write.</para>
            /// </summary>
            private void LD_HLnm_A()
            {
                Cycles += 2;

                ushort HL = Binary.U16(L, H);

                Write(HL--, A);
                H = Binary.Msb(HL);
                L = Binary.Lsb(HL);
            }

            /// <summary>
            ///     <c>LD A, (HL+): Load accumulator (indirect HL, increment) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 8-bit A register, data from the absolute address specified by</para>
            ///     <para>the 16-bit register HL. The value of HL is incremented after the memory read.</para>
            /// </summary>
            private void LD_A_HLnp()
            {
                Cycles += 2;

                ushort HL = Binary.U16(L, H);

                A = Read(HL++);
                H = Binary.Msb(HL);
                L = Binary.Lsb(HL);
            }

            /// <summary>
            ///     <c>LD (HL+), A: Load from accumulator (indirect HL, increment) | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit register HL, data from</para>
            ///     <para>the 8-bit A register. The value of HL is incremented after the memory write.</para>
            /// </summary>
            private void LD_HLnp_A()
            {
                Cycles += 2;

                ushort HL = Binary.U16(L, H);

                Write(HL++, A);
                H = Binary.Msb(HL);
                L = Binary.Lsb(HL);
            }

            /// <summary>
            ///     <c>LD rr, nn: Load 16-bit register / register pair | Cycle = 3, Byte = 3, Flag = no</c>
            ///     <para>Load to the 16-bit register rr, the immediate 16-bit data nn.</para>
            /// </summary>
            private void LD_RRw_nn(ref byte Rmw, ref byte Rlw)
            {
                Cycles += 3;

                Rlw = Read(PC++);
                Rmw = Read(PC++);
            }

            /// <summary>
            ///     <para>Same LD_RRw_nn(ref byte Rmw, ref byte Rlw) but ref 16 bit only.</para>
            /// </summary>
            private void LD_RRw_nn(ref ushort RRw)
            {
                Cycles += 3;

                RRw = Binary.U16(Read(PC++), Read(PC++));
            }

            /// <summary>
            ///     <c>LD (nn), SP: Load from stack pointer (direct) | Cycle = 5, Byte = 3, Flag = no</c>
            ///     <para>Load to the absolute address specified by the 16-bit operand nn, data from the 16-bit SP register.</para>
            /// </summary>
            private void LD_Nnn_SP()
            {
                Cycles += 5;

                ushort nn = Binary.U16(Read(PC++), Read(PC++));

                Write(nn, Binary.Lsb(SP));
                Write(++nn, Binary.Msb(SP));
            }

            /// <summary>
            ///     <c>LD SP, HL: Load stack pointer from HL | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Load to the 16-bit SP register, data from the 16-bit HL register.</para>
            /// </summary>
            private void LD_SP_HL()
            {
                Cycles += 2;
                SP = Binary.U16(L, H);
            }

            /// <summary>
            ///     <c>LD HL, SP e | Cycle = 3, Byte = 2, Flag = ZNHC</c>
            ///     <para>Add the signed value e8 to SP and store the result in HL.</para>
            /// </summary>
            private void LD_HL_SP_en()
            {
                Cycles += 3;
                byte n = Read(PC++);
                sbyte e = unchecked((sbyte)n);

                ushort result = (ushort)(SP + e);

                H = Binary.Msb(result);
                L = Binary.Lsb(result);

                FlagZ = false;
                FlagN = false;
                FlagH = ((SP ^ e ^ result) & 0x10) == 0x10;
                FlagC = ((SP ^ e ^ result) & 0x100) == 0x100;
            }

            /// <summary>
            ///     <c>PUSH rr: Push to stack | Cycle = 4, Byte = 1, Flag = no</c>
            ///     <para>Push to the stack memory, data from the 16-bit register rr.</para>
            /// </summary>
            private void PUSH_RR(byte Rmr, byte Rlr)
            {
                Cycles += 4;

                Write(--SP, Rmr);
                Write(--SP, Rlr);
            }

            /// <summary>
            ///     <c>Same PUSH rr but with AF | Cycle = 4, Byte = 1, Flag = no</c>
            /// </summary>
            private void PUSH_AF()
            {
                Cycles += 4;

                byte F = 0;

                if (FlagZ) Binary.SetBit(ref F, 7, true);
                if (FlagN) Binary.SetBit(ref F, 6, true);
                if (FlagH) Binary.SetBit(ref F, 5, true);
                if (FlagC) Binary.SetBit(ref F, 4, true);

                Write(--SP, A);
                Write(--SP, F);
            }

            /// <summary>
            ///     <c>POP rr: Pop from stack | Cycle = 3, Byte = 1, Flag = no</c>
            ///     <para>Pops to the 16-bit register rr, data from the stack memory.This instruction</para>
            ///     <para>does not do calculations that affect flags, but POP AF completely replaces the</para>
            ///     <para>F register value, so all flags are changed based on the 8-bit data that is read</para>
            ///     <para>from memory.</para>
            /// </summary>
            private void POP_RR(ref byte Rmw, ref byte Rlw)
            {
                Cycles += 3;

                Rlw = Read(SP++);
                Rmw = Read(SP++);
            }

            /// <summary>
            ///     <c>Same POP rr | Cycle = 3, Byte = 1, Flag = no</c>
            /// </summary>
            private void POP_AF()
            {
                Cycles += 3;

                byte F = Read(SP++);

                FlagZ = Binary.ReadBit(F, 7);
                FlagN = Binary.ReadBit(F, 6);
                FlagH = Binary.ReadBit(F, 5);
                FlagC = Binary.ReadBit(F, 4);

                A = Read(SP++);
            }

            /// <summary>
            ///     <c>ADD r: Add (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, the 8-bit register r, and stores the result</para>
            ///     <para>back into the A register.</para>
            /// </summary>
            private void ADD_Rr(byte Rr)
            {
                Cycles++;

                byte _A = A;
                ushort result = (ushort)(A + Rr);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (Rr & 0xF) > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>ADD (HL): Add (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, data from the absolute address specified by the</para>
            ///     <para>16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void ADD_HLn()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(Binary.U16(L, H));
                ushort result = (ushort)(A + n);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (n & 0xF) > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>ADD n: Add (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, the immediate data n, and stores the result</para>
            ///     <para>back into the A register.</para>
            /// </summary>
            private void ADD_n()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(PC++);
                ushort result = (ushort)(A + n);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (n & 0xF) > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>ADD SP e | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Add the signed value e8 to SP.</para>
            /// </summary>
            private void ADD_SP_en()
            {
                Cycles += 4;

                byte n = Read(PC++);
                sbyte e = unchecked((sbyte)n);
                int result = SP + e;

                FlagZ = false;
                FlagN = false;
                FlagH = (((SP & 0xF) + (e & 0xF)) & 0x10) == 0x10;
                FlagC = (((SP & 0xFF) + (e & 0xFF)) & 0x100) == 0x100;

                SP = (ushort)result;
            }

            /// <summary>
            ///     <c>ADD HL rr | Cycle = 2, Byte = 1, Flag = NHC</c>
            ///     <para>Add the value in r16 to HL.</para>
            /// </summary>
            private void ADD_HL_RRr(byte Rmr, byte Rlr)
            {
                Cycles += 2;
                ushort HL = Binary.U16(L, H);
                ushort RR = Binary.U16(Rlr, Rmr);
                int result = HL + RR;

                H = Binary.Msb((ushort)result);
                L = Binary.Lsb((ushort)result);

                FlagN = false;
                FlagH = ((HL ^ RR ^ (ushort)result) & 0x1000) != 0;
                FlagC = (result & 0x10000) == 0x10000;
            }
        
            /// <summary>
            ///     <c>Same ADD HL rr but with SP | Cycle = 2, Byte = 1, Flag = NHC</c>
            /// </summary>
            private void ADD_HL_SP()
            {
                Cycles += 2;
                ushort HL = Binary.U16(L, H);
                int result = HL + SP;

                H = Binary.Msb((ushort)result);
                L = Binary.Lsb((ushort)result);

                FlagN = false;
                FlagH = ((HL ^ SP ^ (ushort)result) & 0x1000) == 0x1000;
                FlagC = (result & 0x10000) == 0x10000;
            }

            /// <summary>
            ///     <c>ADC r: Add with carry (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, the carry flag and the 8-bit register r, and</para>
            ///     <para>stores the result back into the A register.</para>
            /// </summary>
            private void ADC_Rr(byte Rr)
            {
                Cycles++;

                byte _A = A;
                byte carry = (byte)(FlagC ? 1 : 0);
                ushort result = (ushort)(A + Rr + carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (Rr & 0xF) + carry > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>ADC (HL): Add with carry (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, the carry flag and data from the absolute address</para>
            ///     <para>specified by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void ADC_HLn()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(Binary.U16(L, H));
                byte carry = (byte)(FlagC ? 1 : 0);
                ushort result = (ushort)(A + n + carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (n & 0xF) + carry > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>ADC n: Add with carry (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Adds to the 8-bit A register, the carry flag and the immediate data n, and</para>
            ///     <para>stores the result back into the A register.</para>
            /// </summary>
            private void ADC_n()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(PC++);
                byte carry = (byte)(FlagC ? 1 : 0);
                ushort result = (ushort)(A + n + carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = (_A & 0xF) + (n & 0xF) + carry > 0xF;
                FlagC = result > 0xFF;
            }

            /// <summary>
            ///     <c>SUB r: Subtract (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the 8-bit register r, and stores the</para>
            ///     <para>result back into the A register.</para>
            /// </summary>
            private void SUB_Rr(byte Rr)
            {
                Cycles++;

                byte _A = A;
                short result = (short)(A - Rr);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>SUB (HL): Subtract (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, data from the absolute address specified</para>
            ///     <para>by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void SUB_HLn()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(Binary.U16(L, H));
                short result = (short)(A - n);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>SUB n: Subtract (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the immediate data n, and stores the result</para>
            ///     <para>back into the A register.</para>
            /// </summary>
            private void SUB_n()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(PC++);
                short result = (short)(A - n);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>SBC r: Subtract with carry (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the carry flag and the 8-bit register r, and stores</para>
            ///     <para>the result back into the A register.</para>
            /// </summary>
            private void SBC_Rr(byte Rr)
            {
                Cycles++;

                byte _A = A;
                byte carry = (byte)(FlagC ? 1 : 0);
                short result = (short)(A - Rr - carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) - carry < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>SBC (HL): Subtract with carry (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the carry flag and data from the absolute address</para>
            ///     <para>specified by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void SBC_HLn()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(Binary.U16(L, H));
                byte carry = (byte)(FlagC ? 1 : 0);
                short result = (short)(A - n - carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) - carry < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>SBC n: Subtract with carry (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the carry flag and the immediate data n, and stores</para>
            ///     <para>the result back into the A register.</para>
            /// </summary>
            private void SBC_n()
            {
                Cycles += 2;

                byte _A = A;
                byte n = Read(PC++);
                byte carry = (byte)(FlagC ? 1 : 0);
                short result = (short)(A - n - carry);
                A = (byte)result;

                FlagZ = A == 0;
                FlagN = true;
                FlagH = (_A & 0xF) - (result & 0xF) - carry < 0;
                FlagC = result < 0;
            }

            /// <summary>
            ///     <c>CP r: Compare (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the 8-bit register r, and updates flags based on</para>
            ///     <para>the result. This instruction is basically identical to SUB r, but does not update the A register.</para>
            /// </summary>
            private void CP_Rr(byte Rr)
            {
                Cycles++;

                ushort result = (ushort)(A - Rr);

                FlagZ = result == 0;
                FlagN = true;
                FlagH = (A & 0xF) < (result & 0xF);
                FlagC = Rr > A;
            }

            /// <summary>
            ///     <c>CP (HL): Compare (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, data from the absolute address specified by the 16-bit</para>
            ///     <para>register HL, and updates flags based on the result.This instruction is basically identical to SUB</para>
            ///     <para>(HL), but does not update the A register.</para>
            /// </summary>
            private void CP_HLn()
            {
                Cycles += 2;

                byte HL = Read(Binary.U16(L, H));
                ushort result = (ushort)(A - HL);

                FlagZ = result == 0;
                FlagN = true;
                FlagH = (A & 0xF) < (result & 0xF);
                FlagC = HL > A;
            }

            /// <summary>
            ///     <c>CP n: Compare (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Subtracts from the 8-bit A register, the immediate data n, and updates flags based on the result. This</para>
            ///     <para>instruction is basically identical to SUB n, but does not update the A register.</para>
            /// </summary>
            private void CP_n()
            {
                Cycles += 2;

                byte n = Read(PC++);
                ushort result = (ushort)(A - n);

                FlagZ = result == 0;
                FlagN = true;
                FlagH = (A & 0xF) < (result & 0xF);
                FlagC = n > A;
            }

            /// <summary>
            ///     <c>INC r: Increment (register) | Cycle = 1, Byte = 1, Flag = ZNH</c>
            ///     <para>Increments data in the 8-bit register r.</para>
            /// </summary>
            private void INC_Rw(ref byte Rw)
            {
                Cycles++;

                Rw++;

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = (Rw & 0xF) == 0;
            }

            /// <summary>
            ///     <c>INC (HL): Increment (indirect HL) | Cycle = 3, Byte = 1, Flag = ZNH</c>
            ///     <para>Increments data at the absolute address specified by the 16-bit register HL.</para>
            /// </summary>
            private void INC_HLn()
            {
                Cycles += 3;

                ushort HL = Binary.U16(L, H);
                byte data = Read(Binary.U16(L, H));
                Write(HL, ++data);

                FlagZ = data == 0;
                FlagN = false;
                FlagH = (data & 0xF) == 0;
            }

            /// <summary>
            ///     <c>INC rr | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Increment value in register RR by 1.</para>
            /// </summary>
            private void INC_RRw(ref byte Rmw, ref byte Rlw)
            {
                Cycles += 2;

                ushort RR = (ushort)(Binary.U16(Rlw, Rmw) + 1);

                Rmw = Binary.Msb(RR);
                Rlw = Binary.Lsb(RR);
            }

            /// <summary>
            ///     <c>Same INC rr but with 16 bit register | Cycle = 2, Byte = 1, Flag = no</c>
            /// </summary>
            private void INC_RRw(ref ushort RRw)
            {
                Cycles += 2;
                RRw++;
            }

            /// <summary>
            ///     <c>DEC r: Decrement (register) | Cycle = 1, Byte = 1, Flag = ZNH</c>
            ///     <para>Decrements data in the 8-bit register r.</para>
            /// </summary>
            private void DEC_Rw(ref byte Rw)
            {
                Cycles++;

                Rw--;

                FlagZ = Rw == 0;
                FlagN = true;
                FlagH = (Rw & 0xF) == 0xF;
            }

            /// <summary>
            ///     <c>DEC (HL): Decrement (indirect HL) | Cycle = 3, Byte = 1, Flag = ZNH</c>
            ///     <para>Decrements data at the absolute address specified by the 16-bit register HL.</para>
            /// </summary>
            private void DEC_HLn()
            {
                Cycles += 3;

                byte HLn = Read(Binary.U16(L, H));

                byte data = (byte)(HLn - 1);
                Write(Binary.U16(L, H), data);

                FlagZ = data == 0;
                FlagN = true;
                FlagH = (data & 0xF) == 0xF;
            }

            /// <summary>
            ///     <c>DEC rr | Cycle = 2, Byte = 1, Flag = no</c>
            ///     <para>Decrement value in register rr by 1.</para>
            /// </summary>
            private void DEC_RRw(ref byte Rmw, ref byte Rlw)
            {
                Cycles += 2;

                ushort RR = (ushort)(Binary.U16(Rlw, Rmw) - 1);

                Rmw = Binary.Msb(RR);
                Rlw = Binary.Lsb(RR);
            }

            /// <summary>
            ///     <c>Same DEC rr but with 16 bit register | Cycle = 2, Byte = 1, Flag = no</c>
            /// </summary>
            private void DEC_RRw(ref ushort RRw)
            {
                Cycles += 2;
                RRw--;
            }

            /// <summary>
            ///     <c>AND r: Bitwise AND (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise AND operation between the 8-bit A register and the 8-bit register r,</para>
            ///     <para>and stores the result back into the A register.</para>
            /// </summary>
            private void AND_Rr(byte Rr)
            {
                Cycles++;

                A &= Rr;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = true;
                FlagC = false;
            }

            /// <summary>
            ///     <c>AND (HL): Bitwise AND (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise AND operation between the 8-bit A register and data from the absolute</para>
            ///     <para>address specified by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void AND_HLn()
            {
                Cycles += 2;

                A &= Read(Binary.U16(L, H));

                FlagZ = A == 0;
                FlagN = false;
                FlagH = true;
                FlagC = false;
            }

            /// <summary>
            ///     <c>AND n: Bitwise AND (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Performs a bitwise AND operation between the 8-bit A register and immediate data n, and stores</para>
            ///     <para>the result back into the A register.</para>
            /// </summary>
            private void AND_n()
            {
                Cycles += 2;

                A &= Read(PC++);

                FlagZ = A == 0;
                FlagN = false;
                FlagH = true;
                FlagC = false;
            }

            /// <summary>
            ///     <c>OR r: Bitwise OR (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise OR operation between the 8-bit A register and the 8-bit register r, and</para>
            ///     <para>stores the result back into the A register</para>
            /// </summary>
            private void OR_Rr(byte Rr)
            {
                Cycles++;

                A |= Rr;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>OR (HL): Bitwise OR (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise AND operation between the 8-bit A register and data from the absolute</para>
            ///     <para>address specified by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void OR_HLn()
            {
                Cycles += 2;

                A |= Read(Binary.U16(L, H));

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>OR n: Bitwise OR (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Performs a bitwise OR operation between the 8-bit A register and immediate data n, and stores</para>
            ///     <para>the result back into the A register.</para>
            /// </summary>
            private void OR_n()
            {
                Cycles += 2;

                A |= Read(PC++);

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>XOR r: Bitwise XOR (register) | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise XOR operation between the 8-bit A register and the 8-bit register r, and</para>
            ///     <para>stores the result back into the A register</para>
            /// </summary>
            private void XOR_Rr(byte Rr)
            {
                Cycles++;

                A ^= Rr;

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>XOR (HL): Bitwise XOR (indirect HL) | Cycle = 2, Byte = 1, Flag = ZNHC</c>
            ///     <para>Performs a bitwise XOR operation between the 8-bit A register and data from the absolute</para>
            ///     <para>address specified by the 16-bit register HL, and stores the result back into the A register.</para>
            /// </summary>
            private void XOR_HLn()
            {
                Cycles += 2;

                A ^= Read(Binary.U16(L, H));

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>XOR n: Bitwise XOR (immediate) | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Performs a bitwise XOR operation between the 8-bit A register and immediate data n, and stores</para>
            ///     <para>the result back into the A register.</para>
            /// </summary>
            private void XOR_n()
            {
                Cycles += 2;

                A ^= Read(PC++);

                FlagZ = A == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>CCF: Complement carry flag | Cycle = 1, Byte = 1, Flag = NHC</c>
            ///     <para>Flips the carry flag, and clears the N and H flags.</para>
            /// </summary>
            private void CCF()
            {
                Cycles++;

                FlagN = false;
                FlagH = false;
                FlagC = !FlagC;
            }

            /// <summary>
            ///     <c>SCF: Set carry flag | Cycle = 1, Byte = 1, Flag = NHC</c>
            ///     <para>Sets the carry flag, and clears the N and H flags.</para>
            /// </summary>
            private void SCF()
            {
                Cycles++;

                FlagN = false;
                FlagH = false;
                FlagC = true;
            }

            /// <summary>
            ///     <c>DAA: Decimal adjust accumulator | Cycle = 1, Byte = 1, Flag = ZHC</c>
            ///     <para>Decimal Adjust Accumulator to get a correct BCD representation after an arithmetic instruction.</para>
            /// </summary>
            private void DAA()
            {
                Cycles++;

                ushort AdjustA = A;
                byte lsb = Binary.Lsb(A);

                if (!FlagN)
                {
                    if (FlagH || lsb > 9)
                    AdjustA += 0x6;

                    if (FlagC || AdjustA > 0x9F)
                    AdjustA += 0x60;
                }
                else
                {
                    if (FlagH)
                    AdjustA = (byte)(AdjustA - 6);

                    if (FlagC)
                    AdjustA -= 0x60;
                }

                A = (byte)AdjustA;

                FlagZ = A == 0;
                FlagH = false;
                FlagC = (AdjustA & 0x100) == 0x100 || FlagC;
            }

            /// <summary>
            ///     <c>CPL: Complement accumulator | Cycle = 1, Byte = 1, Flag = NH</c>
            ///     <para>Flips all the bits in the 8-bit A register, and sets the N and H flags.</para>
            /// </summary>
            private void CPL()
            {
                Cycles++;
                A = (byte)~A;

                FlagN = true;
                FlagH = true;
            }

            /// <summary>
            ///     <c>JP nn: Jump | Cycle = 4, Byte = 3, Flag = no</c>
            ///     <para>Unconditional jump to the absolute address specified by the 16-bit operand nn.</para>
            /// </summary>
            private void JP_nn()
            {
                Cycles += 4;
                PC = Binary.U16(Read(PC++), Read(PC++));
            }

            /// <summary>
            ///     <c>JP HL: Jump to HL | Cycle = 1, Byte = 1, Flag = no</c>
            ///     <para>Unconditional jump to the absolute address specified by the 16-bit register HL.</para>
            /// </summary>
            private void JP_HL()
            {
                Cycles++;
                PC = Binary.U16(L, H);
            }

            /// <summary>
            ///     <c>JP cc, nn: Jump (conditional) | Cycle = 4/3, Byte = 3, Flag = no</c>
            ///     <para>Conditional jump to the absolute address specified by the 16-bit operand nn, depending on the</para>
            ///     <para>condition cc. Note that the operand(absolute address) is read even when the condition is false!</para>
            /// </summary>
            private void JP_CC_nn(bool CC)
            {
                Cycles += (byte)(CC ? 4 : 3);
                ushort nn = Binary.U16(Read(PC++), Read(PC++));

                if (CC)
                PC = nn;
            }

            /// <summary>
            ///     <c>JR e: Relative jump | Cycle = 3, Byte = 2, Flag = no</c>
            ///     <para>Unconditional jump to the relative address specified by the signed 8-bit operand e.</para>
            /// </summary>
            private void JR_en()
            {
                Cycles += 3;
                sbyte e = unchecked((sbyte)Read(PC++));
                PC = (ushort)(PC + e);
            }

            /// <summary>
            ///     <c>JR cc, e: Relative jump (conditional) | Cycle = 3/2, Byte = 2, Flag = no</c>
            ///     <para>Conditional jump to the relative address specified by the signed 8-bit operand e, depending on the condition cc.</para>
            ///     <para>Note that the operand(relative address offset) is read even when the condition is false!</para>
            /// </summary>
            private void JR_CC_en(bool CC)
            {
                Cycles += (byte)(CC ? 3 : 2);

                sbyte e = unchecked((sbyte)Read(PC++));

                if (CC)
                PC = (ushort)(PC + e);
            }

            /// <summary>
            ///     <c>CALL nn: Call function | Cycle = 6, Byte = 3, Flag = no</c>
            ///     <para>Unconditional function call to the absolute address specified by the 16-bit operand nn.</para>
            /// </summary>
            private void CALL_nn()
            {
                Cycles += 6;

                ushort nn = Binary.U16(Read(PC++), Read(PC++));

                Write(--SP, Binary.Msb(PC));
                Write(--SP, Binary.Lsb(PC));
                PC = nn;
            }

            /// <summary>
            ///     <c>CALL cc, nn: Call function (conditional) | Cycle = 6/3, Byte = 3, Flag = no</c>
            ///     <para>Conditional function call to the absolute address specified by the 16-bit operand nn,</para>
            ///     <para>depending on the condition cc. Note that the operand(absolute address) is read even when the</para>
            ///     <para>condition is false!</para>
            /// </summary>
            private void CALL_CC_nn(bool CC)
            {
                Cycles += (byte)(CC ? 6 : 3);
                byte lsb = Read(PC++);
                byte msb = Read(PC++);

                if (CC)
                {
                    Write(--SP, Binary.Msb(PC));
                    Write(--SP, Binary.Lsb(PC));

                    PC = Binary.U16(lsb, msb);
                }
            }

            /// <summary>
            ///     <c>RET: Return from function | Cycle = 4, Byte = 1, Flag = no</c>
            ///     <para>Unconditional return from a function.</para>
            /// </summary>
            private void RET()
            {
                Cycles += 4;
                PC = Binary.U16(Read(SP++), Read(SP++));
            }

            /// <summary>
            ///     <c>RET cc: Return from function (conditional) | Cycle = 5/2, Byte = 1, Flag = no</c>
            ///     <para>Conditional return from a function, depending on the condition cc.</para>
            /// </summary>
            private void RET_CC(bool CC)
            {
                Cycles += (byte)(CC ? 5 : 2);

                if (CC)
                PC = Binary.U16(Read(SP++), Read(SP++));
            }

            /// <summary>
            ///     <c>RETI: Return from interrupt handler | Cycle = 4, Byte = 1, Flag = no</c>
            ///     <para>Unconditional return from a function. Also enables interrupts by setting IME=1.</para>
            /// </summary>
            private void RETI()
            {
                Cycles += 4;

                PC = Binary.U16(Read(SP++), Read(SP++));
                IME = 0;
            }

            /// <summary>
            ///     <c>RST n: Restart / Call function (implied) | Cycle = 4, Byte = 1, Flag = no</c>
            ///     <para>Unconditional function call to the absolute fixed address defined by the opcode.</para>
            /// </summary>
            private void RST_n(byte at)
            {
                Cycles += 4;

                Write(--SP, Binary.Msb(PC));
                Write(--SP, Binary.Lsb(PC));

                PC = at;
            }

            /// <summary>
            ///     <c>HALT: Halt system clock | Cycle = 0, Byte = 1, Flag = no</c>
            ///     <para>Enter CPU low-power consumption mode until an interrupt occurs. The exact behavior of</para>
            ///     <para>this instruction depends on the state of the IME flag.</para>
            /// </summary>
            private void HALT()
            {
                Halt = true;
    /*            if (IME < 0)
                {
                    Halt = true;
                }
                else
                {
                    // Halt bug
                    if ((IO.IE & IO.IF) != 0 && IME == 0)
                    HaltBug = true;
                }*/

    /*            if (IME >= 0)
                {
                    IME = 0;
                    PC--;
                }
                else
                {
                    Halt = true;

                    if ((IO.IE & IO.IF) != 0)
                    HaltBug = true;
                }*/
            }

            /// <summary>
            ///     <c>STOP: Stop system and main clocks | Cycle = 0, Byte = 2, Flag = no</c>
            ///     <para>Enter CPU very low power mode. Also used to switch between double and normal speed CPU modes in GBC.</para>
            /// </summary>
            private void STOP()
            {
                //Console.WriteLine("INTR STOP");
                //IO.DIV = 0x00;
                Stop = true;
            }

            /// <summary>
            ///     <c>DI: Disable interrupts | Cycle = 1, Byte = 1, Flag = no</c>
            ///     <para>Disables interrupt handling by setting IME=0 and cancelling any scheduled effects of</para>
            ///     <para>the EI instruction if any.</para>
            /// </summary>
            private void DI()
            {
                Cycles++;
                IME = -1;
            }

            /// <summary>
            ///     <c>EI: Enable interrupts | Cycle = 1, Byte = 1, Flag = no</c>
            ///     <para>Schedules interrupt handling to be enabled after the next machine cycle.</para>
            /// </summary>
            private void EI()
            {
                Cycles++;
                IME = 1;
            }

            /// <summary>
            ///     <c>NOP: No operation | Cycle = 1, Byte = 1, Flag = no</c>
            ///     <para>No operation. This instruction doesn’t do anything, but can be used to add a delay of one</para>
            ///     <para>machine cycle and increment PC by one.</para>
            /// </summary>
            private void NOP()
            {
                Cycles++;
            }

            /// <summary>
            ///     <c>Unknown | Cycle = ?, Byte = 1, Flag = no</c>
            /// </summary>
            private static void Unknown()
            {
                //Cycles++;
            }

            /// <summary>
            ///     <c>RLCA | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Rotate Left through Carry Accumulator : 000c rotate akku left</para>
            /// </summary>
            private void RLCA()
            {
                Cycles++;

                byte _A = A;
                A = (byte)((A << 1) | (A >> 7));

                FlagZ = false;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_A, 7);
            }

            /// <summary>
            ///     <c>RLA | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Rotate Left through Accumulator : 000c rotate akku left through carry</para>
            /// </summary>
            private void RLA()
            {
                Cycles++;

                byte _A = A;
                A <<= 1;

                Binary.SetBit(ref A, 0, FlagC);

                FlagZ = false;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_A, 7);
            }

            /// <summary>
            ///     <c>RRCA | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Rotate Right through Carry Accumulator : 000c rotate akku right</para>
            /// </summary>
            private void RRCA()
            {
                Cycles++;

                byte _A = A;
                A = (byte)((A >> 1) | ((A & 0x01) << 7));

                FlagZ = false;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_A, 0);
            }

            /// <summary>
            ///     <c>RRA | Cycle = 1, Byte = 1, Flag = ZNHC</c>
            ///     <para>Rotate Right through Accumulator : 000c rotate akku right through carry</para>
            /// </summary>
            private void RRA()
            {
                Cycles++;

                byte _A = A;
                A >>= 1;

                Binary.SetBit(ref A, 7, FlagC);

                FlagZ = false;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_A, 0);
            }

            /// <summary>
            ///     <c>CB operation | Cycle = 0, Byte = 1, Flag = no</c>
            ///     <para>Execute opcode with CB prefix</para>
            /// </summary>
            private void CB_op()
            {
                CB_Instructions[Read(PC++)]?.Invoke();
            }

        #endregion

            #region CB prefixed instructions

            /// <summary>
            ///     <c>RLC r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Left through Carry.</para>
            /// </summary>
            private void RLC_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw = (byte)((Rw << 1) | (Rw >> 7));

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 7);
            }

            /// <summary>
            ///     <c>LC (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Left through Carry with direct memory at HL.</para>
            /// </summary>
            private void RLC_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)((data << 1) | (data >> 7));
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 7);
            }

            /// <summary>
            ///     <c>RRC r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Right through Carry.</para>
            /// </summary>
            private void RRC_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw = (byte)((Rw >> 1) | ((Rw & 0x01) << 7));

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 0);
            }

            /// <summary>
            ///     <c>RRC (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Right through Carry with direct memory at HL.</para>
            /// </summary>
            private void RRC_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)((data >> 1) | ((data & 0x01) << 7));
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 0);
            }

            /// <summary>
            ///     <c>RL r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Left through.</para>
            /// </summary>
            private void RL_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw <<= 1;

                Binary.SetBit(ref Rw, 0, FlagC);

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 7);
            }

            /// <summary>
            ///     <c>RL (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Left through with direct memory at HL.</para>
            /// </summary>
            private void RL_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)(data << 1);

                Binary.SetBit(ref newData, 0, FlagC);
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 7);
            }

            /// <summary>
            ///     <c>RR r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Right through.</para>
            /// </summary>
            private void RR_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw >>= 1;

                Binary.SetBit(ref Rw, 7, FlagC);

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 0);
            }

            /// <summary>
            ///     <c>RR (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Rotate Right through with direct memory at HL.</para>
            /// </summary>
            private void RR_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)(data >> 1);

                Binary.SetBit(ref newData, 7, FlagC);
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 0);
            }

            /// <summary>
            ///     <c>SLA r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift Left Arithmetic.</para>
            /// </summary>
            private void SLA_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw = (byte)(Rw << 1);

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 7);
            }

            /// <summary>
            ///     <c>SLA (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift Left Arithmetic with direct memory at HL.</para>
            /// </summary>
            private void SLA_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)(data << 1);
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 7);
            }

            /// <summary>
            ///     <c>SRA r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift Right Arithmetic.</para>
            /// </summary>
            private void SRA_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw = (byte)((Rw >> 1) | (Rw & 0x80));

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 0);
            }

            /// <summary>
            ///     <c>SRA (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift Right Arithmetic with direct memory at HL.</para>
            /// </summary>
            private void SRA_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)((data >> 1) | (data & 0x80));
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 0);
            }

            /// <summary>
            ///     <c>SWAP r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Exchange low/hi-nibble.</para>
            /// </summary>
            private void SWAP_Rw(ref byte Rw)
            {
                Cycles += 2;

                Rw = (byte)(((Rw & 0x0F) << 4) | ((Rw & 0xF0) >> 4));

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>SWAP (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Exchange low/hi-nibble with direct memory at HL.</para>
            /// </summary>
            private void SWAP_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)(((data & 0x0F) << 4) | ((data & 0xF0) >> 4));
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = false;
            }

            /// <summary>
            ///     <c>SRL r | Cycle = 2, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift right logical.</para>
            /// </summary>
            private void SRL_Rw(ref byte Rw)
            {
                Cycles += 2;

                byte _R = Rw;
                Rw = (byte)(Rw >> 1);

                FlagZ = Rw == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(_R, 0);
            }

            /// <summary>
            ///     <c>SRL (HL) | Cycle = 4, Byte = 2, Flag = ZNHC</c>
            ///     <para>Shift right logical with direct memory at HL.</para>
            /// </summary>
            private void SRL_HLn()
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                byte newData = (byte)(data >> 1);
                Write(HL, newData);

                FlagZ = newData == 0;
                FlagN = false;
                FlagH = false;
                FlagC = Binary.ReadBit(data, 0);
            }

            /// <summary>
            ///     <c>BIT n,r | Cycle = 2, Byte = 2, Flag = ZNH</c>
            ///     <para>Test bit n.</para>
            /// </summary>
            private void BIT_n_r(byte n, byte Rr)
            {
                Cycles += 2;

                FlagZ = !Binary.ReadBit(Rr, n);
                FlagN = false;
                FlagH = true;
            }

            /// <summary>
            ///     <c>BIT n,(HL) | Cycle = 3, Byte = 2, Flag = ZNH</c>
            ///     <para>Test bit n with direct memory at HL.</para>
            /// </summary>
            private void BIT_n_HLn(byte n)
            {
                Cycles += 3;

                FlagZ = !Binary.ReadBit(Read(Binary.U16(L, H)), n);
                FlagN = false;
                FlagH = true;
            }

            /// <summary>
            ///     <c>SET n,r | Cycle = 2, Byte = 2, Flag = no</c>
            ///     <para>Set bit n.</para>
            /// </summary>
            private void SET_n_r(byte n, ref byte Rw)
            {
                Cycles += 2;
                Binary.SetBit(ref Rw, n, true);
            }

            /// <summary>
            ///     <c>SET n,(HL) | Cycle = 4, Byte = 2, Flag = no</c>
            ///     <para>Set bit n with direct memory at HL.</para>
            /// </summary>
            private void SET_n_HLn(byte n)
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                Binary.SetBit(ref data, n, true);
                Write(HL, data);
            }

            /// <summary>
            ///     <c>RES n,r | Cycle = 2, Byte = 2, Flag = no</c>
            ///     <para>Unset bit n.</para>
            /// </summary>
            private void RES_n_r(byte n, ref byte Rw)
            {
                Cycles += 2;
                Binary.SetBit(ref Rw, n, false);
            }

            /// <summary>
            ///     <c>SRES n,(HL) | Cycle = 4, Byte = 2, Flag = no</c>
            ///     <para>Unset bit n with direct memory at HL.</para>
            /// </summary>
            private void RES_n_HLn(byte n)
            {
                Cycles += 4;

                ushort HL = Binary.U16(L, H);
                byte data = Read(HL);
                Binary.SetBit(ref data, n, false);
                Write(HL, data);
            }

            #endregion

        #endregion
    }
}