// ---
// APU
// ---
using GameBoyReborn;

#pragma warning disable CS0414
#pragma warning disable CS0169

namespace Emulator
{
    public class APU
    {
        // Construct
        private readonly IO IO;

        public APU(Emulation Emulation)
        {
            // Relation
            IO = Emulation.IO;

            // Init
            CH1_InitNR();
            CH2_InitNR();
        }

        // Cycles
        private double CycleDuration = 1.0f / Audio.Frequency;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        private readonly double[] WaveDutyCycle = new double[4]{ 12.5f/100.0f*Math.PI, 25.0f/100.0f*Math.PI, 50.0f/100.0f*Math.PI, 75.0f/100.0f*Math.PI };

        // Volume
        // int MasterVolume

        // Execution
        public void Execution()
        {
            // Update buffers
            for (int indexBuffer = 0; indexBuffer < Audio.MaxSamplesPerUpdate; indexBuffer++)
            Audio.AudioBuffer[indexBuffer] = (short)((CH1_GetValue() + CH2_GetValue()) / 2);
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
            IO.NR10 = b;
            CH1_Pace = (byte)(b >> 4 & 0x07);
            CH1_Direction = Binary.ReadBit(b, 3);
            CH1_IndividualStep = (byte)(b & 0x07);
            CH1_SweepTime = CH1_Pace * (1.0f / 128.0f);
        }
        public void CH1_WriteNR11(byte b)
        {
            IO.NR11 = b;
            CH1_InitialLengthTimer = (byte)(b & 0x3F);
            CH1_WaveForm = (byte)(b >> 6 & 3);

            if (CH1_LengthEnable)
            CH1_WaveLength = (64 - CH1_InitialLengthTimer) * (1.0f / 255.0f);
        }
        public void CH1_WriteNR12(byte b)
        {
            IO.NR12 = b;
            CH1_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH1_EnvDir = Binary.ReadBit(b, 3);
            CH1_SweepPace = (byte)(b & 7);
            CH1_LengthVolume = CH1_SweepPace * (1.0f / 64.0f);
        }
        public void CH1_WriteNR13(byte b)
        {
            IO.NR13 = CH1_LowPeriod = b;
        }
        public void CH1_WriteNR14(byte b)
        {
            IO.NR14 = b;
            CH1_HighPeriod = (byte)(b & 7);
            CH1_Period = (ushort)(CH1_HighPeriod << 8 | CH1_LowPeriod);
            CH1_Trigger = Binary.ReadBit(b, 7);
            CH1_LengthEnable = Binary.ReadBit(b, 6);

            IO.NR52 = (byte)((CH1_SweepPace != 0) ? IO.NR52 | 1 : IO.NR52 & 0xFE);

            if (Binary.ReadBit(IO.NR52, 7))
            CH1_InitNR();
        }

