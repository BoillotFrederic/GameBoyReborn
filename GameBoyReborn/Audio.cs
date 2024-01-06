// ---------
// Audio set
// ---------
using Raylib_cs;

namespace GameBoyReborn
{
    public class Audio
    {
        // Params
        private const int MaxSamples = 512;
        public static int MaxSamplesPerUpdate = 4096;
        private const int Frequency = 44100;
        public static short[] AudioBuffer = new short[MaxSamplesPerUpdate];
        private static AudioStream _CH1_AudioStream;
        private static AudioStream _CH2_AudioStream;
        private static AudioStream _CH3_AudioStream;
        private static AudioStream _CH4_AudioStream;
        private static float updateInterval = 0.02f;
        private static float elapsedTime = 0f;

        // Getter / Setter
        public static AudioStream CH1_AudioStream { get { return _CH1_AudioStream; } }

        // Init
        public static void Init()
        {
            Raylib.InitAudioDevice();
            Raylib.SetAudioStreamBufferSizeDefault(MaxSamplesPerUpdate);

            _CH1_AudioStream = Raylib.LoadAudioStream(Frequency, 16, 1);

            Raylib.SetMasterVolume(1);
            Raylib.SetAudioStreamVolume(_CH1_AudioStream, 1);
            Raylib.PlayAudioStream(_CH1_AudioStream);
        }

        // Loop
        public static void Loop()
        {
/*            elapsedTime += Raylib.GetFrameTime();

            if (elapsedTime >= updateInterval)
            {*/
                if (Raylib.IsAudioStreamProcessed(_CH1_AudioStream))
                {
                    unsafe
                    {
                        fixed (short* pData = &Noise()[0])
                        {

                            Raylib.UpdateAudioStream(_CH1_AudioStream, pData, MaxSamplesPerUpdate);
                        }
                    }
                }

/*                elapsedTime = 0f;
            }*/
        }

        // Close
        public static void Close()
        {
            Raylib.UnloadAudioStream(_CH1_AudioStream);
            Raylib.CloseAudioDevice();
        }

        // GameBoy effects
        // ---------------

        // Noise song
        private static short[] Noise()
        {
            Random random = new();

            short[] managedBuffer = new short[MaxSamplesPerUpdate];
            for (int i = 0; i < MaxSamplesPerUpdate; i++)
            managedBuffer[i] = (short)random.Next(-32768, 32767);

            return managedBuffer;
        }

        // Wave duty
        private static short[] WaveDuty(float dutyCycle, int tone)
        {
            double numSamplesPerPeriod = 44100.0 / tone;
            double sampleSize = 256.0f / numSamplesPerPeriod;

            float samples = MaxSamplesPerUpdate / 16.0f;
            float dutyCycleSamples = (float)(samples * dutyCycle);
            short[] managedBuffer = new short[MaxSamplesPerUpdate];

            int sampleStep = 0;

            for (int i = 0; i < MaxSamplesPerUpdate; i++)
            {

                int dutyIndex = (int)(numSamplesPerPeriod == 0 ? 0 : i % numSamplesPerPeriod);

                if(dutyIndex == 0 && i != 0)
                sampleStep += (int)numSamplesPerPeriod;

                if (sampleStep + dutyIndex < i)
                {
                    managedBuffer[i] = managedBuffer[i - 1];
                    sampleStep++;
                    i++;
                }
                else if (sampleStep + dutyIndex < i)
                {
                    sampleStep--;
                    i--;
                }

                managedBuffer[i] = (short)((dutyIndex < sampleSize * dutyCycle) ? 32767 : -32767);
            }
            /*            for (int i = 0; i < MaxSamplesPerUpdate; i++)
                        {
                            managedBuffer[i] = (short)((i % samples < dutyCycleSamples) ? 32767 : -32767);
                        }*/

            return managedBuffer;
        }
    }
}