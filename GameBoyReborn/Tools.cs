// -------------
// General tools
// -------------

using System.Reflection;

namespace GameBoyReborn
{
    // Binary handle
    // -------------

    public static class Binary
    {
        // Handle 16 bits
        // --------------

        /// <summary>
        /// Least significant bit from 16 bits
        /// </summary>
        /// <returns>0xABCD to 0xCD</returns>
        public static byte Lsb(ushort u16)
        {
            return (byte)(u16 & 0xFF);
        }

        /// <summary>
        /// Least significant bit from 8 bits
        /// </summary>
        /// <returns>0xAB to 0xB</returns>
        public static byte Lsb(byte u8)
        {
            return (byte)(u8 & 0xF);
        }

        /// <summary>
        /// Most significant bit from 16 bits
        /// </summary>
        /// <returns>0xABCD to 0xAB</returns>
        public static byte Msb(ushort u16)
        {
            return (byte)((u16 >> 8) & 0xFF);
        }

        /// <summary>
        /// Most significant bit from 8 bits
        /// </summary>
        /// <returns>0xAB to 0xA</returns>
        public static byte Msb(byte u8)
        {
            return (byte)((u8 >> 4) & 0xF);
        }

        /// <summary>
        /// Create unsigned 16 bits with two 8 bits
        /// </summary>
        /// <param name="lsb">Least significant bit</param>
        /// <param name="msb">Most significant bit</param>
        /// <returns>0xCD and 0xAB = 0xABCD</returns>
        public static ushort U16(byte lsb, byte msb)
        {
            return (ushort)((msb << 8) | lsb);
        }

        // Handle bit
        // ----------

        /// <summary>
        /// Read bit at position from right to left
        /// </summary>
        /// <param name="u8">8 bits</param>
        /// <param name="pos">Position</param>
        /// <returns>True if set or false</returns>
        public static bool ReadBit(byte u8, byte pos)
        {
            return ((u8 >> pos) & 0x01) == 1;
        }

        /// <summary>
        /// Set/unset bit at position from right to left
        /// </summary>
        /// <param name="u8">8 bits</param>
        /// <param name="pos">Position</param>
        /// <param name="set"></param>
        public static void SetBit(ref byte u8, byte pos, bool set)
        {
            if (set)
            u8 |= (byte)(1 << pos);

            else
            u8 &= (byte)~(1 << pos);
        }
    }

    // Formulas handle
    // ---------------

    public static class Formulas
    {
        /// <summary>
        /// Center an element in container
        /// </summary>
        /// <param name="container">Container size</param>
        /// <param name="elm">Element size</param>
        /// <returns>New position</returns>
        public static int CenterElm(int container, int elm)
        {
            return (container - elm) / 2;
        }

        /// <summary>
        /// Just the percentage of an integer
        /// </summary>
        /// <param name="percent">Percentage</param>
        /// <param name="integer">Integer</param>
        public static int Percent(int percent, int integer)
        {
            return integer * percent / 100;
        }
    }

    // Refexion lighten
    // ----------------

    public static class RefLight
    {
        public static int GetIntByName(string name, Type classType, BindingFlags flags)
        {
            FieldInfo? fieldInfo = classType.GetField(name, flags);

            if (fieldInfo != null)
            {
                object? value = fieldInfo.GetValue(null);
                if (value != null)
                return (int)value;
            }

            return 0;
        }

        public static Delegate? GetDelegateMethodByName(string name, Type classType, Type methodType, BindingFlags flags)
        {
            MethodInfo? methodSetTextures = classType.GetMethod(name, flags);
            Delegate? delegateSetTextures = null;

            if(methodSetTextures != null)
            delegateSetTextures = methodSetTextures.CreateDelegate(methodType);

            return delegateSetTextures;
        }
    }
}
