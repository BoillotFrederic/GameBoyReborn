// ---------
// Audio set
// ---------
using Raylib_cs;
using Emulator;
using System.Runtime.InteropServices;

namespace GameBoyReborn
{
    public class Audio
    {
        // Params
        public const int Frequency = 44100;
        private static AudioStream AudioStream;

        // Init
        unsafe delegate void AudioCallbackDelegate(short* data, uint framesCount);
        public static void Init(Emulation Emulation)
        {
            Raylib.InitAudioDevice();

            AudioStream = Raylib.LoadAudioStream(Frequency, 16, 2);

            Raylib.SetMasterVolume(1);
            Raylib.SetAudioStreamVolume(AudioStream, 1);
            Raylib.PlayAudioStream(AudioStream);

            // Audio loop
            if (Emulation != null)
            unsafe
            {
                AudioCallbackDelegate callback = Emulation.APU.Execution;
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);

                delegate* unmanaged[Cdecl]<void*, uint, void> ptr = (delegate* unmanaged[Cdecl]<void*, uint, void>)callbackPtr.ToPointer();
                Raylib.SetAudioStreamCallback(AudioStream, ptr);
            }
        }

        // Close
        public static void Close()
        {
            Raylib.UnloadAudioStream(AudioStream);
            Raylib.CloseAudioDevice();
        }
    }
}