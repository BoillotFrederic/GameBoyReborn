// -------
// Drawing
// -------
using Raylib_cs;
using Emulator;
using static GameBoyReborn.Program;

namespace GameBoyReborn
{
    #region Handle screen

    public class Drawing
    {
        // Drawn screen data
        // -----------------
        private static readonly Color[] ScreenData = new Color[SystemWidth * SystemHeight];

        private static Image _ScreenImage;
        public static Image ScreenImage
        {
            get { return _ScreenImage; }
            set { _ScreenImage = value; }
        }

        // Draw screen
        private static int DrawWidth;
        private static int DrawHeight;

        public static void Screen()
        {
            // Clear
            Raylib.ClearBackground(Color.WHITE);

            // Draw screen
            DrawWidth = DebugEnable ? WindowWidth / 2 : WindowWidth;
            DrawHeight = DebugEnable ? WindowHeight / 2 : WindowHeight;
            UpdateScreenImage();
            Texture2D screenTexture = Raylib.LoadTextureFromImage(ScreenImage);
            screenTexture.Width = DrawWidth;
            screenTexture.Height = DrawHeight;
            Raylib.DrawTexture(screenTexture, 0, 0, Color.RAYWHITE);
            //UpdateAndDrawTexts();
            //ShowXboxButton();

            // Draw debug
            if (DebugEnable)
            ShowStates();

            Raylib.EndDrawing();
            Raylib.UnloadTexture(screenTexture);
        }

        // Update screen image
        private static void UpdateScreenImage()
        {
            unsafe
            {
                fixed (Color* pData = &ScreenData[0])
                {
                    _ScreenImage.Data = pData;
                }
            }
        }

        // Set pixel
        public static void SetPixel(byte x, byte y, Color? color)
        {
            if(color != null)
            ScreenData[y * SystemWidth + x] = (Color)color;
        }

        // Get pixel
        public static Color GetPixel(byte x, byte y)
        {
            return ScreenData[y * SystemWidth + x];
        }

        // Disable screen
        public static void DisableScreen()
        {
            Array.Clear(ScreenData);
        }

        #endregion

        #region Debug

        // Show states
        // ---------------

