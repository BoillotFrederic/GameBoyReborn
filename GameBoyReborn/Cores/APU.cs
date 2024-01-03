// ---
// APU
// ---
using Raylib_cs;

namespace GameBoyReborn
{
    public class APU
    {
        // Cycles
        private double CycleDuration = 0;
        private double FrameDuration = 0;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        // Channels waves
        private short[] CH1_Wave = new short[4096];
        private short[] CH1_WaveReport = new short[1024];

        private readonly float[] CH1_CH2_WaveDutyCycle = new float[4]{ 12.5f/100, 25.0f/100, 50.0f/100, 75.0f/100 };

        // Volume
        // int MasterVolume

        // Construct
        private readonly IO IO;
        private readonly CPU CPU;
        private readonly PPU PPU;

        public APU(IO _IO, CPU _CPU, PPU _PPU)
        {
            IO = _IO;
            CPU = _CPU;
            PPU = _PPU;
        }

        // Execution
        public void Execution()
        {
            // 1 / (4194304 / 4) (Freq CPU)
            CycleDuration = 0.0000002384185791015625 * CPU.Cycles * 4;
            FrameDuration += CycleDuration;
            CH1_WaveDutyTimeElapsed += CycleDuration;

            // Construct wave
            if ((IO.NR52 >> 7 & 1) == 1)
            {
                DAC1 = (IO.NR12 & 0xF8) != 0;

                Channel1();
                Channel2();
                Channel3();
                Channel4();
            }

            // Output
            if (PPU.CompletedFrame)
            {
                FrameDuration = 0;
                //Console.WriteLine(CH1_BufferPos);
                // Send sound
                if (CH1_Wave.Length != 0 && CH1_Wave != null)
                {
                    unsafe
                    {
/*                        for (int i = 0; i < CH1_Wave.Length; i++)
                        {
                            if (CH1_Wave[i] != 0)
                                Console.WriteLine(CH1_Wave[i]);
                        }*/
                        fixed (short* pData = &CH1_Wave[0])
                        {
                            /*
                                                        int _last = 0;

                                                        for (int i = 0; i < CH1_Wave.Length; i++)
                                                        {
                                                            if (CH1_Wave[i] != _last) Console.WriteLine(CH1_Wave[i]);
                                                            _last = CH1_Wave[i];
                                                        }
                            */

                            Array.Copy(CH1_Wave, Audio.AudioBuffer, 4096);
                            //Raylib.UpdateAudioStream(Program.CH1_AudioStream, pData, 0);
                        }
                    }

                    //Array.Clear(Audio.AudioBuffer, 0, 4096);
                    Array.Clear(CH1_Wave, 0, 4096);
                }
            }
        }

        // Pulse channel 1
        private double CH1_SweepTimeElapsed = 0;
        private double CH1_WaveDutyTimeElapsed = 0;
        private byte CH1_SweepCounter = 0;
        private byte CH1_WaveCounter = 0;
        private short CH1_NextWave = 0;
        private bool[] CH1_WaveDuty;
        private int test = 0;
        private bool WaveCreationEnable = false;
        private bool CH1_Enable = false;

