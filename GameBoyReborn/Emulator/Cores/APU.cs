// ---------------------
// Audio Processing Unit
// ---------------------
using GameBoyReborn;
using Raylib_cs;

namespace Emulator
{
    public class APU
    {
        #region Construct

        private readonly IO IO;

        public APU(Emulation Emulation)
        {
            // Relation
            IO = Emulation.IO;

            // Noise sound
            Noise7Bit = GenerateNoise(32767, 7);
            Noise15Bit = GenerateNoise(32767, 15);
        }

        #endregion

        #region APU ticks

        private bool OutputRight = false;

        /// <summary>
        /// Execution (APU ticks)
        /// </summary>
        public unsafe void Execution(short* buffer, uint frames)
        {
            if (Binary.ReadBit(IO.NR52, 7))
            {
                for (ushort indexBuffer = 0; indexBuffer < frames * 2; indexBuffer++)
                {
                    // Get channels
                    // ------------

                    short CH1_Value = CH1_GetValue();
                    short CH2_Value = CH2_GetValue();
                    short CH3_Value = CH3_GetValue();
                    short CH4_Value = CH4_GetValue();

                    // Output
                    // ------

                    bool VIN;
                    double volume;

                    // Right
                    if (OutputRight)
                    {
                        CH1_Value = (short)(Binary.ReadBit(IO.NR51, 0) ? CH1_Value : 0);
                        CH2_Value = (short)(Binary.ReadBit(IO.NR51, 1) ? CH2_Value : 0);
                        CH3_Value = (short)(Binary.ReadBit(IO.NR51, 2) ? CH3_Value : 0);
                        CH4_Value = (short)(Binary.ReadBit(IO.NR51, 3) ? CH4_Value : 0);

                        VIN = Binary.ReadBit(IO.NR50, 3);
                        volume = (byte)(((IO.NR50 & 7) + 1) / 8);

                        OutputRight = false;
                    }

                    // Left
                    else
                    {
                        CH1_Value = (short)(Binary.ReadBit(IO.NR51, 4) ? CH1_Value : 0);
                        CH2_Value = (short)(Binary.ReadBit(IO.NR51, 5) ? CH2_Value : 0);
                        CH3_Value = (short)(Binary.ReadBit(IO.NR51, 6) ? CH3_Value : 0);
                        CH4_Value = (short)(Binary.ReadBit(IO.NR51, 7) ? CH4_Value : 0);

                        VIN = Binary.ReadBit(IO.NR50, 7);
                        volume = (byte)((((IO.NR50) >> 4 & 7) + 1) / 8);

                        OutputRight = true;
                    }

                    // Final value
                    short value = (short)((CH1_Value + CH2_Value + CH3_Value + CH4_Value) / 4);
                    buffer[indexBuffer] = (short)(VIN ? value * volume : value);

                    if (Raylib.IsWindowResized())
                    buffer[indexBuffer] = 0;
                }
            }
        }

        #endregion

        #region APU operating variables

        // Precalculations
        private const int AudioFrequency = Audio.Frequency * 2;
        private const double WaveRef = Math.PI * 2;

        // Cycles
        private const double CycleDuration = 1.0f / AudioFrequency;

        // DACs
        private bool DAC1 = false;
        private bool DAC2 = false;
        private bool DAC3 = false;
        private bool DAC4 = false;

        // Wave duty
        private readonly double[] WaveDutyCycle = new double[4] { 12.5f / 100.0f * WaveRef, 25.0f / 100.0f * WaveRef, 50.0f / 100.0f * WaveRef, 75.0f / 100.0f * WaveRef };

        // Volume
        private const double VolumeRatio = 32767.0f / 15.0f;

        #endregion

        #region Channel 1

        // Registers
        // ---------

