// ----------------
// Instructions set
// ----------------
internal class Instructions
{
    // Construct
    private CPU CPU;

    public Instructions(CPU cpu)
    {
        this.CPU = cpu;
    }

    // Registers
    private byte A;
    private byte B;
    private byte C;
    private ushort HL;
    private ushort SP;
    private ushort BC;
    private ushort DE;

    // Flags
    private bool Zf;
    private bool Nf;
    private bool Hf;
    private bool Cf;

    // All set
    // -------


    public void LD_Rw_Rr(ref byte Rw, byte Rr)
    {
    }
}
