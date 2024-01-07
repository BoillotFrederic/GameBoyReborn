// -------
// Graphic
// -------
using Raylib_cs;

namespace GameBoyReborn
{
    public class PPU
    {
        // Construct
        private readonly Memory Memory;
        private readonly IO IO;
        private readonly CPU CPU;
        private readonly Color[] ColorMap = new Color[4] { Color.WHITE, Color.GRAY, Color.DARKGRAY, Color.BLACK };
        private readonly byte[] BG_WIN_TileData = new byte[16];

        public PPU(IO _IO, Memory _Memory, CPU _CPU)
        {
            // Relation
            IO = _IO;
            Memory = _Memory;
            CPU = _CPU;

            // Init registers
            InitRegisters();
        }

        // Registers
        private bool LCD_and_PPU_enable;
        private bool Window_tile_map_area;
        private bool Window_enable;
        private bool BG_and_Window_tile_data_area;
        private bool BG_tile_map_area;
        private bool OBJ_size;
        private bool OBJ_enable;
        private bool BG_and_Window_enable_priority;
        private bool LYC_int_select;
        private bool Mode_2_int_select;
        private bool Mode_1_int_select;
        private bool Mode_0_int_select;
        private bool LYC_equal_LY;
        private byte Mode_PPU;
        private byte SCX;
        private byte SCY;
        private byte WX;
        private byte WY;
        private byte BGP;

        // Check append frame
        public bool CompletedFrame;

        // Cycles
        private int LyCycles = 0;

        // Init all PPU registers
        private void InitRegisters()
        {
            LCD_and_PPU_enable = Binary.ReadBit(IO.LCDC, 7);
            Window_tile_map_area = Binary.ReadBit(IO.LCDC, 6);
            Window_enable = Binary.ReadBit(IO.LCDC, 5);
            BG_and_Window_tile_data_area = Binary.ReadBit(IO.LCDC, 4);
            BG_tile_map_area = Binary.ReadBit(IO.LCDC, 3);
            OBJ_size = Binary.ReadBit(IO.LCDC, 2);
            OBJ_enable = Binary.ReadBit(IO.LCDC, 1);
            BG_and_Window_enable_priority = Binary.ReadBit(IO.LCDC, 0);
            LYC_int_select = Binary.ReadBit(IO.STAT, 6);
            Mode_2_int_select = Binary.ReadBit(IO.STAT, 5);
            Mode_1_int_select = Binary.ReadBit(IO.STAT, 4);
            Mode_0_int_select = Binary.ReadBit(IO.STAT, 3);
            LYC_equal_LY = Binary.ReadBit(IO.STAT, 2);
            Mode_PPU = (byte)(IO.STAT & 3);
            SCX = IO.SCX;
            SCY = IO.SCY;
            WX = IO.WX;
            WY = IO.WY;
            BGP = IO.BGP;
        }

        // PPU IO write
        public void LCDC(byte b)
        {
            IO.LCDC = b;
            LCD_and_PPU_enable = Binary.ReadBit(b, 7);
            Window_tile_map_area = Binary.ReadBit(b, 6);
            Window_enable = Binary.ReadBit(b, 5);
            BG_and_Window_tile_data_area = Binary.ReadBit(b, 4);
            BG_tile_map_area = Binary.ReadBit(b, 3);
            OBJ_size = Binary.ReadBit(b, 2);
            OBJ_enable = Binary.ReadBit(b, 1);
            BG_and_Window_enable_priority = Binary.ReadBit(b, 0);
        }
        public void STAT(byte b)
        {
            IO.STAT = b;
            LYC_int_select = Binary.ReadBit(b, 6);
            Mode_2_int_select = Binary.ReadBit(b, 5);
            Mode_1_int_select = Binary.ReadBit(b, 4);
            Mode_0_int_select = Binary.ReadBit(b, 3);
            LYC_equal_LY = Binary.ReadBit(b, 2);
            Mode_PPU = (byte)(b & 3);
        }
        public void LYC(byte b)
        {
        }
        public void SCX_W(byte b) { IO.SCX = SCX = b; }
        public void SCY_W(byte b) { IO.SCY = SCY = b; }
        public void WX_W(byte b) { IO.WX = WX = b; }
        public void WY_W(byte b) { IO.WY = WY = b; }
        public void BGP_W(byte b) { IO.BGP = BGP = b; }

