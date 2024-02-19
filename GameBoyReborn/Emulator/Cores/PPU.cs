// -------
// Graphic
// -------
using GameBoyReborn;
using Raylib_cs;

namespace Emulator
{
    public class PPU
    {
        // Construct
        private readonly Memory Memory;
        private readonly IO IO;
        private readonly CPU CPU;
        private bool DMA_InProgress = false;
        private short DMA_cycles = 0;
        private readonly Color[] ColorMap = new Color[4] { Color.RAYWHITE, Color.GRAY, Color.DARKGRAY, Color.BLACK };
        private byte[] BG_TileData = new byte[16];
        private byte[] WIN_TileData = new byte[16];
        private byte[] OBJ_TileData = new byte[32];

        public PPU(Emulation Emulation)
        {
            // Relation
            IO = Emulation.IO;
            Memory = Emulation.Memory;
            CPU = Emulation.CPU;

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
        private readonly Color?[] BGP_pal = new Color?[4];
        private readonly Color?[] OBP0_pal = new Color?[4];
        private readonly Color?[] OBP1_pal = new Color?[4];

        // Check append frame
        public bool CompletedFrame;

        // Cycles
        private int LyCycles = 0;
        private int ScreenRenableDelay = 0;

        // LCD enable
        private bool Enable = false;
        private bool Reenable = false;

        // Init all PPU registers
        private void InitRegisters()
        {
            Enable = true;
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
        }

        // PPU IO write
        public void LCDC(byte b)
        {
            IO.LCDC = b;

            // Handle enable screen
            if (!Binary.ReadBit(b, 7))
            {
                Enable = false;
            }

            if (!LCD_and_PPU_enable && Binary.ReadBit(b, 7))
            {
                //Console.WriteLine("By IO : " + LyCycles);

                if (!LCD_and_PPU_enable)
                    ScreenRenableDelay = 240;

/*                Console.WriteLine(LyCycles);
                LyCycles = 0;
                IO.LY = 0;
                Set_Mode_PPU(3);
                CompletedFrame = true;
                Reenable = true;*/
            }

            LCD_and_PPU_enable = Binary.ReadBit(b, 7);
            Window_tile_map_area = Binary.ReadBit(b, 6);
            Window_enable = Binary.ReadBit(b, 5);
            BG_and_Window_tile_data_area = Binary.ReadBit(b, 4);
            BG_tile_map_area = Binary.ReadBit(b, 3);
            OBJ_size = Binary.ReadBit(b, 2);
            OBJ_enable = Binary.ReadBit(b, 1);
            BG_and_Window_enable_priority = Binary.ReadBit(b, 0);

 /*           // Handle enable screen
            if (LCD_and_PPU_enable)
            {
                if (!Enable)
                {
                    Console.WriteLine("By IO : " + LyCycles);
                    ScreenRenableDelay = 449;
                }
            }
            else
            {
                Console.WriteLine("Disable : " + LyCycles);
                Enable = false;
                IO.LY = 0;
                LyCycles = 0;
                Set_Mode_PPU(0);
            }*/
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
        public void Set_LYC_equal_LY(bool enable)
        {
            Binary.SetBit(ref IO.STAT, 2, enable);
            LYC_equal_LY = enable;
        }
        public void Set_Mode_PPU(byte mode)
        {
            IO.STAT = (byte)((IO.STAT & 0xFC) | mode);
            Mode_PPU = mode;
        }

        public void BGP(byte b)
        {
            IO.BGP = b;
            BGP_pal[0] = ColorMap[b & 3];
            BGP_pal[1] = ColorMap[(b >> 2) & 3];
            BGP_pal[2] = ColorMap[(b >> 4) & 3];
            BGP_pal[3] = ColorMap[(b >> 6) & 3];
        }
        public void OBP0(byte b)
        {
            IO.OBP0 = b;
            OBP0_pal[1] = ColorMap[(b >> 2) & 3];
            OBP0_pal[2] = ColorMap[(b >> 4) & 3];
            OBP0_pal[3] = ColorMap[(b >> 6) & 3];
        }
        public void OBP1(byte b)
        {
            IO.OBP1 = b;
            OBP1_pal[1] = ColorMap[(b >> 2) & 3];
            OBP1_pal[2] = ColorMap[(b >> 4) & 3];
            OBP1_pal[3] = ColorMap[(b >> 6) & 3];
        }

        // Execution
        int FOR_DEBUG_TotalTicks = 0;

        public void Execution()
        {
            // Cycles elapsed
            int cycles = CPU.Cycles * 4;
            FOR_DEBUG_TotalTicks += cycles;
            LyCycles += cycles;

            // DMA transfer
            if (DMA_InProgress)
            {
                DMA_cycles -= (short)cycles;

                if (DMA_cycles <= 0)
                DMA_InProgress = false;
            }

            // Screen enable
            if (/*LCD_and_PPU_enable*/ Enable)
            {
                /*
                if (CPU.FOR_DEBUG_DebugModeEnable)
                Console.WriteLine(LyCycles);
                */

                // Scanline
                switch (Mode_PPU)
                {
                    // Horizontal blank
                    // ----------------
                    case 0:
                        if (LyCycles >= 456)
                        {
                            if (IO.LY < 143)
                            {
                                // Got to OAM scan
                                Set_Mode_PPU(2);

                                if (Mode_2_int_select)
                                Binary.SetBit(ref IO.IF, 1, true);
                            }
                            else
                            {
                                // Got to VBlank
                                Set_Mode_PPU(1);

                                Binary.SetBit(ref IO.IF, 0, true);

                                if (Mode_1_int_select)
                                Binary.SetBit(ref IO.IF, 1, true);
                            }

                            // This mode
                            Set_LYC_equal_LY(IO.LY == IO.LYC);

                            if (LYC_equal_LY && LYC_int_select)
                            Binary.SetBit(ref IO.IF, 1, true);

                            IO.LY++;
                        }
                    break;

                    // Vertical blank
                    // --------------
                    case 1:
                        // This mode
                        if (LyCycles >= 456)
                        {
                            Set_LYC_equal_LY(IO.LY == IO.LYC);

                            if (LYC_equal_LY && LYC_int_select)
                            Binary.SetBit(ref IO.IF, 1, true);

                            IO.LY++;
                        }

                        // Completed
                        if (IO.LY == 154)
                        {
                            CompletedFrame = true;
                            //Console.WriteLine(TotalTicks);
                            FOR_DEBUG_TotalTicks = 0;

                            // Go to OAM scan
                            Set_Mode_PPU(2);
                            IO.LY = 0;

                            if (Mode_2_int_select)
                            Binary.SetBit(ref IO.IF, 1, true);
                        }
                    break;

                    // OAM scan
                    // --------
                    case 2:
                        if (LyCycles >= 80)
                        {
                            // Go to drawing pixel
                            Set_Mode_PPU(3);
                        }
                    break;

                    // Drawing pixel
                    // -------------
                    case 3:
                        if (LyCycles >= 252)
                        {
                            // Draw
                            if (!Reenable)
                            DrawLine();

                            Reenable = false;

                            // Go to horizontal blank
                            Set_Mode_PPU(0);

                            if (Mode_0_int_select)
                            Binary.SetBit(ref IO.IF, 1, true);
                        }
                    break;
                }

                // Next scanline
                if (LyCycles >= 456)
                LyCycles -= 456;
            }

            // Screen disable
            else
            {
                if (ScreenRenableDelay > 0)
                {
                    ScreenRenableDelay -= cycles;

                    if (ScreenRenableDelay <= 0)
                    {
                        //Console.WriteLine(LyCycles);
                        ScreenRenableDelay = 0;
                        CompletedFrame = true;
                        LyCycles = 0;
                        IO.LY = 0;
                        Set_Mode_PPU(0);
                        Reenable = true;
                        Enable = true;

                        if (Mode_2_int_select)
                        Binary.SetBit(ref IO.IF, 1, true);

                        if (LYC_equal_LY && LYC_int_select)
                        Binary.SetBit(ref IO.IF, 1, true);
                    }
                }
                else if (LyCycles >= 70224)
                {
                    CompletedFrame = true;
                    LyCycles -= 70224;
                    FOR_DEBUG_TotalTicks = 0;
                }
            }
        }

        // Draw tile
        private static byte DrawTile(byte X, byte Y, Color?[] Pal, byte Msb, byte Lsb, byte ShiftX, bool Flip)
        {
            byte shift = !Flip ? (byte)(7 - ShiftX) : ShiftX;

            byte lowBit = (byte)((Msb >> shift) & 1);
            byte highBit = (byte)((Lsb >> shift) & 1);
            byte pixelValue = (byte)((highBit << 1) | lowBit);

            if (X < 160 && Y < 144)
            Drawing.SetPixel(X, Y, Pal[pixelValue]);

            return pixelValue;
        }

        // Draw line
        private void DrawLine()
        {
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
            sbyte OBJ_LastTile = -1;

            // Priority
            byte BG_WIN_PixelPriority = 0;

            // Set Y position
            if (BG_and_Window_enable_priority)
            {
                BG_WIN_SetInedex(ref BG_TileY, ref BG_PixelInTileY, IO.LY, IO.SCY);

                if (Window_enable)
                BG_WIN_SetInedex(ref WIN_TileY, ref WIN_PixelInTileY, (byte)(IO.LY - IO.WY), 0);
            }

            // Browse line
            for (byte x = 0; x < 160; x++)
            {
                // Draw background
                if (BG_and_Window_enable_priority)
                {
                    // Set X position and draw
                    BG_WIN_SetInedex(ref BG_TileX, ref BG_PixelInTileX, x, IO.SCX);
                    BG_WIN_UpdateTileData(ref BG_LastTileX, BG_TileX, BG_TileY, BG_StartTileMapArea, ref BG_TileData);
                    BG_WIN_PixelPriority = DrawTile(x, IO.LY, BGP_pal, BG_TileData[BG_PixelInTileY * 2], BG_TileData[BG_PixelInTileY * 2 + 1], BG_PixelInTileX, false);

                    // Draw window
                    if (Window_enable && x + 7 >= IO.WX && IO.LY >= IO.WY)
                    {
                        // Set X position and draw
                        BG_WIN_SetInedex(ref WIN_TileX, ref WIN_PixelInTileX, (byte)(x + 7 - IO.WX), 0);
                        BG_WIN_UpdateTileData(ref WIN_LastTileX, WIN_TileX, WIN_TileY, WIN_StartTileMapArea, ref WIN_TileData);
                        BG_WIN_PixelPriority = DrawTile(x, IO.LY, BGP_pal, WIN_TileData[BG_PixelInTileY * 2], WIN_TileData[WIN_PixelInTileY * 2 + 1], WIN_PixelInTileX, false);
                    }
                }

                // Draw objects
                if (OBJ_enable)
                {
                    // Scan line
                    for (int i = 159, ObjMax = 0; i > 0; i -= 4)
                    {
                        byte Y_pos = Memory.OAM[i - 3];
                        byte X_pos = Memory.OAM[i - 2];
                        byte TileIndex = Memory.OAM[i - 1];
                        byte Attr = Memory.OAM[i];
                        byte ObjSize = (byte)(!OBJ_size ? 7 : 15);
                        byte ObjPixelInTileY = (byte)(IO.LY + 16 - Y_pos);
                        byte ObjPixelInTileX = (byte)(x + 8 - X_pos);

                        // Object for this line with limit
                        if (ObjPixelInTileY <= ObjSize && ObjPixelInTileX <= 8 && ObjMax < 10)
                        {
                            bool ObjPriority = Binary.ReadBit(Attr, 7);
                            bool ObjFlipY = Binary.ReadBit(Attr, 6);
                            bool ObjFlipX = Binary.ReadBit(Attr, 5);
                            Color?[] ObjDmgPalette = Binary.ReadBit(Attr, 4) ? OBP1_pal : OBP0_pal;

                            if (!ObjPriority || (ObjPriority && BG_WIN_PixelPriority == 0))
                            {
                                // Y Flip
                                ObjPixelInTileY = (byte)(ObjFlipY ? ObjSize - ObjPixelInTileY : ObjPixelInTileY);

                                OBJ_UpdateTileData(ref OBJ_LastTile, TileIndex, ref OBJ_TileData);
                                DrawTile(x, IO.LY, ObjDmgPalette, OBJ_TileData[ObjPixelInTileY * 2], OBJ_TileData[ObjPixelInTileY * 2 + 1], ObjPixelInTileX, ObjFlipX);
                            }

                            ObjMax++;
                        }
                    }
                }
            }
        }

        // Handle backgroud and window
        // ---------------------------

        // Set all index background and window
        private static void BG_WIN_SetInedex(ref byte Tile, ref byte PixelInTile, byte pos, byte ShiftPixel)
        {
            byte Pixel = (byte)(pos + ShiftPixel > 255 ? (pos + ShiftPixel) % 256 : pos + ShiftPixel); // Pixel index in screen (256 x 256)
            Tile = (byte)Math.Floor(Pixel / 8.0); // Tile index in tile map (32 x 32)
            PixelInTile = (byte)(Pixel % 8); // Pixel index in tile (8 x 8)
        }

        // Update background and windo tile data
        private void BG_WIN_UpdateTileData(ref sbyte LastTileX, byte TileX, byte TileY, ushort StartTileMapArea, ref byte[] TileData)
        {
            if (LastTileX != (sbyte)TileX)
            {
                LastTileX = (sbyte)TileX;

                byte TileIndexData = Memory.VideoRam_nn[Memory.selectedVideoBank][StartTileMapArea + (TileY * 32 + TileX)];
                ushort StartTileDataArea = (ushort)(BG_and_Window_tile_data_area ? 0 : 0x1000);
                short BG_TileIndexDataS = !BG_and_Window_tile_data_area ? unchecked((sbyte)TileIndexData) : TileIndexData;

                Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], StartTileDataArea + (BG_TileIndexDataS * 16), TileData, 0, 16);
            }
        }

        // Handle Objects
        // --------------

        // DMA Transfer
        public void DMATransfer(byte b)
        {
            IO.DMA = b;

            DMA_InProgress = true;
            DMA_cycles = 640;
            Array.Clear(Memory.OAM);

            ushort startAddress = (ushort)(b << 8);

            if (startAddress >= 0xE000)
            startAddress -= 0x2000;

            for (byte r = 0; r < 0xA0; r++)
            Memory.OAM[r] = Memory.Read((ushort)(startAddress | r));
        }

        // Update objects tile data
        private void OBJ_UpdateTileData(ref sbyte LastTile, ushort at, ref byte[] TileData)
        {
            if (LastTile != (sbyte)at)
            {
                LastTile = (sbyte)at;

                if(!OBJ_size)
                Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], at * 16, TileData, 0, 16);
                else
                {
                    Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], (at & 0xFE) * 16, TileData, 0, 16);
                    Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], (at | 1) * 16, TileData, 16, 16);
                }
            }
        }
    }
}
