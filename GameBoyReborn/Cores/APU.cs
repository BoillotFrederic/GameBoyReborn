// ---
// APU
// ---
using Raylib_cs;

namespace GameBoyReborn
{
    public class APU
    {
        // Cycles
        private double CycleDuration = 1.0f / Audio.Frequency;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        // Channels waves
        private short[] CH1_Wave = new short[4096];
        private short[][] CH1_WaveTest = new short[4][];

        private readonly double[] WaveDutyCycle = new double[4]{ 12.5f/100.0f*Math.PI, 25.0f/100.0f*Math.PI, 50.0f/100.0f*Math.PI, 75.0f/100.0f*Math.PI };

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
                    Audio.CH1_AudioBuffer[indexBuffer] = CH1_Value;
                }
            }
        }

        // Channel 1
        // ---------

        // Channel 1 params
        private ushort CH1_IndexSample = 0;
        private byte CH1_Pace = 0;
        private bool CH1_Direction = false;
        private byte CH1_IndividualStep = 0;
        private double CH1_SweepTime = 0;
        private byte CH1_WaveForm = 0;
        private byte CH1_InitialLengthTimer = 0;
        private double CH1_WaveLength = 0;
        private byte CH1_InitialVolume = 0;
        private bool CH1_EnvDir = false;
        private byte CH1_SweepPace = 0;
        private double CH1_LengthVolume = 0;
        private double CH1_LengthVolumeElapsed = 0;
        private bool CH1_EnvVolumeEnable = false;
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
                // Delay for stop
                if (CH1_LengthEnable & CH1_WaveLength <= 0)
                {
                    CH1_WaveLength -= CycleDuration;

                    if(CH1_WaveLength <= 0)
                    {
                        CH1_LengthEnable = false;
                        DAC1 = false;
                    }
                }

                // Sweep
                int NewPeriod = CH1_Period;

                if (CH1_Pace != 0 && CH1_SweepTime <= 0)
                {
                    CH1_SweepTime -= CycleDuration;

                    if(CH1_SweepTime >= 0)
                    {
                        if (CH1_Direction)
                        NewPeriod = (ushort)(CH1_Period - (CH1_Period / (int)Math.Pow(2, CH1_IndividualStep)));
                        else
                        NewPeriod = (ushort)(CH1_Period + (CH1_Period / (int)Math.Pow(2, CH1_IndividualStep)));
                    }
                }

                if ((!CH1_Direction && NewPeriod > 0x7FF) || (CH1_Direction && NewPeriod <= 0))
                {
                    DAC1 = false;
                    CH1_Pace = 0;
                    return 0;
                }

                CH1_Period = (ushort)NewPeriod;

                // Volume
                double VolumeRatio = 32767.0f / 15.0f;

                if (CH1_EnvVolumeEnable)
                {
                    CH1_LengthVolumeElapsed += CycleDuration;

                    if(CH1_LengthVolumeElapsed >= CH1_LengthVolume)
                    {
                        CH1_LengthVolumeElapsed -= CH1_LengthVolume;

                        if (CH1_EnvDir)
                        {
                            if (CH1_InitialVolume < 15)
                            CH1_InitialVolume++;
                            else
                            CH1_SweepPace = 0;
                        }
                        else
                        {
                            if (CH1_InitialVolume > 0)
                            CH1_InitialVolume--;
                            else
                            {
                                DAC1 = false;
                                CH1_Pace = 0;
                                return 0;
                            }
                        }
                    }
                }

                int Volume = (int)Math.Round(CH1_InitialVolume * VolumeRatio);


                // Apply frequency
                CH1_IndexSample++;

                double Frequency = 131072.0f / (2048 - CH1_Period);
                double Ratio = Math.PI * Frequency / Audio.Frequency;
                double Sample = Math.PI / Ratio;
                short Value = (short)((CH1_IndexSample) * Ratio < WaveDutyCycle[CH1_WaveForm] ? 1 * Volume : -1 * Volume);

                if (CH1_IndexSample > Sample)
                CH1_IndexSample = 0;

                return Value;
            }
            
            return 0;
        }

        // Channel 1 IO init
        private void CH1_InitNR()
        {
            CH1_Pace = (byte)(IO.NR10 >> 4 & 0x07);
            CH1_Direction = Binary.ReadBit(IO.NR10, 3);
            CH1_IndividualStep = (byte)(IO.NR10 & 0x07);
            CH1_SweepTime = CH1_Pace * (1.0f / 128.0f);
            CH1_WaveForm = (byte)(IO.NR11 >> 6 & 3);
            CH1_InitialLengthTimer = (byte)(IO.NR11 & 0x3F);
            CH1_WaveLength = (64 - CH1_InitialLengthTimer) * (1.0f / 255.0f);
            CH1_InitialVolume = (byte)(IO.NR12 >> 4 & 0x0F);
            CH1_EnvDir = Binary.ReadBit(IO.NR12, 3);
            CH1_SweepPace = (byte)(IO.NR12 & 7);
            CH1_LengthVolume = CH1_SweepPace * (1.0f / 64.0f);
            CH1_LengthVolumeElapsed = 0;
            CH1_EnvVolumeEnable = CH1_InitialVolume != 0 && CH1_SweepPace != 0;
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
            CH1_SweepTime = CH1_Pace * (1.0f/128.0f);
        }
        public void CH1_WriteNR11(byte b)
        {
            CH1_InitialLengthTimer = (byte)(b & 0x3F);
            CH1_WaveForm = (byte)(b >> 6 & 3);

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
            {
                CH1_InitNR();
            }
        }

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