        private int CH1_WaveDutyPos = 0;
        private bool LengthEnable;
        private byte InitialLengthTimer;
        private byte WaveDuty;
        private byte SoundLength;
        private byte CH1_BufferPos = 0;
        private void Channel1()
        {
            if (DAC1 && Binary.ReadBit(IO.NR52, 0))
            {
                if (CH1_WaveDutyTimeElapsed >= 0.00390625)
                {
                    CH1_WaveDutyTimeElapsed = 0;

                    // Wave duty check
                    ushort period = (ushort)((IO.NR14 & 0x07) << 8 | IO.NR13);
                    double ToneFrequency = 131072 / (2048 - period);

                    if (CH1_WaveDutyPos == 0)
                    {
                        CH1_BufferPos = (byte)Math.Floor(FrameDuration / 0.00390625);
                        if (CH1_BufferPos > 3)
                        CH1_BufferPos = 3;

                        LengthEnable = Binary.ReadBit(IO.NR14, 6);
                        InitialLengthTimer = (byte)(IO.NR11 & 0x1F);
                        WaveDuty = (byte)(IO.NR11 >> 5 & 3);
                        SoundLength = (byte)(64 - InitialLengthTimer);
                    }

                    // Wave duty construct
                    if (CH1_BufferPos < 4)
                    {
                        double numSamplesPerPeriod = (double)(44100.0 / ToneFrequency);

                        for (int i = 0; i < 1024; i++)
                        {

                            int dutyIndex = (int)(i % numSamplesPerPeriod);
                            CH1_Wave[CH1_BufferPos * 1024 + i] = (short)((dutyIndex < (numSamplesPerPeriod * CH1_CH2_WaveDutyCycle[WaveDuty])) ? 32767 : -32767);


                            //CH1_Wave[CH1_BufferPos * 1024 + i] = (short)((i % 256 < (256 * CH1_CH2_WaveDutyCycle[WaveDuty])) ? 32767 : -32767);
                        }
                    }

                    if (CH1_BufferPos >= 3)
                    CH1_BufferPos = 0;
                    else
                    CH1_BufferPos++;

                    // Wave duty completed
                    if (CH1_WaveDutyPos++ >= SoundLength)
                    {
                        CH1_WaveDutyPos = 0;
                        Binary.SetBit(ref IO.NR52, 0, false);
                    }
                }

/*                // Period
                ushort period = (ushort)((IO.NR14 & 0x07) << 8 | IO.NR13);

                // Sweep
                if (CH1_SweepTimeElapsed >= 0.0078125)
                {
                    CH1_SweepTimeElapsed = 0;

                    // Register
                    byte SweepPace = (byte)(IO.NR12 & 3);
                    bool EnvDir = Binary.ReadBit(IO.NR12, 3);
                    byte InitialVolume = (byte)(IO.NR12 >> 3 & 0x0F);

                    //double SampleRate = 1048576 / (2048 - period);
                    //double ToneFrequency = 131072 / (2048 - period);
                    byte Pace = (byte)(IO.NR10 >> 4 & 0x07);
                    bool Direction = Binary.ReadBit(IO.NR10, 3);
                    byte IndividualStep = (byte)(IO.NR10 & 0x07);

                    if (CH1_SweepCounter == 0)
                    CH1_SweepCounter = Pace;

                    CH1_SweepCounter--;

                    if (CH1_SweepCounter == 0)
                    Pace = 0;

                    if (IndividualStep != 0)
                    {
                        if (Direction)
                        period = (ushort)(period - (period / (2 ^ IndividualStep)));
                        else
                        {
                            period = (ushort)(period + (period / (2 ^ IndividualStep)));

                            if (period > 0x1FF)
                            IO.NR52 &= 0xFE;
                        }
                    }
                }*/
            }
            else
                CH1_WaveCounter = 0;
        }
        /*        private double CH1_SweepTimeElapsed = 0;
                private double CH1_WaveDutyTimeElapsed = 0;
                private byte CH1_SweepCounter = 0;
                private byte CH1_WaveCounter = 0;
                private short CH1_NextWave = 0;
                private bool[] CH1_WaveDuty;
                private int test = 0;
                private bool WaveCreationEnable = false;
                private bool CH1_Enable = false;

                private void Channel1()
                {
                    if (DAC1 && Binary.ReadBit(IO.NR52, 0))
                    {
                        CH1_SweepTimeElapsed += CycleDuration;
                        CH1_WaveDutyTimeElapsed += CycleDuration;

                        // Period
                        ushort period = (ushort)((IO.NR14 & 0x07) << 8 | IO.NR13);

                        // Sweep
                        if (CH1_SweepTimeElapsed >= 0.0078125)
                        {
                            CH1_SweepTimeElapsed = 0;

                            // Register
                            byte SweepPace = (byte)(IO.NR12 & 3);
                            bool EnvDir = Binary.ReadBit(IO.NR12, 3);
                            byte InitialVolume = (byte)(IO.NR12 >> 3 & 0x0F);

                            //double SampleRate = 1048576 / (2048 - period);
                            //double ToneFrequency = 131072 / (2048 - period);
                            byte Pace = (byte)(IO.NR10 >> 4 & 0x07);
                            bool Direction = Binary.ReadBit(IO.NR10, 3);
                            byte IndividualStep = (byte)(IO.NR10 & 0x07);

                            if (CH1_SweepCounter == 0)
                            CH1_SweepCounter = Pace;

                            CH1_SweepCounter--;

                            if (CH1_SweepCounter == 0)
                            Pace = 0;

                            if (IndividualStep != 0)
                            {
                                if (Direction)
                                period = (ushort)(period - (period / (2 ^ IndividualStep)));
                                else
                                {
                                    period = (ushort)(period + (period / (2 ^ IndividualStep)));

                                    if (period > 0x1FF)
                                    IO.NR52 &= 0xFE;
                                }
                            }
                        }

                        // Wave duty
                        if (CH1_WaveDutyTimeElapsed >= 0.00390625)
                        {
                            CH1_WaveDutyTimeElapsed = 0;

                            bool LengthEnable = Binary.ReadBit(IO.NR14, 6);
                            byte InitialLengthTimer = (byte)(IO.NR11 & 0x1F);
                            byte WaveDuty = (byte)(IO.NR11 >> 5 & 3);
                            double SoundLength = LengthEnable ? (64 - InitialLengthTimer) * 0.00390625 : 0.25;



                            *//*                    if (CH1_WaveCounter == 0)
                                                CH1_WaveDuty = new bool[64 - InitialLengthTimer];

                                                CH1_WaveDuty[CH1_WaveCounter++] = CH1_CH2_WaveDutyCycle[WaveDuty, CH1_WaveCounter % 8];*//*

                            test++;

                                *//*                    if (CH1_WaveCounter++ == SoundLength / 0.00390625)
                                                    {
                                                        Console.WriteLine(SoundLength);
                                                        CH1_WaveCounter = 0;
                                                        CH1_NextWave = 0;
                                                        Binary.SetBit(ref IO.NR52, 0, false);
                                                    }*//*

                                // Write buffer
                                //double SampleRate = 1048576 / (2048 - period);
                                *//*                    double ToneFrequency = 131072 / (2048 - period);

                                                    int sampleRate = 44100;
                                                    int numSamples = (int)(sampleRate / ToneFrequency);

                                                    short[] samples = new short[numSamples];

                                                    for (int i = 0; i < numSamples; i++)
                                                    {
                                                        int dutyIndex = i % CH1_WaveDuty.Length;
                                                        samples[i] = (short)(CH1_WaveDuty[dutyIndex] ? 32767 : -32767);
                                                    }*//*

                                //Console.WriteLine(samples.Length);
                                //CH1_Wave.Concat(samples);
                                *//*                    Array.Copy(samples, 0, CH1_Wave, CH1_NextWave, numSamples);
                                                    CH1_NextWave += (short)numSamples;*//*

                        }
                    }
                    else
                    CH1_WaveCounter = 0;
                }*/

        // Pulse channel 2
        private double CH2_SweepTimeElapsed = 0;
        private void Channel2()
        {
            if (Binary.ReadBit(IO.NR52, 1))
            {
                byte Pace = (byte)(IO.NR10 >> 4 & 0x07);

                if (CH2_SweepTimeElapsed >= 0.0078125 && Pace != 0)
                {
                    Console.WriteLine("Channel2");
                }
            }
        }

        // Wave channel
        private void Channel3()
        {
            if (Binary.ReadBit(IO.NR52, 2))
            {
                Console.WriteLine("Channel3");
            }
        }

        // Noise channel
        private void Channel4()
        {
            if (Binary.ReadBit(IO.NR52, 3))
            {
                Console.WriteLine("Channel4");
            }
        }
    }
}
