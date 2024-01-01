// ---
// APU
// ---
using Raylib_cs;

namespace GameBoyReborn
{
    public class APU
    {
        // Cycles
        private double CH1_Ticks = 0;
        private double CH2_Ticks = 0;
        private double CH3_Ticks = 0;
        private double CH4_Ticks = 0;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        // Volume
        // int MasterVolume

        // Construct
        private readonly IO IO;
        private readonly CPU CPU;

        public APU(IO _IO, CPU _CPU)
        {
            IO = _IO;
            CPU = _CPU;
        }

        // Execution
        public void Execution()
        {
            // 1 / 4194304 (Freq CPU)
            CH1_Ticks += 0.0000002384185791015625 * CPU.Cycles;
            CH2_Ticks += 0.0000002384185791015625 * CPU.Cycles;
            CH3_Ticks += 0.0000002384185791015625 * CPU.Cycles;
            CH4_Ticks += 0.0000002384185791015625 * CPU.Cycles;

            if((IO.NR52 >> 7 & 1) == 1)
            {
                DAC1 = (IO.NR12 & 0xF8) != 0;

                Channel1();
                Channel2();
                Channel3();
                Channel4();
            }
        }

        // Pulse channel 1
        private void Channel1()
        {
            if (DAC1 && (IO.NR52 & 1) == 1)
            {
                if (CH1_Ticks >= 0.0078125)
                {
                    // Init cycles
                    CH1_Ticks = 0;

                    // Register
                    byte Pace = (byte)(IO.NR10 >> 4 & 0x07);
                    bool Direction = (IO.NR10 >> 3 & 1) == 1;
                    byte IndividualStep = (byte)(IO.NR10 & 0x07);
                    byte InitialLengthTimer = (byte)(IO.NR11 & 0x1F);
                    byte WaveDuty = (byte)(IO.NR11 >> 5 & 3);
                    byte SweepPace = (byte)(IO.NR12 & 3);
                    bool EnvDir = (IO.NR12 >> 2) == 1;
                    byte InitialVolume = (byte)(IO.NR12 >> 3 & 0x0F);
                    ushort period = (ushort)((IO.NR14 & 0x07) << 8 | IO.NR13);
                    double SampleRate = 1048576 / (2048 - period);
                    double ToneFrequency = 131072 / (2048 - period);

                    // Write buffer
                    int sampleRate = 44100;
                    int numSamples = (int)(sampleRate / ToneFrequency);
                    short[] samples = new short[numSamples];

                    double phase = 0.0;
                    double increment = 2.0 * Math.PI * ToneFrequency / sampleRate;

                    for (int i = 0; i < numSamples; i++)
                    {
                        samples[i] = (short)(Math.Sin(phase) * 32767);
                        phase += increment;
                        if (phase > 2.0 * Math.PI)
                        phase -= 2.0 * Math.PI;
                    }

                    // Send sound
                    if(samples.Length != 0)
                    unsafe
                    {
                        fixed (short* pData = &samples[0])
                        {
                            Raylib.SetAudioStreamBufferSizeDefault(numSamples);
                            Raylib.UpdateAudioStream(Program.AudioStream, pData, 0);
                        }
                    }

/*                    short[] bufferAudio = new short[numSamples];
                    for (int i = 0; i < samples.Length; i++)
                    {
                        bufferAudio[i] = samples[i];

                        if (increment >= bufferAudio.Length)
                        {
                            unsafe
                            {
                                fixed (short* pData = &samples[0])
                                {
                                    Raylib.SetAudioStreamBufferSizeDefault(numSamples);
                                    Raylib.UpdateAudioStream(Program.AudioStream, pData, 0);
                                }
                            }
                            i = 0;
                        }
                    }*/
                }
            }
        }

        // Pulse channel 2
        private void Channel2()
        {
            if ((IO.NR52 >> 1 & 1) == 1)
            {
                byte Pace = (byte)(IO.NR10 >> 4 & 0x07);

                if (CH2_Ticks >= 0.0078125 && Pace != 0)
                {
                    Console.WriteLine("Channel2");
                }
            }
        }

        // Wave channel
        private void Channel3()
        {
            if ((IO.NR52 >> 2 & 1) == 1)
            {
                Console.WriteLine("Channel3");
            }
        }

        // Noise channel
        private void Channel4()
        {
            if ((IO.NR52 >> 3 & 1) == 1)
            {
                Console.WriteLine("Channel4");
            }
        }
    }
}
