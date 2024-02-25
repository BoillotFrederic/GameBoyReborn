// ---------
// Audio set
// ---------
using Raylib_cs;
using Emulator;

#pragma warning disable CS0414
#pragma warning disable CS0169

namespace GameBoyReborn
{
    public class Audio
    {
        // Params
        private static Emulation? Emulation;
        public const int MaxSamplesPerUpdate = 2048;
        public const int Frequency = 44100;
        public static short[] AudioBuffer = new short[MaxSamplesPerUpdate];
        private static AudioStream _AudioStream;

        // Getter / Setter
        public static AudioStream AudioStream { get { return _AudioStream; } }

        // Init
        public static void Init(Emulation? _Emulation)
        {
            Emulation = _Emulation;

            Raylib.InitAudioDevice();
            Raylib.SetAudioStreamBufferSizeDefault(MaxSamplesPerUpdate);

            _AudioStream = Raylib.LoadAudioStream(Frequency, 16, 1);

            Raylib.SetMasterVolume(1);
            Raylib.SetAudioStreamVolume(_AudioStream, 1);
            Raylib.PlayAudioStream(_AudioStream);
        }

        // Loop
        public static void Loop()
        {
            if (Emulation != null && Raylib.IsAudioStreamProcessed(_AudioStream))
            {
                Emulation.APU.Execution();

                unsafe
                {
                    fixed (short* pData = &AudioBuffer[0])
                    {
                        Raylib.UpdateAudioStream(_AudioStream, pData, MaxSamplesPerUpdate);
                    }
                }
            }
        }

        // Close
        public static void Close()
        {
            Raylib.UnloadAudioStream(_AudioStream);
            Raylib.CloseAudioDevice();
        }

 /*       // GameBoy effects
        // ---------------

        public static short[] ApplyReverb(short[] input, int delayMilliseconds, float attenuation)
        {
            int delaySamples = (int)(delayMilliseconds * Frequency / 1000.0f);
            short[] output = new short[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (i + delaySamples < input.Length)
                {
                    output[i] += (short)(input[i + delaySamples] * attenuation);
                }
            }

            return output;
        }

        // Noise song
        private static short[] Noise()
        {
            Random random = new();

            short[] managedBuffer = new short[MaxSamplesPerUpdate];
            for (int i = 0; i < MaxSamplesPerUpdate; i++)
            managedBuffer[i] = (short)random.Next(-32768, 32767);

            return managedBuffer;
        }

        // Wave song test
        private static int IndexSample = 0;
        private static short[] Wave()
        {
            double[] WaveDutyCycle = new double[4] { 12.5f / 100.0f * Math.PI, 25.0f / 100.0f * Math.PI, 50.0f / 100.0f * Math.PI, 75.0f / 100.0f * Math.PI };

            short[] managedBuffer = new short[MaxSamplesPerUpdate];
            for (int i = 0; i < MaxSamplesPerUpdate; i++)
            {
                // Apply frequency
                IndexSample++;

                double Frequency = 131072.0f / (2048 - 1000);
                double Ratio = Math.PI * Frequency / Audio.Frequency;
                double Sample = Math.PI / Ratio;
                managedBuffer[i] = (short)(IndexSample * Ratio < WaveDutyCycle[3] ? 32767 : -32767);

                if (IndexSample >= Sample)
                IndexSample = 0;
            }

            return managedBuffer;
        }*/
    }
}