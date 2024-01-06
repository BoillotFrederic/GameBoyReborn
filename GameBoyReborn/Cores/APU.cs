// ---
// APU
// ---
using Raylib_cs;

namespace GameBoyReborn
{
    public class APU
    {
        // Cycles
        // private double CycleDuration = 0;
        private double FrameDuration = 0;
        private double CycleDuration = 1.0f / Audio.MaxSamplesPerUpdate;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        // Channels waves
        private short[] CH1_Wave = new short[4096];
        private short[][] CH1_WaveTest = new short[4][];

        private readonly float[] CH1_CH2_WaveDutyCycle = new float[4]{ 12.5f/100, 25.0f/100, 50.0f/100, 75.0f/100 };

        // Volume
        // int MasterVolume

        // Construct
        private readonly IO IO;
        private readonly CPU CPU;
        private readonly PPU PPU;

        public APU(IO _IO, CPU _CPU, PPU _PPU)
        {
            // Relation
            IO = _IO;
            CPU = _CPU;
            PPU = _PPU;

            // Init
            CH1_InitNR();
        }

        // Execution
        public void Execution()
        {
            if (PPU.CompletedFrame)
            {
                // Update buffers
                for (int indexBuffer = 0; indexBuffer < Audio.MaxSamplesPerUpdate; indexBuffer++)
                {
                    // Channel 1
                    short CH1_Value = CH1_GetValue();
                    Audio.AudioBuffer[indexBuffer] = CH1_Value;
                }
            }

                /*            // 1 / (4194304 / 4) (Freq CPU)
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

                                // Send sound
                                if (CH1_Wave.Length != 0 && CH1_Wave != null)
                                {
                                    unsafe
                                    {
                                        fixed (short* pData = &CH1_Wave[0])
                                        {
                                            Array.Copy(CH1_Wave, Audio.AudioBuffer, 4096);
                                            //Raylib.UpdateAudioStream(Program.CH1_AudioStream, pData, 0);
                                        }
                                    }

                                    Array.Clear(CH1_Wave, 0, 4096);
                                }
                            }*/
        }

        // Channel 1 params
        private double CH1_WaveDutyTimeElapsed = 0;
        private byte CH1_Pace = 0;
        private bool CH1_Direction = false;
        private byte CH1_IndividualStep = 0;
        private double CH1_WaveForm = 0;
        private byte CH1_InitialLengthTimer = 0;
        private double CH1_WaveLength = 0;
        private byte CH1_InitialVolume = 0;
        private bool CH1_EnvDir = false;
        private byte CH1_SweepPace = 0;
        private double CH1_LengthVolume = 0;
        private byte CH1_LowPeriod = 0;
        private byte CH1_HighPeriod = 0;
        private ushort CH1_Period = 0;
        private bool CH1_Trigger = false;
        private bool CH1_LengthEnable = false;

        // Get value for x position in buffer
        private short CH1_GetValue()
        {
            if (DAC1)
            {
                // Wave duty form
                byte WaveDuty = (byte)(IO.NR11 >> 5 & 3);

                // If length enable
                if (Binary.ReadBit(IO.NR14, 6))
                {
                    byte InitialLengthTimer = (byte)(IO.NR11 & 0x1F);
                    byte SoundLength = (byte)(64 - InitialLengthTimer);
                }

                if (CH1_WaveDutyTimeElapsed <= 0)
                {
                    // Wave duty check
                    ushort period = (ushort)((IO.NR14 & 0x07) << 8 | IO.NR13);
                    double ToneFrequency = 131072 / (2048 - period);


                }
            }
            
            return 0;
        }

