// ------
// Joypad
// ------
using GameBoyReborn;

namespace Emulator
{
    public class Joypad
    {
        // Construct
        public Joypad(Emulation Emulation)
        {
        }

        // Input joypad to byte for Gameboy
        public static byte ReadAndWrite(byte requested)
        {
            // All buttons off
            requested |= 0x0F;

            // D-pad
            if ((requested & 0x30) != 0x10)
            {
                if (Input.DPadDown || Input.AxisLeftPadDown)
                Binary.SetBit(ref requested, 3, false);

                if (Input.DPadUp || Input.AxisLeftPadUp)
                Binary.SetBit(ref requested, 2, false);

                if (Input.DPadLeft || Input.AxisLeftPadLeft)
                Binary.SetBit(ref requested, 1, false);

                if (Input.DPadRight || Input.AxisLeftPadRight)
                Binary.SetBit(ref requested, 0, false);
            }

            // Buttons
            else if ((requested & 0x30) != 0x20)
            {
                if (Input.MiddlePadRight)
                Binary.SetBit(ref requested, 3, false);

                if (Input.MiddlePadLeft)
                Binary.SetBit(ref requested, 2, false);

                if (Input.XabyPadB)
                Binary.SetBit(ref requested, 1, false);

                if (Input.XabyPadA)
                Binary.SetBit(ref requested, 0, false);
            }

            return requested;
        }
    }
}