        private static void ShowStates()
        {
            if (Program.Emulation != null)
            {
                CPU CPU = Program.Emulation.CPU;
                IO IO = Program.Emulation.IO;
                Memory Memory = Program.Emulation.Memory;

                string InstructionName = CPU.Opcode != 0xCB ? InstructionsName[CPU.Opcode] : (Memory.Read(CPU.PC).ToString("X2") + " : " + CBInstructionsName[Memory.Read(CPU.PC)]);
                int sizeText = 20;
                int lineX = 10;

                Raylib.DrawText("Cycles elapsed = " + CPU.LoopAccumulator, lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("Scanline = " + IO.LY, lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("PC = " + (CPU.PC - 1).ToString("X4") + " (" + (CPU.PC - 1) + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("SP = " + CPU.SP.ToString("X4") + " (" + CPU.SP + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("A = " + CPU.A.ToString("X2") + " (" + CPU.A + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("B = " + CPU.B.ToString("X2") + " (" + CPU.B + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("C = " + CPU.C.ToString("X2") + " (" + CPU.C + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("D = " + CPU.D.ToString("X2") + " (" + CPU.D + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("E = " + CPU.E.ToString("X2") + " (" + CPU.E + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("H = " + CPU.H.ToString("X2") + " (" + CPU.H + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("L = " + CPU.L.ToString("X2") + " (" + CPU.L + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("FLAGS = Z(" + (CPU.FlagZ?1:0) + "), N(" + (CPU.FlagN?1:0) + "), H(" + (CPU.FlagH?1:0) + "), C(" + (CPU.FlagC?1:0) + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("Interrupts = Stop(" + (CPU.Stop?1:0) + "), Halt(" + (CPU.Halt?1:0) + "), IME(" + CPU.IME + ")", lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("IE = " + IO.IE.ToString("X2") + ", IF = " + IO.IF.ToString("X2"), lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("LastOp = " + CPU.LastOpcode.ToString("X2"), lineX, SpaceLine(), sizeText, Color.BLACK);
                Raylib.DrawText("Op = " + CPU.Opcode.ToString("X2") + ", " + InstructionName + " : Next ushort = " + Memory.Read(CPU.Opcode != 0xCB ? CPU.PC : (ushort)(CPU.PC + 1)).ToString("X2") + Memory.Read((ushort)(CPU.Opcode != 0xCB ? CPU.PC + 1 : CPU.PC + 2)).ToString("X2"), lineX, SpaceLine(), sizeText, Color.BLACK);

                CurrentLine = 5;
            }
        }

        // Auto space line
        private static int CurrentLine = 5;
        private static int SpaceLine()
        {
            int lineY = CurrentLine;
            CurrentLine += 18;
            return DrawHeight + lineY;
        }

        private static readonly string[] InstructionsName = new string[]
        {
            "NOP()", "LD_RRw_nn(ref B, ref C)", "LD_RRn_A(B, C)", "INC_RRw(ref B, ref C)", "INC_Rw(ref B)", "DEC_Rw(ref B)", "LD_Rw_n(ref B)", "RLCA()",
            "LD_Nnn_SP()", "ADD_HL_RRr(B, C)", "LD_A_RRn(B, C)", "DEC_RRw(ref B, ref C)", "INC_Rw(ref C)", "DEC_Rw(ref C)", "LD_Rw_n(ref C)", "RRCA()",
            "STOP()", "LD_RRw_nn(ref D, ref E)", "LD_RRn_A(D, E)", "INC_RRw(ref D, ref E)", "INC_Rw(ref D)", "DEC_Rw(ref D)", "LD_Rw_n(ref D)", "RLA()",
            "JR_en()", "ADD_HL_RRr(D, E)", "LD_A_RRn(D, E)", "DEC_RRw(ref D, ref E)", "INC_Rw(ref E)", "DEC_Rw(ref E)", "LD_Rw_n(ref E)", "RRA()",
            "JR_CC_en(!FlagZ)", "LD_RRw_nn(ref H, ref L)", "LD_HLnp_A()", "INC_RRw(ref H, ref L)", "INC_Rw(ref H)", "DEC_Rw(ref H)", "LD_Rw_n(ref H)",
            "DAA()", "JR_CC_en(FlagZ)", "ADD_HL_RRr(H, L)", "LD_A_HLnp()", "DEC_RRw(ref H, ref L)", "INC_Rw(ref L)", "DEC_Rw(ref L)", "LD_Rw_n(ref L)",
            "CPL()", "JR_CC_en(!FlagC)", "LD_RRw_nn(ref SP)", "LD_HLnm_A()", "INC_RRw(ref SP)", "INC_HLn()", "DEC_HLn()", "LD_HLn_n()", "SCF()",
            "JR_CC_en(FlagC)", "ADD_HL_SP()", "LD_A_HLnm()", "DEC_RRw(ref SP)", "INC_Rw(ref A)", "DEC_Rw(ref A)", "LD_Rw_n(ref A)", "CCF()",
            "LD_Rw_Rr(ref B, B)", "LD_Rw_Rr(ref B, C)", "LD_Rw_Rr(ref B, D)", "LD_Rw_Rr(ref B, E)", "LD_Rw_Rr(ref B, H)", "LD_Rw_Rr(ref B, L)",
            "LD_Rw_HLn(ref B)", "LD_Rw_Rr(ref B, A)", "LD_Rw_Rr(ref C, B)", "LD_Rw_Rr(ref C, C)", "LD_Rw_Rr(ref C, D)", "LD_Rw_Rr(ref C, E)",
            "LD_Rw_Rr(ref C, H)", "LD_Rw_Rr(ref C, L)", "LD_Rw_HLn(ref C)", "LD_Rw_Rr(ref C, A)", "LD_Rw_Rr(ref D, B)", "LD_Rw_Rr(ref D, C)",
            "LD_Rw_Rr(ref D, D)", "LD_Rw_Rr(ref D, E)", "LD_Rw_Rr(ref D, H)", "LD_Rw_Rr(ref D, L)", "LD_Rw_HLn(ref D)", "LD_Rw_Rr(ref D, A)",
            "LD_Rw_Rr(ref E, B)", "LD_Rw_Rr(ref E, C)", "LD_Rw_Rr(ref E, D)", "LD_Rw_Rr(ref E, E)", "LD_Rw_Rr(ref E, H)", "LD_Rw_Rr(ref E, L)",
            "LD_Rw_HLn(ref E)", "LD_Rw_Rr(ref E, A)", "LD_Rw_Rr(ref H, B)", "LD_Rw_Rr(ref H, C)", "LD_Rw_Rr(ref H, D)", "LD_Rw_Rr(ref H, E)",
            "LD_Rw_Rr(ref H, H)", "LD_Rw_Rr(ref H, L)", "LD_Rw_HLn(ref H)", "LD_Rw_Rr(ref H, A)", "LD_Rw_Rr(ref L, B)", "LD_Rw_Rr(ref L, C)",
            "LD_Rw_Rr(ref L, D)", "LD_Rw_Rr(ref L, E)", "LD_Rw_Rr(ref L, H)", "LD_Rw_Rr(ref L, L)", "LD_Rw_HLn(ref L)", "LD_Rw_Rr(ref L, A)",
            "LD_HLn_Rr(B)", "LD_HLn_Rr(C)", "LD_HLn_Rr(D)", "LD_HLn_Rr(E)", "LD_HLn_Rr(H)", "LD_HLn_Rr(L)", "HALT()", "LD_HLn_Rr(A)", "LD_Rw_Rr(ref A, B)",
            "LD_Rw_Rr(ref A, C)", "LD_Rw_Rr(ref A, D)", "LD_Rw_Rr(ref A, E)", "LD_Rw_Rr(ref A, H)", "LD_Rw_Rr(ref A, L)", "LD_Rw_HLn(ref A)",
            "LD_Rw_Rr(ref A, A)", "ADD_Rr(B)", "ADD_Rr(C)", "ADD_Rr(D)", "ADD_Rr(E)", "ADD_Rr(H)", "ADD_Rr(L)", "ADD_HLn()", "ADD_Rr(A)", "ADC_Rr(B)",
            "ADC_Rr(C)", "ADC_Rr(D)", "ADC_Rr(E)", "ADC_Rr(H)", "ADC_Rr(L)", "ADC_HLn()", "ADC_Rr(A)", "SUB_Rr(B)", "SUB_Rr(C)", "SUB_Rr(D)", "SUB_Rr(E)",
            "SUB_Rr(H)", "SUB_Rr(L)", "SUB_HLn()", "SUB_Rr(A)", "SBC_Rr(B)", "SBC_Rr(C)", "SBC_Rr(D)", "SBC_Rr(E)", "SBC_Rr(H)", "SBC_Rr(L)", "SBC_HLn()",
            "SBC_Rr(A)", "AND_Rr(B)", "AND_Rr(C)", "AND_Rr(D)", "AND_Rr(E)", "AND_Rr(H)", "AND_Rr(L)", "AND_HLn()", "AND_Rr(A)", "XOR_Rr(B)", "XOR_Rr(C)",
            "XOR_Rr(D)", "XOR_Rr(E)", "XOR_Rr(H)", "XOR_Rr(L)", "XOR_HLn()", "XOR_Rr(A)", "OR_Rr(B)", "OR_Rr(C)", "OR_Rr(D)", "OR_Rr(E)", "OR_Rr(H)",
            "OR_Rr(L)", "OR_HLn()", "OR_Rr(A)", "CP_Rr(B)", "CP_Rr(D)", "CP_Rr(E)", "CP_Rr(H)", "CP_Rr(L)", "CP_HLn()", "CP_Rr(A)", "RET_CC(!FlagZ)",
            "POP_RR(ref B, ref C)", "JP_CC_nn(!FlagZ)", "JP_nn()", "CALL_CC_nn(!FlagZ)", "PUSH_RR(B, C)", "ADD_n()", "RST_n(0x00)", "RET_CC(FlagZ)", "RET()",
            "JP_CC_nn(FlagZ)", "CB_op()", "CALL_CC_nn(FlagZ)", "CALL_nn()", "ADC_n()", "RST_n(0x08)", "RET_CC(!FlagC)", "POP_RR(ref D, ref E)",
            "JP_CC_nn(!FlagC)", "unknown()", "CALL_CC_nn(!FlagC)", "PUSH_RR(D, E)", "SUB_n()", "RST_n(0x10)", "RET_CC(FlagC)", "RETI()", "JP_CC_nn(FlagC)",
            "Unknown()", "CALL_CC_nn(FlagC)", "Unknown()", "SBC_n()", "RST_n(0x18)", "LDH_Nn_A()", "POP_RR(ref H, ref L)", "LDH_Cn_A()", "Unknown()",
            "Unknown()", "PUSH_RR(H, L)", "AND_n()", "RST_n(0x20)", "ADD_SP_en()", "JP_HL()", "LD_nn_A()", "Unknown()", "Unknown()", "Unknown()", "XOR_n()",
            "RST_n(0x28)", "LDH_A_Nn()", "POP_AF()", "LDH_A_Cn()", "DI()", "Unknown()", "PUSH_AF()", "OR_n()", "RST_n(0x30)", "LD_HL_SP_en()", "LD_SP_HL()",
            "LD_A_nn()", "EI()", "Unknown()", "Unknown()", "CP_n()", "RST_n(0x38)"
        };

        private static readonly string[] CBInstructionsName = new string[]
        {
            "RLC_Rw(ref B)", "RLC_Rw(ref C)", "RLC_Rw(ref D)", "RLC_Rw(ref E)", "RLC_Rw(ref H)", "RLC_Rw(ref L)", "RLC_HLn()", "RLC_Rw(ref A)", "RRC_Rw(ref B)",
            "RRC_Rw(ref C)", "RRC_Rw(ref D)", "RRC_Rw(ref E)", "RRC_Rw(ref H)", "RRC_Rw(ref L)", "RRC_HLn()", "RRC_Rw(ref A)", "RL_Rw(ref B)", "RL_Rw(ref C)",
            "RL_Rw(ref D)", "RL_Rw(ref E)", "RL_Rw(ref H)", "RL_Rw(ref L)", "RL_HLn()", "RL_Rw(ref A)", "RR_Rw(ref B)", "RR_Rw(ref C)", "RR_Rw(ref D)",
            "RR_Rw(ref E)", "RR_Rw(ref H)", "RR_Rw(ref L)", "RR_HLn()", "RR_Rw(ref A)", "SLA_Rw(ref B)", "SLA_Rw(ref C)", "SLA_Rw(ref D)", "SLA_Rw(ref E)",
            "SLA_Rw(ref H)", "SLA_Rw(ref L)", "SLA_HLn()", "SLA_Rw(ref A)", "SRA_Rw(ref B)", "SRA_Rw(ref C)", "SRA_Rw(ref D)", "SRA_Rw(ref E)", "SRA_Rw(ref H)",
            "SRA_Rw(ref L)", "SRA_HLn()", "SRA_Rw(ref A)", "SWAP_Rw(ref B)", "SWAP_Rw(ref C)", "SWAP_Rw(ref D)", "SWAP_Rw(ref E)", "SWAP_Rw(ref H)",
            "SWAP_Rw(ref L)", "SWAP_HLn()", "SWAP_Rw(ref A)", "SRL_Rw(ref B)", "SRL_Rw(ref C)", "SRL_Rw(ref D)", "SRL_Rw(ref E)", "SRL_Rw(ref H)", "SRL_Rw(ref L)",
            "SRL_HLn()", "SRL_Rw(ref A)", "BIT_n_r(0, B)", "BIT_n_r(0, C)", "BIT_n_r(0, D)", "BIT_n_r(0, E)", "BIT_n_r(0, H)", "BIT_n_r(0, L)", "BIT_n_HLn(0)",
            "BIT_n_r(0, A)", "BIT_n_r(1, B)", "BIT_n_r(1, C)", "BIT_n_r(1, D)", "BIT_n_r(1, E)", "BIT_n_r(1, H)", "BIT_n_r(1, L)", "BIT_n_HLn(1)", "BIT_n_r(1, A)",
            "BIT_n_r(2, B)", "BIT_n_r(2, C)", "BIT_n_r(2, D)", "BIT_n_r(2, E)", "BIT_n_r(2, H)", "BIT_n_r(2, L)", "BIT_n_HLn(2)", "BIT_n_r(2, A)", "BIT_n_r(3, B)",
            "BIT_n_r(3, C)", "BIT_n_r(3, D)", "BIT_n_r(3, E)", "BIT_n_r(3, H)", "BIT_n_r(3, L)", "BIT_n_HLn(3)", "BIT_n_r(3, A)", "BIT_n_r(4, B)", "BIT_n_r(4, C)",
            "BIT_n_r(4, D)", "BIT_n_r(4, E)", "BIT_n_r(4, H)", "BIT_n_r(4, L)", "BIT_n_HLn(4)", "BIT_n_r(4, A)", "BIT_n_r(5, B)", "BIT_n_r(5, C)", "BIT_n_r(5, D)",
            "BIT_n_r(5, E)", "BIT_n_r(5, H)", "BIT_n_r(5, L)", "BIT_n_HLn(5)", "BIT_n_r(5, A)", "BIT_n_r(6, B)", "BIT_n_r(6, C)", "BIT_n_r(6, D)", "BIT_n_r(6, E)",
            "BIT_n_r(6, H)", "BIT_n_r(6, L)", "BIT_n_HLn(6)", "BIT_n_r(6, A)", "BIT_n_r(7, B)", "BIT_n_r(7, C)", "BIT_n_r(7, D)", "BIT_n_r(7, E)", "BIT_n_r(7, H)",
            "BIT_n_r(7, L)", "BIT_n_HLn(7)", "BIT_n_r(7, A)", "RES_n_r(0, ref B)", "RES_n_r(0, ref C)", "RES_n_r(0, ref D)", "RES_n_r(0, ref E)", "RES_n_r(0, ref H)",
            "RES_n_r(0, ref L)", "RES_n_HLn(0)", "RES_n_r(0, ref A)", "RES_n_r(1, ref B)", "RES_n_r(1, ref C)", "RES_n_r(1, ref D)", "RES_n_r(1, ref E)",
            "RES_n_r(1, ref H)", "RES_n_r(1, ref L)", "RES_n_HLn(1)", "RES_n_r(1, ref A)", "RES_n_r(2, ref B)", "RES_n_r(2, ref C)", "RES_n_r(2, ref D)",
            "RES_n_r(2, ref E)", "RES_n_r(2, ref H)", "RES_n_r(2, ref L)", "RES_n_HLn(2)", "RES_n_r(2, ref A)", "RES_n_r(3, ref B)", "RES_n_r(3, ref C)",
            "RES_n_r(3, ref D)", "RES_n_r(3, ref E)", "RES_n_r(3, ref H)", "RES_n_r(3, ref L)", "RES_n_HLn(3)", "RES_n_r(3, ref A)", "RES_n_r(4, ref B)",
            "RES_n_r(4, ref C)", "RES_n_r(4, ref D)", "RES_n_r(4, ref E)", "RES_n_r(4, ref H)", "RES_n_r(4, ref L)", "RES_n_HLn(4)", "RES_n_r(4, ref A)",
            "RES_n_r(5, ref B)", "RES_n_r(5, ref C)", "RES_n_r(5, ref D)", "RES_n_r(5, ref E)", "RES_n_r(5, ref H)", "RES_n_r(5, ref L)", "RES_n_HLn(5)",
            "RES_n_r(5, ref A)", "RES_n_r(6, ref B)", "RES_n_r(6, ref C)", "RES_n_r(6, ref D)", "RES_n_r(6, ref E)", "RES_n_r(6, ref H)", "RES_n_r(6, ref L)",
            "RES_n_HLn(6)", "RES_n_r(6, ref A)", "RES_n_r(7, ref B)", "RES_n_r(7, ref C)", "RES_n_r(7, ref D)", "RES_n_r(7, ref E)", "RES_n_r(7, ref H)",
            "RES_n_r(7, ref L)", "RES_n_HLn(7)", "RES_n_r(7, ref A)", "SET_n_r(0, ref B)", "SET_n_r(0, ref C)", "SET_n_r(0, ref D)", "SET_n_r(0, ref E)",
            "SET_n_r(0, ref H)", "SET_n_r(0, ref L)", "SET_n_HLn(0)", "SET_n_r(0, ref A)", "SET_n_r(1, ref B)", "SET_n_r(1, ref C)", "SET_n_r(1, ref D)",
            "SET_n_r(1, ref E)", "SET_n_r(1, ref H)", "SET_n_r(1, ref L)", "SET_n_HLn(1)", "SET_n_r(1, ref A)", "SET_n_r(2, ref B)", "SET_n_r(2, ref C)",
            "SET_n_r(2, ref D)", "SET_n_r(2, ref E)", "SET_n_r(2, ref H)", "SET_n_r(2, ref L)", "SET_n_HLn(2)", "SET_n_r(2, ref A)", "SET_n_r(3, ref C)",
            "SET_n_r(3, ref D)", "SET_n_r(3, ref E)", "SET_n_r(3, ref H)", "SET_n_r(3, ref L)", "SET_n_HLn(3)", "SET_n_r(3, ref A)", "SET_n_r(4, ref B)",
            "SET_n_r(4, ref C)", "SET_n_r(4, ref D)", "SET_n_r(4, ref E)", "SET_n_r(4, ref H)", "SET_n_r(4, ref L)", "SET_n_HLn(4)", "SET_n_r(4, ref A)",
            "SET_n_r(5, ref B)", "SET_n_r(5, ref C)", "SET_n_r(5, ref D)", "SET_n_r(5, ref E)", "SET_n_r(5, ref H)", "SET_n_r(5, ref L)", "SET_n_HLn(5)",
            "SET_n_r(5, ref A)", "SET_n_r(6, ref B)", "SET_n_r(6, ref C)", "SET_n_r(6, ref D)", "SET_n_r(6, ref E)", "SET_n_r(6, ref H)", "SET_n_r(6, ref L)",
            "SET_n_HLn(6)", "SET_n_r(6, ref A)", "SET_n_r(7, ref B)", "SET_n_r(7, ref C)", "SET_n_r(7, ref D)", "SET_n_r(7, ref E)", "SET_n_r(7, ref H)",
            "SET_n_r(7, ref L)", "SET_n_HLn(7)", "SET_n_r(7, ref A)"
        };

        // Show floating text
        // ------------------

        private readonly static List<TextStruct> TextQueue = new();
        private readonly static List<int> TextToRemove = new();

        // Text structure
        private class TextStruct
        {
            public string text = "";
            public int remainingTime;
            public Color color;
        }

        // Add text to the queue
        public static void Text(string text, Color color, int remainingTime)
        {
            TextStruct newLine = new()
            {
                text = text,
                remainingTime = remainingTime,
                color = color
            };

            TextQueue.Add(newLine);
        }

        // Update and draw texts
        private static void UpdateAndDrawTexts()
        {
            TextToRemove.Clear();

            for (int i = 0; i < TextQueue.Count; i++)
            {
                Raylib.DrawText(TextQueue[i].text, 10, 10 + (i * 20), 20, TextQueue[i].color);
                TextQueue[i].remainingTime -= 16;

                if (TextQueue[i].remainingTime <= 0)
                TextToRemove.Add(i);
            }

            for (int i = TextToRemove.Count - 1; i >= 0; i--)
            {
                int index = TextToRemove[i];

                if (index >= 0 && index < TextQueue.Count)
                TextQueue.RemoveAt(index);
            }
        }

        // Show keys/buttons
        // -----------------

        private static void ShowXboxButton()
        {
            // D-PAD
            Raylib.DrawText("D-PAD : Down : " + Input.DPadDown, 10, 10, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Left : " + Input.DPadLeft, 10, 30, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Right : " + Input.DPadRight, 10, 50, 20, Color.BLUE);
            Raylib.DrawText("D-PAD : Up : " + Input.DPadUp, 10, 70, 20, Color.BLUE);

            // XABY-PAD
            Raylib.DrawText("XABY-PAD : A : " + Input.XabyPadA, 10, 90, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : B : " + Input.XabyPadB, 10, 110, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : X : " + Input.XabyPadX, 10, 130, 20, Color.BLUE);
            Raylib.DrawText("XABY-PAD : Y : " + Input.XabyPadY, 10, 150, 20, Color.BLUE);

            // MIDDLE-PAD
            Raylib.DrawText("MIDDLE-PAD : Back : " + Input.MiddlePadLeft, 10, 170, 20, Color.BLUE);
            Raylib.DrawText("MIDDLE-PAD : Start : " + Input.MiddlePadRight, 10, 190, 20, Color.BLUE);
            Raylib.DrawText("MIDDLE-PAD : Center : " + Input.MiddlePadCenter, 10, 210, 20, Color.BLUE);

            // TRIGGER-PAD
            Raylib.DrawText("TRIGGER-PAD : LB : " + Input.TriggerPadLB, 10, 230, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : RB : " + Input.TriggerPadRB, 10, 250, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : LT : " + Input.TriggerPadLT, 10, 270, 20, Color.BLUE);
            Raylib.DrawText("TRIGGER-PAD : RT : " + Input.TriggerPadRT, 10, 290, 20, Color.BLUE);

            // AXIS-PAD
            Raylib.DrawText("AXIS-PAD : LEFT Down : " + Input.AxisLeftPadDown, 10, 310, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Left : " + Input.AxisLeftPadLeft, 10, 330, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Right : " + Input.AxisLeftPadRight, 10, 350, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : LEFT Up : " + Input.AxisLeftPadUp, 10, 370, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Down : " + Input.AxisRightPadDown, 10, 390, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Left : " + Input.AxisRightPadLeft, 10, 410, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Right : " + Input.AxisRightPadRight, 10, 430, 20, Color.BLUE);
            Raylib.DrawText("AXIS-PAD : RIGHT Up : " + Input.AxisRightPadUp, 10, 450, 20, Color.BLUE);
        }

        #endregion
    }
}