        // Execution
        public void Execution()
        {
            int cycles = CPU.Cycles * 4;

            if (LCD_and_PPU_enable)
            {
                LyCycles += cycles;

                switch (Mode_PPU)
                {
                    // Horizontal blank
                    // ----------------
                    case 0:
                        if (LyCycles >= 456)
                        {
                            // Next mode (OAM scan / Vertical blank)
                            if (IO.LY != 143)
                            {
                                IO.LY++;
                                LyCompare();
                                Mode_PPU = 2;

                                if (Mode_2_int_select)
                                Binary.SetBit(ref IO.IF, 1, true); // 1 = LCD
                            }
                            else
                            {
                                Mode_PPU = 1;

                                if (Mode_1_int_select)
                                Binary.SetBit(ref IO.IF, 0, true); // 0 = VBlank
                            }
                        }
                    break;

                    // Vertical blank
                    // --------------
                    case 1:
                        if (LyCycles >= 456)
                        {
                            IO.LY++;
                            LyCompare();
                        }

                        // Completed
                        if (IO.LY == 153)
                        {
                            CompletedFrame = true;

                            // Next mode (OAM scan)
                            Mode_PPU = 2;
                            IO.LY = 0;

                            if (Mode_2_int_select)
                            Binary.SetBit(ref IO.IF, 1, true); // 1 = LCD
                        }
                    break;

                    // OAM scan
                    // --------
                    case 2:
                        if (LyCycles >= 80)
                        {
                            // Next mode (Drawing pixel)
                            Mode_PPU = 3;
                        }
                    break;

                    // Drawing pixel
                    // -------------
                    case 3:
                        if (LyCycles >= 252)
                        {
                            // Draw
                            DrawLine();

                            // Next mode (Horizontal blank)
                            Mode_PPU = 0;

                            if (Mode_0_int_select)
                            Binary.SetBit(ref IO.IF, 1, true); // 1 = LCD
                        }
                    break;
                }

                if (LyCycles >= 456)
                LyCycles = LyCycles - 456;
            }
            else
            {
                LyCycles += 16;

                if (LyCycles >= 70224)
                CompletedFrame = true;
            }
        }

        private void LyCompare()
        {
            if (IO.LY == IO.LYC)
            {
                LYC_equal_LY = true;

                /*            if (ReadBit(IO.IE, setInterrupt))
                            SetBit(ref IO.IF, setInterrupt, true);*/
            }
            else
            LYC_equal_LY = false;
        }

