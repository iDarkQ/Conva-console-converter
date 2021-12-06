using ConvaConsole.Utils;
using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace ConvaConsole.Converters.Video
{
    public class Mp3Converter : Converter
    {
        public int Download(Client<YouTubeVideo> service, string saveDirectory, string videoLink)
        {
            string folder = Program.RESULT_PATH;
            Console.WriteLine(folder);
            var videoInfos = service.GetAllVideos(videoLink);

            YouTubeVideo audio = null;
            var resultAudio = audio;

            resultAudio = videoInfos.First(i =>
                (i.AudioBitrate == videoInfos.Max(j => j.AudioBitrate) && (i.AudioFormat == AudioFormat.Aac)
            ));

            if (resultAudio == null)
            {
                Console.WriteLine("Audio track could not be found");
                return ErrorCodes.AUDIO_NOT_FOUND;
            }

            string format = FormatExtension.AudioFormats.First(k => k.Key == resultAudio.AudioFormat.ToString()).Value;
            string fileName = Path.GetFileNameWithoutExtension(resultAudio.FullName);
            File.WriteAllBytes(folder + fileName + ".mp3", resultAudio.GetBytes());
            Console.WriteLine("Audio downloaded...");
            return -1;
        }

        public string GetExtensionFormatName()
        {
            return "mp3";
        }

        public string GetFormat()
        {
            return "Mp3";
        }

        public string GetName()
        {
            return "Mp3 Converter";
        }

        public bool GetAudioFile()
        {
            return true;
        }
    }
}
