// ---
// CPU
// ---
public class CPU
{
    // Construct
    private Instructions Instructions;
    public State State;

    public CPU()
    {
        Instructions = new Instructions(this);
        State = new State(this);
    }

    // Cycles
    public int Cycles;

    // Program counter
    public ushort PC;

    // Interrupt
    public bool IME;
    public bool IME_scheduled;

    // Execution
    public void Execution()
    {
    }
}
