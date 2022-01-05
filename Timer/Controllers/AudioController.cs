using System;
using System.IO;
using System.Windows.Media;

namespace MainSpace.Controllers
{
    public static class AudioController
    {
        private static MediaPlayer _audio;

        static AudioController()
        {
            _audio = new MediaPlayer();
        }

        public static void Play()
        {
            _audio.Open(new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "beep.wav")));
            _audio.Play();
        }
    }
}
