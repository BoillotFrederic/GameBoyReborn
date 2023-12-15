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

    // CONTROL
    bool LCD_and_PPU_enable;
    bool Window_tile_map_area;
    bool Window_enable;
    bool BG_and_Window_tile_data_area;
    bool BG_tile_map_area;
    bool OBJ_size;
    bool OBJ_enable;
    bool BG_and_Window_enable_priority;

    // STAT
    bool LYC_int_select;
    bool Mode_2_int_select;
    bool Mode_1_int_select;
    bool Mode_0_int_select;
    bool LYC_equal_LY;
    byte Mode_PPU;

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
            // Mode 0
            if(((IO.STAT) & 3)  == 0)
            {
                HorizontalBlank();
            }
            // Mode 1
            if (((IO.STAT) & 3) == 1)
            {
                VerticalBlank();
            }
            // Mode 2
            if (((IO.STAT) & 3) == 2)
            {
                OAMScan();
            }
            // Mode 3
            if (((IO.STAT) & 3) == 3)
            {
                DrawingPixels();
            }

            /*  
                // Other idea
                ushort CurrentLY = (ushort)Math.Floor(154 * CPU.Cycles / 70224.0);
                ushort CyclesLY = (byte)Math.Floor(CPU.Cycles % 456.0);

                // Mode 0
                if (CyclesLY <= 456 && CyclesLY > 252 && CyclesLY < 144)
                {

                }
                // Mode 1
                else if (CurrentLY >= 144)
                {

                }
                // Mode 2
                else if (CyclesLY <= 80 && CyclesLY < 144)
                {

                }
                // Mode 3
                else if (CyclesLY > 80 && CyclesLY <= 252 && CyclesLY < 144)
                {

                }
           */
        }
    }

    // Execute modes
    private void HorizontalBlank()
    {

    }

    private void VerticalBlank()
    {

    }

    private void OAMScan()
    {

    }

    private void DrawingPixels()
    {

    }

    // Create tiles
    private void Background()
    {
        Color[] Pal = new Color[4] { ColorMap[(IO.BGP >> 6) & 3], ColorMap[(IO.BGP >> 4) & 3], ColorMap[(IO.BGP >> 2) & 3], ColorMap[IO.BGP & 3] };
        Color[] TilesSet = new Color[256 * 256];

        ushort StartTileMapArea = (ushort)(BG_tile_map_area ? 0x1800 : 0x1C00);
        ushort EndTileMapArea = (ushort)(BG_tile_map_area ? 0x1BFF: 0x1FFF);
        ushort StartTileDataArea = (ushort)(BG_and_Window_tile_data_area ? 0x800 : 0x17FF);
        ushort EndTileDataArea = (ushort)(BG_and_Window_tile_data_area ? 0x0 : 0x0FFF);

        // Browse tiles
        for (int ty = 0; ty < 32; ty++)
        {
            for (int tx = 0; tx < 32; tx++)
            {
                byte TileIndex = Memory.VideoRam_nn[Memory.selectedVideoBank][StartTileMapArea + (ty * 32 + tx)];
                byte[] TileData = new byte[16];
                Array.Copy(Memory.VideoRam_nn[Memory.selectedVideoBank], StartTileDataArea + TileIndex * 16, TileData, 0, 16);

                // Draw tiles
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        byte lowBit = (byte)((TileData[y * 2] >> (7 - x)) & 1);
                        byte highBit = (byte)((TileData[y * 2 + 1] >> (7 - x)) & 1);
                        byte pixelValue = (byte)((highBit << 1) | lowBit);

                        TilesSet[(ty * 8 + y) * 32 + tx * 8 + x] = Pal[pixelValue];
                    }
                }
            }
        }
    }

    private void Window()
    {
        Color[] Pal = new Color[4] { ColorMap[(IO.BGP >> 6) & 3], ColorMap[(IO.BGP >> 4) & 3], ColorMap[(IO.BGP >> 2) & 3], ColorMap[IO.BGP & 3] };

        ushort StartTileMapArea = (ushort)(Window_tile_map_area ? 0x1800 : 0x1C00);
        ushort EndTileMapArea = (ushort)(Window_tile_map_area ? 0x1BFF : 0x1FFF);
        ushort StartTileDataArea = (ushort)(Window_tile_map_area ? 0x800 : 0x17FF);
        ushort EndTileDataArea = (ushort)(Window_tile_map_area ? 0x0 : 0x0FFF);
    }

    private void Object()
    {
        Color[] PalObj0 = new Color[4] { ColorMap[(IO.OBP0 >> 6) & 3], ColorMap[(IO.OBP0 >> 4) & 3], ColorMap[(IO.OBP0 >> 2) & 3], ColorMap[IO.OBP0 & 3] };
        Color[] PalObj1 = new Color[4] { ColorMap[(IO.OBP1 >> 6) & 3], ColorMap[(IO.OBP1 >> 4) & 3], ColorMap[(IO.OBP1 >> 2) & 3], ColorMap[IO.OBP1 & 3] };
    }
}