        // Get value for x position in buffer
        private short CH1_GetValue()
        {
            if (DAC1)
            {
                // Delay for stop
                if (CH1_LengthEnable & CH1_WaveLength <= 0)
                {
                    CH1_WaveLength -= CycleDuration;

                    if (CH1_WaveLength <= 0)
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

                    if (CH1_SweepTime >= 0)
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
                    Binary.SetBit(ref IO.NR52, 0, false);
                    return 0;
                }

                CH1_Period = (ushort)NewPeriod;

                // Volume
                double VolumeRatio = 32767.0f / 15.0f;

                if (CH1_EnvVolumeEnable)
                {
                    CH1_LengthVolumeElapsed += CycleDuration;

                    if (CH1_LengthVolumeElapsed >= CH1_LengthVolume)
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
                short Value = (short)(CH1_IndexSample * Ratio < WaveDutyCycle[CH1_WaveForm] ? 1 * Volume : -1 * Volume);

                if (CH1_IndexSample >= Sample)
                CH1_IndexSample = 0;

                return Value;
            }

            return 0;
        }


        // Channel 2
        // ---------

        // Channel 2 params
        private ushort CH2_IndexSample = 0;
        private byte CH2_WaveForm = 0;
        private byte CH2_InitialLengthTimer = 0;
        private double CH2_WaveLength = 0;
        private byte CH2_InitialVolume = 0;
        private bool CH2_EnvDir = false;
        private byte CH2_SweepPace = 0;
        private double CH2_LengthVolume = 0;
        private double CH2_LengthVolumeElapsed = 0;
        private bool CH2_EnvVolumeEnable = false;
        private byte CH2_LowPeriod = 0;
        private byte CH2_HighPeriod = 0;
        private ushort CH2_Period = 0;
        private bool CH2_Trigger = false;
        private bool CH2_LengthEnable = false;

        // Channel 1 IO init
        private void CH2_InitNR()
        {
            CH2_WaveForm = (byte)(IO.NR21 >> 6 & 3);
            CH2_InitialLengthTimer = (byte)(IO.NR21 & 0x3F);
            CH2_WaveLength = (64 - CH2_InitialLengthTimer) * (1.0f / 255.0f);
            CH2_InitialVolume = (byte)(IO.NR22 >> 4 & 0x0F);
            CH2_EnvDir = Binary.ReadBit(IO.NR22, 3);
            CH2_SweepPace = (byte)(IO.NR22 & 7);
            CH2_LengthVolume = CH2_SweepPace * (1.0f / 64.0f);
            CH2_LengthVolumeElapsed = 0;
            CH2_EnvVolumeEnable = CH2_InitialVolume != 0 && CH2_SweepPace != 0;
            CH2_LowPeriod = IO.NR23;
            CH2_HighPeriod = (byte)(IO.NR24 & 7);
            CH2_Period = (ushort)(CH2_HighPeriod << 8 | CH2_LowPeriod);
            CH2_Trigger = Binary.ReadBit(IO.NR24, 7);
            CH2_LengthEnable = Binary.ReadBit(IO.NR24, 6);

            IO.NR52 = (byte)((CH2_SweepPace != 0) ? IO.NR52 | 2 : IO.NR52 & 0xFD);

            if (Binary.ReadBit(IO.NR52, 7))
            DAC2 = CH2_SweepPace != 0;
        }

        public void CH2_WriteNR21(byte b)
        {
            IO.NR21 = b;
            CH2_InitialLengthTimer = (byte)(b & 0x3F);
            CH2_WaveForm = (byte)(b >> 6 & 3);

            if (CH2_LengthEnable)
            CH2_WaveLength = (64 - CH2_InitialLengthTimer) * (1.0f / 255.0f);
        }
        public void CH2_WriteNR22(byte b)
        {
            IO.NR22 = b;
            CH2_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH2_EnvDir = Binary.ReadBit(b, 3);
            CH2_SweepPace = (byte)(b & 7);
            CH2_LengthVolume = CH2_SweepPace * (1.0f / 64.0f);
        }
        public void CH2_WriteNR23(byte b)
        {
            IO.NR23 = CH2_LowPeriod = b;
        }
        public void CH2_WriteNR24(byte b)
        {
            IO.NR24 = b;
            CH2_HighPeriod = (byte)(b & 7);
            CH2_Period = (ushort)(CH2_HighPeriod << 8 | CH2_LowPeriod);
            CH2_Trigger = Binary.ReadBit(b, 7);
            CH2_LengthEnable = Binary.ReadBit(b, 6);

            IO.NR52 = (byte)((CH2_SweepPace != 0) ? IO.NR52 | 2 : IO.NR52 & 0xFD);

            if (Binary.ReadBit(IO.NR52, 7))
            CH2_InitNR();
        }

        // Get value for x position in buffer
        private short CH2_GetValue()
        {
            if (DAC2)
            {
                // Delay for stop
                if (CH2_LengthEnable & CH2_WaveLength <= 0)
                {
                    CH2_WaveLength -= CycleDuration;

                    if (CH2_WaveLength <= 0)
                    {
                        CH2_LengthEnable = false;
                        DAC2 = false;
                    }
                }

                // Volume
                double VolumeRatio = 32767.0f / 15.0f;

                if (CH2_EnvVolumeEnable)
                {
                    CH2_LengthVolumeElapsed += CycleDuration;

                    if (CH2_LengthVolumeElapsed >= CH2_LengthVolume)
                    {
                        CH2_LengthVolumeElapsed -= CH2_LengthVolume;

                        if (CH2_EnvDir)
                        {
                            if (CH2_InitialVolume < 15)
                            CH2_InitialVolume++;
                            else
                            CH2_SweepPace = 0;
                        }
                        else
                        {
                            if (CH2_InitialVolume > 0)
                            CH2_InitialVolume--;
                            else
                            {
                                DAC2 = false;
                                return 0;
                            }
                        }
                    }
                }

                int Volume = (int)Math.Round(CH2_InitialVolume * VolumeRatio);


                // Apply frequency
                CH2_IndexSample++;

                double Frequency = 131072.0f / (2048 - CH2_Period);
                double Ratio = Math.PI * Frequency / Audio.Frequency;
                double Sample = Math.PI / Ratio;
                short Value = (short)(CH2_IndexSample * Ratio < WaveDutyCycle[CH2_WaveForm] ? 1 * Volume : -1 * Volume);

                if (CH2_IndexSample >= Sample)
                CH2_IndexSample = 0;

                return Value;
            }

            return 0;
        }

        // Wave channel
        private void Channel3()
        {
            if (Binary.ReadBit(IO.NR52, 2))
            {
                //Console.WriteLine("Channel3");
            }
        }

        // Noise channel
        private void Channel4()
        {
            if (Binary.ReadBit(IO.NR52, 3))
            {
                //Console.WriteLine("Channel4");
            }
        }
    }
}