        // Channel 1 IO init
        private void CH1_InitNR()
        {
            CH1_WaveDutyTimeElapsed = 0;
            CH1_Pace = (byte)(IO.NR10 >> 4 & 0x07);
            CH1_Direction = Binary.ReadBit(IO.NR10, 3);
            CH1_IndividualStep = (byte)(IO.NR10 & 0x07);
            CH1_WaveForm = IO.NR11 >> 6 & 3;
            CH1_InitialLengthTimer = (byte)(IO.NR11 & 0x3F);
            CH1_WaveLength = (64 - CH1_InitialLengthTimer) * (1.0f / 255.0f);
            CH1_InitialVolume = (byte)(IO.NR12 >> 4 & 0x0F);
            CH1_EnvDir = Binary.ReadBit(IO.NR12, 3);
            CH1_SweepPace = (byte)(IO.NR12 & 7);
            CH1_LengthVolume = CH1_SweepPace * (1.0f / 64.0f);
            CH1_LowPeriod = IO.NR13;
            CH1_HighPeriod = (byte)(IO.NR14 & 7);
            CH1_Period = (ushort)(CH1_HighPeriod << 8 | CH1_LowPeriod);
            CH1_Trigger = Binary.ReadBit(IO.NR14, 7);
            CH1_LengthEnable = Binary.ReadBit(IO.NR14, 6);

            IO.NR52 = (byte)((CH1_SweepPace != 0) ? IO.NR52 | 1 : IO.NR52 & 0xFE);

            if (Binary.ReadBit(IO.NR52, 7))
            DAC1 = CH1_SweepPace != 0;
        }

        // Channel 1 IO write
        public void CH1_WriteNR10(byte b)
        {
            CH1_Pace = (byte)(b >> 4 & 0x07);
            CH1_Direction = Binary.ReadBit(b, 3);
            CH1_IndividualStep = (byte)(b & 0x07);
        }
        public void CH1_WriteNR11(byte b)
        {
            CH1_InitialLengthTimer = (byte)(b & 0x3F);
            CH1_WaveForm = b >> 6 & 3;

            if(CH1_LengthEnable)
            CH1_WaveLength = (64 - CH1_InitialLengthTimer) * (1.0f/255.0f);
        }
        public void CH1_WriteNR12(byte b)
        {
            CH1_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH1_EnvDir = Binary.ReadBit(b, 3);
            CH1_SweepPace = (byte)(b & 7);
            CH1_LengthVolume = CH1_SweepPace * (1.0f/64.0f);
        }
        public void CH1_WriteNR13(byte b)
        {
            CH1_LowPeriod = b;
        }
        public void CH1_WriteNR14(byte b)
        {
            CH1_HighPeriod = (byte)(b & 7);
            CH1_Period = (ushort)(CH1_HighPeriod << 8 | CH1_LowPeriod);
            CH1_Trigger = Binary.ReadBit(b, 7);
            CH1_LengthEnable = Binary.ReadBit(b, 6);

            IO.NR52 = (byte)((CH1_SweepPace != 0) ? IO.NR52 | 1 : IO.NR52 & 0xFE);

            if (Binary.ReadBit(IO.NR52, 7))
            DAC1 = CH1_SweepPace != 0;
        }

        /*        // Pulse channel 1
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
                                int numSamplesPerPeriod = (int)Math.Abs(44100.0 / ToneFrequency);

                                if (numSamplesPerPeriod != 0)
                                for (int i = 0; i < 1024; i++)
                                {
                                    int dutyIndex = numSamplesPerPeriod == 0 ? 0 : i % numSamplesPerPeriod;
                                    CH1_Wave[CH1_BufferPos * 1024 + i] = (short)((dutyIndex < ((int)numSamplesPerPeriod * CH1_CH2_WaveDutyCycle[WaveDuty])) ? 32767 : -32767);
                                }

                            }

                            *//*
                                                if (CH1_BufferPos < 4)
                                                {
                                                    int numSamplesPerPeriod = (int)Math.Abs(44100.0 / ToneFrequency);

                                                    if (numSamplesPerPeriod != 0)
                                                        for (int i = 0; i < 1024; i++)
                                                        {
                                                            int dutyIndex = numSamplesPerPeriod == 0 ? 0 : i % numSamplesPerPeriod;
                                                            //CH1_Wave[CH1_BufferPos * 1024 + i] = (short)((dutyIndex < ((int)numSamplesPerPeriod * CH1_CH2_WaveDutyCycle[WaveDuty])) ? 32767 : -32767);
                                                            CH1_Wave[CH1_BufferPos * 1024 + i] = (short)((dutyIndex < numSamplesPerPeriod * CH1_CH2_WaveDutyCycle[WaveDuty]) ? 32767 : -32767);
                                                        }

                                                }
                            *//*

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

        *//*                // Period
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
                        }*//*
                    }
                    else
                        CH1_WaveCounter = 0;
                }*/


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
