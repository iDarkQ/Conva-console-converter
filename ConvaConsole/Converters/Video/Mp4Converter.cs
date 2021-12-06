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
    public class Mp4Converter : Converter
    {
        public int Download(Client<YouTubeVideo> service, string saveDirectory, string videoLink)
        {
            string folder = Program.TEMP_PATH;
            Console.WriteLine(folder);
            var videoInfos = service.GetAllVideos(videoLink);

            YouTubeVideo video = null;
            YouTubeVideo audio = null;
            var resultVideo = video;
            var resultAudio = audio;

            foreach (YouTubeVideo youtubeVid in videoInfos)
            {
                if(youtubeVid.Format != VideoFormat.Mp4)
                {
                    continue;
                }

                if (resultVideo == null || resultVideo.Resolution < youtubeVid.Resolution)
                {
                    resultVideo = youtubeVid;
                }
            }

            if (resultVideo == null)
            {
                Console.WriteLine("Video could not be found");
                return ErrorCodes.VIDEO_NOT_FOUND;
            }

            resultAudio = videoInfos.First(i =>
                (i.AudioBitrate == videoInfos.Max(j => j.AudioBitrate) && (i.AudioFormat == AudioFormat.Aac)
            ));

            if (resultAudio == null)
            {
                Console.WriteLine("Audio track could not be found");
                return ErrorCodes.AUDIO_NOT_FOUND;
            }

            string format = FormatExtension.AudioFormats.First(k => k.Key == resultAudio.AudioFormat.ToString()).Value;

            File.WriteAllBytes(folder + resultVideo.FullName, resultVideo.GetBytes());
            Console.WriteLine("Video downloaded...");

            File.WriteAllBytes(folder + resultAudio.FullName + "." + format, resultAudio.GetBytes());
            Console.WriteLine("Audio downloaded...");

            string videoFullName = resultVideo.FullName;

            Console.WriteLine("Merging video and audio started...");

            FFMpeg.ReplaceAudio(folder + videoFullName, folder + resultAudio.FullName + "." + format, Program.RESULT_PATH + videoFullName);

            Console.WriteLine("Deleting useless files...");

            File.Delete(folder + resultVideo.FullName);
            File.Delete(folder + resultAudio.FullName + "." + format);

            return -1;
        }

        public string GetExtensionFormatName()
        {
            return "mp4";
        }

        public string GetFormat()
        {
            return "Mp4";
        }

        public string GetName()
        {
            return "Mp4 Converter";
        }

        public bool GetAudioFile()
        {
            return false;
        }
    }
}
