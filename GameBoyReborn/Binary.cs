// -------------
// Handle binary
// -------------
using Raylib_cs;

public static class Binary
{
    // Handle 16 bits
    // --------------

    // Least significant bit from 16 bits
    public static byte Lsb(ushort u16) 
    { 
        return (byte)(u16 & 0xFF); 
    }

    // Most significant bit from 16 bits
    public static byte Msb(ushort u16) 
    { 
        return (byte)((u16 >> 8) & 0xFF); 
    }

    // Create unsigned 16 bits with two 8 bits
    public static ushort U16(byte lsb, byte msb)
    {
        return (ushort)((msb << 8) | lsb);
    }

    // Handle bit
    // ----------

    // Read bit at position from right to left
    public static bool ReadBit(byte data, byte pos) 
    { 
        return ((data >> pos) & 0x01) == 1; 
    }

    // Set / unset bit at position from right to left
    public static void SetBit(ref byte data, byte pos, bool set)
    {
        if (set)
        data |= (byte)(1 << pos);

        else
        data &= (byte)~(1 << pos);
    }
}