        private ushort CH1_IndexSample = 0;
        private byte CH1_Pace = 0;
        private bool CH1_Direction = false;
        private byte CH1_IndividualStep = 0;
        private double CH1_SweepTime = 0;
        private byte CH1_WaveForm = 2;
        private byte CH1_InitialLengthTimer = 0;
        private double CH1_WaveLength = 0.00390625;
        private byte CH1_InitialVolume = 0xF;
        private bool CH1_EnvDir = false;
        private byte CH1_SweepPace = 3;
        private double CH1_LengthVolume = 0.046875;
        private double CH1_LengthVolumeElapsed = 0;
        private bool CH1_EnvVolumeEnable = true;
        private byte CH1_LowPeriod = 0xFF;
        private byte CH1_HighPeriod = 7;
        private ushort CH1_Period = 0x7FF;
        private bool CH1_LengthEnable = false;

        // IO write registers
        // ------------------

        /// <summary>
        /// Channel 1 sweep
        /// </summary>
        public void NR10(byte b)
        {
            CH1_Pace = (byte)(b >> 4 & 0x07);
            CH1_Direction = Binary.ReadBit(b, 3);
            CH1_IndividualStep = (byte)(b & 0x07);
            CH1_SweepTime = CH1_Pace * (1.0f / 128.0f);
        }

        /// <summary>
        /// Channel 1 length timer and duty cycle
        /// </summary>
        public void NR11(byte b)
        {
            CH1_InitialLengthTimer = (byte)(b & 0x3F);
            CH1_WaveForm = (byte)(b >> 6 & 3);

            if (CH1_LengthEnable)
            CH1_WaveLength = (64 - CH1_InitialLengthTimer) * (1.0f / 256.0f);
        }

        /// <summary>
        /// Channel 1 volume and envelope
        /// </summary>
        public void NR12(byte b)
        {
            CH1_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH1_EnvDir = Binary.ReadBit(b, 3);
            CH1_SweepPace = (byte)(b & 7);
            CH1_LengthVolume = CH1_SweepPace * (1.0f / 64.0f);
        }

        /// <summary>
        /// Channel 1 period low
        /// </summary>
        public void NR13(byte b)
        {
            CH1_LowPeriod = b;
            CH1_Period = (ushort)(CH1_HighPeriod << 8 | CH1_LowPeriod);
        }

        /// <summary>
        /// Channel 1 period high and control
        /// </summary>
        public void NR14(byte b)
        {
            CH1_HighPeriod = (byte)(b & 7);
            CH1_Period = (ushort)(CH1_HighPeriod << 8 | CH1_LowPeriod);
            CH1_LengthEnable = Binary.ReadBit(b, 6);
            CH1_EnvVolumeEnable = CH1_InitialVolume != 0 && CH1_SweepPace != 0;

            if (Binary.ReadBit(b, 7) && CH1_InitialVolume > 0)
            {
                Binary.SetBit(ref IO.NR52, 0, true);
                DAC1 = true;
            }
        }

        // Get value for x position in buffer
        // ----------------------------------

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
                        Binary.SetBit(ref IO.NR52, 0, false);
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

                if (!CH1_Direction && NewPeriod > 0x7FF)
                {
                    DAC1 = false;
                    CH1_Pace = 0;
                    Binary.SetBit(ref IO.NR52, 0, false);
                    return 0;
                }

                CH1_Period = (ushort)NewPeriod;

