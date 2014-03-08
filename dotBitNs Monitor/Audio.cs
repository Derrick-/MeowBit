using System;
using System.Runtime.InteropServices;
using System.Resources;
using System.IO;

namespace dotBitNs_Monitor
{
    public static class Audio
    {

        public static void PlaySuccess()
        {
            using(Stream str = Properties.Resources.Blop)
                PlayWavResource(str);
        }

        public static void PlayFail()
        {
            using (Stream str = Properties.Resources.Woosh)
                PlayWavResource(str);
        }


        public const UInt32 SND_ASYNC = 1;
        public const UInt32 SND_MEMORY = 4;
        // these 2 overloads we dont need ... 
        // [DllImport("Winmm.dll")]
        // public static extern bool PlaySound(IntPtr rsc, IntPtr hMod, UInt32 dwFlags); 
        // [DllImport("Winmm.dll")]
        // public static extern bool PlaySound(string Sound, IntPtr hMod, UInt32 dwFlags);

        // this is the overload we want to play embedded resource...
        [DllImport("Winmm.dll")]
        public static extern bool PlaySound(byte[] data, IntPtr hMod, UInt32 dwFlags);

        public static void PlayWavResource(Stream str)
        {
            if (str == null)
                return;
            // bring stream into a byte array
            byte[] bStr = new Byte[str.Length];
            str.Read(bStr, 0, (int)str.Length);
            // play the resource
            PlaySound(bStr, IntPtr.Zero, SND_ASYNC | SND_MEMORY);
        }
    }
}
