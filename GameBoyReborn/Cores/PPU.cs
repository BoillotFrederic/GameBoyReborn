// -------
// Graphic
// -------
using Raylib_cs;

public class PPU
{
    // Construct
    private Memory Memory;
    private IO IO;
    CPU CPU;
    private Color[] ColorMap = new Color[4] {Color.WHITE, Color.GRAY, Color.DARKGRAY, Color.BLACK};

    public PPU(IO _IO, Memory _Memory, CPU _CPU)
    {
        IO = _IO;
        Memory = _Memory;
        CPU = _CPU;
    }

    // Handle binary
    private bool ReadBit(byte data, byte pos) { return ((data >> pos) & 0x01) == 1; }
    private void SetBit(ref byte data, byte pos) { data |= (byte)(1 << pos); }

    // CONTROL
    private bool LCD_and_PPU_enable;
    private bool Window_tile_map_area;
    private bool Window_enable;
    private bool BG_and_Window_tile_data_area;
    private bool BG_tile_map_area;
    private bool OBJ_size;
    private bool OBJ_enable;
    private bool BG_and_Window_enable_priority;

    // STAT
    private bool LYC_int_select;
    private bool Mode_2_int_select;
    private bool Mode_1_int_select;
    private bool Mode_0_int_select;
    private bool LYC_equal_LY;
    private byte Mode_PPU;

    // Mode
    private bool mode0;
    private bool mode1;
    private bool mode2;
    private bool mode3;

    private short LastLy = -1;

    // Current LY Colors
    private Color[] CurrentLyColors = new Color[160];

    // Execution
    public void Execution()
    {
        // Read CONTROL
        LCD_and_PPU_enable = ReadBit(IO.LCDC, 7);
        Window_tile_map_area = ReadBit(IO.LCDC, 6);
        Window_enable = ReadBit(IO.LCDC, 5);
        BG_and_Window_tile_data_area = ReadBit(IO.LCDC, 4);
        BG_tile_map_area = ReadBit(IO.LCDC, 3);
        OBJ_size = ReadBit(IO.LCDC, 2);
        OBJ_enable = ReadBit(IO.LCDC, 1);
        BG_and_Window_enable_priority = ReadBit(IO.LCDC, 0);

        // Read STAT
        LYC_int_select = ReadBit(IO.STAT, 6);
        Mode_2_int_select = ReadBit(IO.STAT, 5);
        Mode_1_int_select = ReadBit(IO.STAT, 4);
        Mode_0_int_select = ReadBit(IO.STAT, 3);
        LYC_equal_LY = ReadBit(IO.STAT, 2);
        Mode_PPU = (byte)((IO.STAT) & 3);

        if (LCD_and_PPU_enable)
        {
            if (ReadBit(IO.IF, 1))
            {
                SetBit(ref IO.IE, 1);
                CPU.IME_scheduled = true;
            }

            IO.LY = (byte)Math.Floor(154 * (CPU.Cycles * 4) / 70224.0);
            ushort CyclesLY = (ushort)Math.Floor(CPU.Cycles * 4 % 456.0);

            if (LastLy != IO.LY)
            {
                mode0 = false;
                mode1 = false;
                mode2 = false;
                mode3 = false;
                IO.STAT = 0x00;
            }

            if (IO.LY == 154)
            {
                IO.LY = 0;
                IO.STAT &= 0xFC;
            }

            if (IO.LY == IO.LYC)
            SetBit(ref IO.STAT, 6);

            // Mode 2
            if (CyclesLY < 80 && !mode2)
            {
                SetBit(ref IO.STAT, 5);
                OAMScan(IO.LY);
                mode2 = true;
            }
            // Mode 3
            else if (CyclesLY < 80 + 172 && IO.LY < 144 && !mode3)
            {
                DrawingPixels(IO.LY);
                mode3 = true;
            }
            // Mode 0
            else if (CyclesLY < 80 + 172 + 204 && !mode0)
            {
                SetBit(ref IO.STAT, 3);
                HorizontalBlank();
                mode0 = true;
            }
            // Mode 1
            else if (IO.LY >= 144 && !mode1)
            {
                SetBit(ref IO.STAT, 4);
                VerticalBlank();
                mode1 = true;
            }
        }
    }

    // Execute modes
    private void HorizontalBlank()
    {
        //Console.WriteLine("MODE 0");
        if (ReadBit(IO.IF, 0))
        {
            SetBit(ref IO.IE, 0);
            CPU.IME_scheduled = true;
        }
    }

    private void VerticalBlank()
    {
        //Console.WriteLine("MODE 1");
        if (ReadBit(IO.IF, 0))
        {
            SetBit(ref IO.IE, 0);
            CPU.IME_scheduled = true;
        }
    }