        // Draw line
        private void DrawLine()
        {
            // Palettes
            Color[] BG_WIN_Pal = new Color[4] { ColorMap[BGP & 3], ColorMap[(BGP >> 2) & 3], ColorMap[(BGP >> 4) & 3], ColorMap[(BGP >> 6) & 3] };

            // Index start for map area
            ushort BG_StartTileMapArea = (ushort)(!BG_tile_map_area ? 0x1800 : 0x1C00);
            ushort WIN_StartTileMapArea = (ushort)(!Window_tile_map_area ? 0x1800 : 0x1C00);

            // Backgroud and window tile index in tile map (32 x 32)
            byte BG_TileY = 0;
            byte BG_TileX = 0;
            byte WIN_TileY = 0;
            byte WIN_TileX = 0;

            // Backgroud and window pixel index in tile (8 x 8)
            byte BG_PixelInTileY = 0;
            byte BG_PixelInTileX = 0;
            byte WIN_PixelInTileY = 0;
            byte WIN_PixelInTileX = 0;

            // Last tiles
            sbyte BG_LastTileX = -1;
            sbyte WIN_LastTileX = -1;

            // Set Y position
            if (BG_and_Window_enable_priority)
            {
                BG_WIN_SetInedex(ref BG_TileY, ref BG_PixelInTileY, IO.LY, SCY, 143);

                if (Window_enable)
                BG_WIN_SetInedex(ref WIN_TileY, ref WIN_PixelInTileY, IO.LY, WY, 143);
            }

            // Browse line
            for (byte x = 0; x < 160; x++)
            {
                // If enable, draw background
                if (BG_and_Window_enable_priority)
                {
                    // Set X position and draw
                    BG_WIN_SetInedex(ref BG_TileX, ref BG_PixelInTileX, x, SCX, 159);
                    BG_WIN_UpdateTileData(ref BG_LastTileX, BG_TileX, BG_TileY, BG_StartTileMapArea);
                    BG_WIN_DrawPixel(x, IO.LY, BG_WIN_Pal, BG_WIN_TileData[BG_PixelInTileY * 2], BG_WIN_TileData[BG_PixelInTileY * 2 + 1], BG_PixelInTileX);

                    // If enable, draw window
                    if (Window_enable)
                    {
                        // Set X position and draw
                        BG_WIN_SetInedex(ref WIN_TileX, ref WIN_PixelInTileX, x, WX, 159);
                        BG_WIN_UpdateTileData(ref WIN_LastTileX, WIN_TileX, WIN_TileY, WIN_StartTileMapArea);
                        BG_WIN_DrawPixel(x, IO.LY, BG_WIN_Pal, BG_WIN_TileData[BG_PixelInTileY * 2], BG_WIN_TileData[WIN_PixelInTileY * 2 + 1], WIN_PixelInTileX);
                    }
                }
            }
        }

        // Handle backgroud and window
        // ---------------------------

        // Set all index background and window
        private static void BG_WIN_SetInedex(ref byte Tile, ref byte PixelInTile, byte pos, byte ShiftPixel, byte Overflow)
        {
            byte Pixel = (byte)(pos + ShiftPixel > 255 ? (ShiftPixel + Overflow) % 256 : pos + ShiftPixel); // Pixel index in screen (256 x 256)
            Tile = (byte)Math.Floor(Pixel / 8.0); // Tile index in tile map (32 x 32)
            PixelInTile = (byte)(Pixel % 8); // Pixel index in tile (8 x 8)
        }