                // Volume
                if (CH1_EnvVolumeEnable && CH1_SweepPace != 0)
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
                                Binary.SetBit(ref IO.NR52, 0, false);
                                return 0;
                            }
                        }
                    }
                }

                int Volume = (int)Math.Round(CH1_InitialVolume * VolumeRatio);

                // Apply frequency
                CH1_IndexSample++;

                double Frequency = 131072.0f / (2048 - CH1_Period);
                double Ratio = WaveRef * Frequency / AudioFrequency;
                double Sample = WaveRef / Ratio;

                if (CH1_IndexSample >= Sample)
                CH1_IndexSample = 0;

                return (short)(CH1_IndexSample * Ratio < WaveDutyCycle[CH1_WaveForm] ? 1 * Volume : -1 * Volume);
            }

            return 0;
        }

        #endregion

        #region Channel 2

        // Registers
        // ---------

        private ushort CH2_IndexSample = 0;
        private byte CH2_WaveForm = 0;
        private byte CH2_InitialLengthTimer = 0x3F;
        private double CH2_WaveLength = 0.00390625;
        private byte CH2_InitialVolume = 0;
        private bool CH2_EnvDir = false;
        private byte CH2_SweepPace = 0;
        private double CH2_LengthVolume = 0;
        private double CH2_LengthVolumeElapsed = 0;
        private bool CH2_EnvVolumeEnable = false;
        private byte CH2_LowPeriod = 0xFF;
        private byte CH2_HighPeriod = 7;
        private ushort CH2_Period = 0x7FF;
        private bool CH2_LengthEnable = false;

        // IO write registers
        // ------------------

        /// <summary>
        /// Channel 2 length timer and duty cycle
        /// </summary>
        public void NR21(byte b)
        {
            CH2_InitialLengthTimer = (byte)(b & 0x3F);
            CH2_WaveForm = (byte)(b >> 6 & 3);

            if (CH2_LengthEnable)
            CH2_WaveLength = (64 - CH2_InitialLengthTimer) * (1.0f / 256.0f);
        }

        /// <summary>
        /// Channel 2 volume and envelope
        /// </summary>
        public void NR22(byte b)
        {
            CH2_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH2_EnvDir = Binary.ReadBit(b, 3);
            CH2_SweepPace = (byte)(b & 7);
            CH2_LengthVolume = CH2_SweepPace * (1.0f / 64.0f);
        }

        /// <summary>
        /// Channel 2 period low
        /// </summary>
        public void NR23(byte b)
        {
            CH2_LowPeriod = b;
            CH2_Period = (ushort)(CH2_HighPeriod << 8 | CH2_LowPeriod);
        }

        /// <summary>
        /// Channel 2 period high and control
        /// </summary>
        public void NR24(byte b)
        {
            CH2_HighPeriod = (byte)(b & 7);
            CH2_Period = (ushort)(CH2_HighPeriod << 8 | CH2_LowPeriod);
            CH2_LengthEnable = Binary.ReadBit(b, 6);
            CH2_EnvVolumeEnable = CH2_InitialVolume != 0 && CH2_SweepPace != 0;

            if (Binary.ReadBit(b, 7) && CH2_InitialVolume > 0)
            {
                Binary.SetBit(ref IO.NR52, 1, true);
                DAC2 = true;
            }
        }

        // Get value for x position in buffer
        // ----------------------------------

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
                        Binary.SetBit(ref IO.NR52, 1, false);
                    }
                }

                // Volume
                if (CH2_EnvVolumeEnable && CH2_SweepPace != 0)
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
                                Binary.SetBit(ref IO.NR52, 1, false);
                                return 0;
                            }
                        }
                    }
                }

                int Volume = (int)Math.Round(CH2_InitialVolume * VolumeRatio);

                // Apply frequency
                CH2_IndexSample++;

                double Frequency = 131072.0f / (2048 - CH2_Period);
                double Ratio = WaveRef * Frequency / AudioFrequency;
                double Sample = WaveRef / Ratio;

                if (CH2_IndexSample >= Sample)
                CH2_IndexSample = 0;

                return (short)(CH2_IndexSample * Ratio < WaveDutyCycle[CH2_WaveForm] ? 1 * Volume : -1 * Volume);
            }

            return 0;
        }

        #endregion

        #region Channel 3

        // Registers
        // ---------

        private ushort CH3_IndexSample = 0;
        public bool CH3_DAC_Enable = false;
        private byte CH3_InitialLengthTimer = 0xFF;
        private double CH3_WaveLength = 0.00390625;
        private byte CH3_OutputLevel = 0;
        private byte CH3_LowPeriod = 0xFF;
        private byte CH3_HighPeriod = 7;
        private ushort CH3_Period = 0;
        private bool CH3_LengthEnable = false;

        // IO write registers
        // ------------------

        /// <summary>
        /// Channel 3 DAC enable
        /// </summary>
        public void NR30(byte b)
        {
            CH3_DAC_Enable = Binary.ReadBit(b, 7);
        }

        /// <summary>
        /// Channel 3 length timer
        /// </summary>
        public void NR31(byte b)
        {
            CH3_InitialLengthTimer = b;
            CH3_WaveLength = (256 - CH3_InitialLengthTimer) * (1.0f / 256.0f);
        }

        /// <summary>
        /// Channel 3 output level
        /// </summary>
        public void NR32(byte b)
        {
            CH3_OutputLevel = (byte)((b >> 5) & 3);
        }

        /// <summary>
        /// Channel 3 period low
        /// </summary>
        public void NR33(byte b)
        {
            CH3_LowPeriod = b;
            CH3_Period = (ushort)(CH3_HighPeriod << 8 | CH3_LowPeriod);
        }

        /// <summary>
        /// Channel 3 period high and control
        /// </summary>
        public void NR34(byte b)
        {
            CH3_HighPeriod = (byte)(b & 7);
            CH3_Period = (ushort)(CH3_HighPeriod << 8 | CH3_LowPeriod);
            CH3_LengthEnable = Binary.ReadBit(b, 6);

            if (Binary.ReadBit(b, 7))
            {
                Binary.SetBit(ref IO.NR52, 2, true);
                DAC3 = true;
            }
        }

        // Get value for x position in buffer
        // ----------------------------------

        private short CH3_GetValue()
        {
            if (DAC3 && CH3_DAC_Enable)
            {
                // Delay for stop
                if (CH3_LengthEnable & CH3_WaveLength <= 0)
                {
                    CH3_WaveLength -= CycleDuration;

                    if (CH3_WaveLength <= 0)
                    {
                        CH3_LengthEnable = false;
                        DAC3 = false;
                        Binary.SetBit(ref IO.NR52, 2, false);
                    }
                }

                // Volume
                if (CH3_OutputLevel == 0)
                return 0;

                // Apply frequency
                CH3_IndexSample++;

                double Frequency = 65536.0f / (2048.0f - CH3_Period);
                double Ratio = WaveRef * Frequency / AudioFrequency;
                double Sample = WaveRef / Ratio;

                if (CH3_IndexSample >= Sample)
                CH3_IndexSample = 0;

                double x = CH3_IndexSample * 30.0f / Sample;

                if (x % 2 == 0)
                return (short)((IO.WaveRAM[(byte)x / 2] >> 4) * VolumeRatio / CH3_OutputLevel);
                else
                return (short)((IO.WaveRAM[(byte)x / 2] & 15) * VolumeRatio / CH3_OutputLevel);
            }

            return 0;
        }

        #endregion

        #region Channel 4

        // Noise sound
        // -----------

        private readonly short[] Noise7Bit = new short[44100 * 2];
        private readonly short[] Noise15Bit = new short[44100 * 2];

        private static short[] GenerateNoise(int amplitude, int bitDepth)
        {
            Random random = new();
            short[] noise = new short[44100 * 2];

            for (int i = 0; i < noise.Length; i++)
            {
                int sample = 0;

                for (int j = 0; j < bitDepth; j++)
                    sample |= random.Next(2) << j;

                noise[i] = (short)(sample * amplitude / Math.Pow(2, bitDepth - 1));
            }

            return noise;
        }

        // Registers
        // ---------

        private ushort CH4_IndexSample = 0;
        private byte CH4_InitialLengthTimer = 0x3F;
        private double CH4_WaveLength = 0.00390625;
        private byte CH4_InitialVolume = 0;
        private bool CH4_EnvDir = false;
        private byte CH4_SweepPace = 0;
        private double CH4_LengthVolume = 0;
        private double CH4_LengthVolumeElapsed = 0;
        private ushort CH4_ClockShift = 0;
        private bool CH4_LFSRwidth = false;
        private byte CH4_ClockDivider = 0;
        private double CH4_Frequency = 0;
        private bool CH4_EnvVolumeEnable = false;
        private bool CH4_LengthEnable = false;

        // IO write registers
        // ------------------

        /// <summary>
        /// Channel 4 length timer
        /// </summary>
        public void NR41(byte b)
        {
            CH4_InitialLengthTimer = (byte)(b & 0x3F);
            CH4_WaveLength = (64 - CH4_InitialLengthTimer) * (1.0f / 256.0f);
        }

        /// <summary>
        /// Channel 4 volume and envelope
        /// </summary>
        public void NR42(byte b)
        {
            CH4_InitialVolume = (byte)(b >> 4 & 0x0F);
            CH4_EnvDir = Binary.ReadBit(b, 3);
            CH4_SweepPace = (byte)(b & 7);
            CH4_LengthVolume = CH4_SweepPace * (1.0f / 64.0f);
            CH4_EnvVolumeEnable = CH4_InitialVolume != 0 && CH4_SweepPace != 0;
        }

        /// <summary>
        /// Channel 4 frequency and randomness
        /// </summary>
        public void NR43(byte b)
        {
            CH4_ClockShift = (byte)(b >> 4);
            CH4_LFSRwidth = Binary.ReadBit(b, 3);
            CH4_ClockDivider = (byte)(b & 7);

            CH4_Frequency = 262144.0f / ((CH4_ClockDivider == 0 ? 0.5f : CH4_ClockDivider) * Math.Pow(2, CH4_ClockShift));
            CH4_Frequency /= AudioFrequency;
        }

        /// <summary>
        /// Channel 4 control
        /// </summary>
        public void NR44(byte b)
        {
            CH4_LengthEnable = Binary.ReadBit(b, 6);

            CH4_Frequency = 262144.0f / ((CH4_ClockDivider == 0 ? 0.5f : CH4_ClockDivider) * Math.Pow(2, CH4_ClockShift));
            CH4_Frequency /= AudioFrequency;

            if (Binary.ReadBit(b, 7) && CH4_InitialVolume > 0)
            {
                Binary.SetBit(ref IO.NR52, 3, true);
                DAC4 = true;
            }
        }

        // Get value for x position in buffer
        // ----------------------------------

        private short CH4_GetValue()
        {
            if (DAC4)
            {
                // Delay for stop
                if (CH4_LengthEnable)
                {
                    CH4_WaveLength -= CycleDuration;

                    if (CH4_WaveLength <= 0)
                    {
                        CH4_LengthEnable = false;
                        DAC4 = false;
                        Binary.SetBit(ref IO.NR52, 3, false);
                        return 0;
                    }
                }

                // Volume
                if (CH4_EnvVolumeEnable && CH4_SweepPace != 0)
                {
                    CH4_LengthVolumeElapsed += CycleDuration;

                    if (CH4_LengthVolumeElapsed >= CH4_LengthVolume)
                    {
                        CH4_LengthVolumeElapsed -= CH4_LengthVolume;

                        if (CH4_EnvDir)
                        {
                            if (CH4_InitialVolume < 15)
                            CH4_InitialVolume++;
                            else
                            CH4_SweepPace = 0;
                        }
                        else
                        {
                            if (CH4_InitialVolume > 0)
                            CH4_InitialVolume--;
                            else
                            {
                                DAC4 = false;
                                Binary.SetBit(ref IO.NR52, 3, false);
                                return 0;
                            }
                        }
                    }
                }

                if(CH4_InitialVolume == 0)
                return 0;

                double Volume = 1.0f / (16 - CH4_InitialVolume);

                // Apply frequency
                CH4_IndexSample++;

                double Ratio = WaveRef * CH4_Frequency / AudioFrequency;
                double Sample = WaveRef / Ratio;

                if (CH4_IndexSample >= Sample)
                CH4_IndexSample = 0;

                double x = CH4_IndexSample * (double)AudioFrequency / Sample;

                if (!CH4_LFSRwidth)
                return (short)(Noise7Bit[(int)x] * Volume * 4.0f);
                else
                return (short)(Noise15Bit[(int)x] * Volume * 4.0f);
            }

            return 0;
        }

        #endregion
    }
}