    private void OAMScan(byte LY)
    {
        Color[] _Background = Background();
        Color[] _Window = Window();
        Color[] _Object = Object();

        for (byte x = 0; x < 160; x++)
        {
            Color PixelColorBackground = _Background[(IO.SCY + LY) * 160 + IO.SCX + x];
            Color PixelColorWindow = _Window[(IO.WY + LY) * 160 + IO.WX + x];
            Color PixelColorObject = _Object[(16 + LY) * 160 + 8 + x];

            CurrentLyColors[x] = PixelColorBackground;
            if (!PixelColorWindow.Equals(PixelColorBackground))
            CurrentLyColors[x] = PixelColorWindow;
            if (!PixelColorObject.Equals(PixelColorWindow))
            CurrentLyColors[x] = PixelColorObject;
        }
    }

    private void DrawingPixels(byte LY)
    {
        for (byte x = 0; x < 160; x++)
        Drawing.SetPixel(x, LY, CurrentLyColors[x]);

        //Console.WriteLine("MODE 3");
    }

    // Create tiles
    private Color[] Background()
    {
        Color[] TilesData = new Color[256 * 256];

        if (BG_and_Window_enable_priority)
        {
            Color[] Pal = new Color[4] { ColorMap[(IO.BGP >> 6) & 3], ColorMap[(IO.BGP >> 4) & 3], ColorMap[(IO.BGP >> 2) & 3], ColorMap[IO.BGP & 3] };

            ushort StartTileMapArea = (ushort)(BG_tile_map_area ? 0x1800 : 0x1C00);
            ushort StartTileDataArea = (ushort)(BG_and_Window_tile_data_area ? 0x800 : 0x17FF);

            // Browse tiles
            for (byte ty = 0; ty < 32; ty++)
            {
                for (byte tx = 0; tx < 32; tx++)
                {
                    byte TileIndex = Memory.VideoRam_nn[Memory.selectedVideoBank][StartTileMapArea + (ty * 32 + tx)];
                    byte[] TileData = new byte[16];
                    Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], StartTileDataArea + TileIndex * 16, TileData, 0, 16);

                    // Draw tiles
                    for (byte y = 0; y < 8; y++)
                    {
                        for (byte x = 0; x < 8; x++)
                        {
                            byte lowBit = (byte)((TileData[y * 2] >> (7 - x)) & 1);
                            byte highBit = (byte)((TileData[y * 2 + 1] >> (7 - x)) & 1);
                            byte pixelValue = (byte)((highBit << 1) | lowBit);

                            TilesData[(ty * 8 + y) * 32 + tx * 8 + x] = Pal[pixelValue];
                        }
                    }
                }
            }
        }

        return TilesData;
    }

    private Color[] Window()
    {
        Color[] TilesData = new Color[256 * 256];

        if (BG_and_Window_enable_priority && Window_enable)
        {
            Color[] Pal = new Color[4] { ColorMap[(IO.BGP >> 6) & 3], ColorMap[(IO.BGP >> 4) & 3], ColorMap[(IO.BGP >> 2) & 3], ColorMap[IO.BGP & 3] };

            ushort StartTileMapArea = (ushort)(Window_tile_map_area ? 0x1800 : 0x1C00);
            ushort StartTileDataArea = (ushort)(Window_tile_map_area ? 0x800 : 0x17FF);

            int windowX = IO.WX - 7;
            int windowY = IO.WY;

            // Browse tiles
            for (int ty = 0; ty < 32; ty++)
            {
                for (int tx = 0; tx < 32; tx++)
                {
                    int screenX = tx * 8 + windowX;
                    int screenY = ty * 8 + windowY;

                    if (screenX >= 0 && screenX < 160 && screenY >= 0 && screenY < 144)
                    {
                        byte TileIndex = Memory.VideoRam_nn[Memory.selectedVideoBank][StartTileMapArea + (ty * 32 + tx)];

                        byte[] TileData = new byte[16];
                        Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], StartTileDataArea + TileIndex * 16, TileData, 0, 16);


                        // Draw tiles
                        for (byte y = 0; y < 8; y++)
                        {
                            for (byte x = 0; x < 8; x++)
                            {
                                byte lowBit = (byte)((TileData[y * 2] >> (7 - x)) & 1);
                                byte highBit = (byte)((TileData[y * 2 + 1] >> (7 - x)) & 1);
                                byte pixelValue = (byte)((highBit << 1) | lowBit);

                                TilesData[(ty * 8 + y) * 32 + tx * 8 + x] = Pal[pixelValue];
                            }
                        }
                    }
                }
            }
        }

        return TilesData;
    }

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
                bool oPriority = ReadBit(Memory.OAM[i + 4], 7);
                bool oYflip = ReadBit(Memory.OAM[i + 4], 6);
                bool oXflip = ReadBit(Memory.OAM[i + 4], 5);
                bool oDMGPalette = ReadBit(Memory.OAM[i + 4], 4);
                bool oBank = ReadBit(Memory.OAM[i + 4], 3); // CGB
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

                            if(1 == 1 /* To do later : oPriority || (oInWindow && windowEnabled(ox + x, oy + y) */)
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