        // Update tile data
        private void BG_WIN_UpdateTileData(ref sbyte LastTileX, byte TileX, byte TileY, ushort StartTileMapArea)
        {
            if (LastTileX != (sbyte)TileX)
            {
                LastTileX = (sbyte)TileX;
                byte TileIndexData = Memory.VideoRam_nn[Memory.selectedVideoBank][StartTileMapArea + (TileY * 32 + TileX)];
                ushort StartTileDataArea = (ushort)(BG_and_Window_tile_data_area ? 0 : TileIndexData > 0x7F ? 800 : 1000);
                short BG_TileIndexDataS = BG_and_Window_tile_data_area ? unchecked((sbyte)TileIndexData) : TileIndexData;

                Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], (StartTileDataArea + BG_TileIndexDataS) * 16, BG_WIN_TileData, 0, 16);
            }
        }

        // Draw pixel background and window
        private static void BG_WIN_DrawPixel(byte X, byte Y, Color[] Pal, byte Msb, byte Lsb, byte ShiftX)
        {
            byte lowBit = (byte)((Msb >> (7 - ShiftX)) & 1);
            byte highBit = (byte)((Lsb >> (7 - ShiftX)) & 1);
            byte pixelValue = (byte)((highBit << 1) | lowBit);

            if (X < 160 && Y < 144)
            Drawing.SetPixel(X, Y, Pal[pixelValue]);
        }

        // Handle Objects
        // --------------

        private Color[] Object()
        {
            Color[] oTilesSet = new Color[176 * 176];

            if (OBJ_enable)
            {

                Color[] PalObj0 = new Color[4] { ColorMap[(IO.OBP0 >> 6) & 3], ColorMap[(IO.OBP0 >> 4) & 3], ColorMap[(IO.OBP0 >> 2) & 3], ColorMap[IO.OBP0 & 3] };
                Color[] PalObj1 = new Color[4] { ColorMap[(IO.OBP1 >> 6) & 3], ColorMap[(IO.OBP1 >> 4) & 3], ColorMap[(IO.OBP1 >> 2) & 3], ColorMap[IO.OBP1 & 3] };

                for (byte i = 0; i < 160; i += 4)
                {
                    byte oy = Memory.OAM[i];
                    byte ox = Memory.OAM[i + 2];
                    byte oIndex = Memory.OAM[i + 3];
                    bool oPriority = Binary.ReadBit(Memory.OAM[i + 4], 7);
                    bool oYflip = Binary.ReadBit(Memory.OAM[i + 4], 6);
                    bool oXflip = Binary.ReadBit(Memory.OAM[i + 4], 5);
                    bool oDMGPalette = Binary.ReadBit(Memory.OAM[i + 4], 4);
                    bool oBank = Binary.ReadBit(Memory.OAM[i + 4], 3); // CGB
                    byte oCGBPalette = (byte)(Memory.OAM[i + 4] & 7); // CGB

                    // Draw 8x8 object
                    if (!OBJ_size)
                    {
                        byte[] oTileData = new byte[16];
                        Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], oIndex * 16, oTileData, 0, 16);

                        for (byte y = 0; y < 8; y++)
                        {
                            for (byte x = 0; x < 8; x++)
                            {
                                byte drawX = oXflip ? (byte)(7 - x) : x;
                                byte drawY = oYflip ? (byte)(7 - y) : y;

                                byte lowBit = (byte)((oTileData[drawY * 2] >> (7 - drawX)) & 1);
                                byte highBit = (byte)((oTileData[drawY * 2 + 1] >> (7 - drawX)) & 1);
                                byte pixelValue = (byte)((highBit << 1) | lowBit);

                                if (1 == 1 /* To do later : oPriority || (oInWindow && windowEnabled(ox + x, oy + y) */)
                                oTilesSet[(oy + y) * 160 + ox + x] = oDMGPalette ? PalObj1[pixelValue] : PalObj0[pixelValue];
                            }
                        }
                    }
                    // Draw 8x16 object
                    else
                    {
                        byte topTileIndex = (byte)(oIndex & 0xFE); // NN & $FE
                        byte bottomTileIndex = (byte)(oIndex | 0x01); // NN | $01

                        byte[] topTileData = new byte[16];
                        byte[] bottomTileData = new byte[16];

                        Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], topTileIndex * 16, topTileData, 0, 16);
                        Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], bottomTileIndex * 16, bottomTileData, 0, 16);

                        for (byte y = 0; y < 16; y++)
                        {
                            for (byte x = 0; x < 8; x++)
                            {
                                byte drawX = oXflip ? (byte)(7 - x) : x;
                                byte drawY = oYflip ? (byte)(15 - y) : y;

                                byte lowBit = (byte)((drawY < 8) ? ((topTileData[drawY * 2] >> (7 - drawX)) & 1) : ((bottomTileData[(drawY - 8) * 2] >> (7 - drawX)) & 1));
                                byte highBit = (byte)((drawY < 8) ? ((topTileData[drawY * 2 + 1] >> (7 - drawX)) & 1) : ((bottomTileData[(drawY - 8) * 2 + 1] >> (7 - drawX)) & 1));
                                byte pixelValue = (byte)((highBit << 1) | lowBit);

                                if (1 == 1 /* To do later : oPriority || (oInWindow && windowEnabled(ox + x, oy + y) */)
                                oTilesSet[(oy + y) * 160 + ox + x] = oDMGPalette ? PalObj1[pixelValue] : PalObj0[pixelValue];
                            }
                        }
                    }
                }
            }

            return oTilesSet;
        }
    }
}
