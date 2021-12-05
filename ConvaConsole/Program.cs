// See https://aka.ms/new-console-template for more information
using ConvaConsole.Converters;
using FFMpegCore;
using FFMpegCore.Enums;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;
using System.Reflection;
using VideoLibrary;

namespace ConvaConsole;

class Program
{

    static ConverterManager converterManager;
    static bool finished = false;
    public static string TEMP_PATH = Path.GetTempPath() + @"ConvaConverterTemp\";
    public static string RESULT_PATH = @"C:\Users\matik\Videos\";

    static void Main()
    {
        initDicionaries();

        converterManager = new ConverterManager();
        converterManager.init();

        using (var service = Client.For(YouTube.Default))
        {
            while (true)
            {
                Console.WriteLine("Enter youtube video link: ");
                string videoLink = Console.ReadLine();

                try
                {

                    Converter resultConverter = null;

                    Console.WriteLine("Type id to choose format:");

                    while (true)
                    {
                        foreach (int converterId in converterManager.GetConverters().Keys)
                        {
                            Converter converter = converterManager.GetConverters()[converterId];
                            Console.WriteLine(converterId + ": " + converter.GetName());
                        }

                        string id = Console.ReadLine();
                        Converter conv = converterManager.GetConverter(Int32.Parse(id));

                        if (id != null && conv != null)
                        {
                            resultConverter = conv;
                            break;
                        }
                    }

                    if(resultConverter != null)
                    {
                        Console.WriteLine("Downloading started...");
                        int result = resultConverter.Download(service, RESULT_PATH, videoLink);

                        switch(result)
                        {
                            case -1:
                                Console.WriteLine("Sucessfully downloaded!\n");
                                break;
                            case 0:
                                Console.WriteLine("Error occured! Couldn't find video in this format!\n");
                                break;
                            case 1:
                                Console.WriteLine("Error occured! Couldn't find audio in this format!\n");
                                break;
                        }
                    }

                    //Console.WriteLine("Downloading started...");
                    //YouTubeVideo? downloadedVideo = DownloadVideo(service, TEMP_PATH + "\\", videoLink);

                    //if (downloadedVideo != null){
                    //    Console.WriteLine("Sucessfully downloaded " + downloadedVideo.FullName + "\n");
                    //}
                }
                catch (ArgumentException exception)
                {
                    Console.WriteLine("An error occurred while searching for a film: " + exception.Message);
                    continue;
                }
            }
        }
    }

    static void initDicionaries()
    {
        var dir = new DirectoryInfo(TEMP_PATH);
        foreach (var file in Directory.GetFiles(dir.ToString()))
        {
            File.Delete(file);
        }

        Directory.CreateDirectory(TEMP_PATH);
    }

    static YouTubeVideo? DownloadVideo(Client<VideoLibrary.YouTubeVideo> service, string folder, string videoLink)
    {
        var videoInfos = service.GetAllVideos(videoLink);

        YouTubeVideo video = null;
        YouTubeVideo audio = null;
        var resultVideo = video;
        var resultAudio = audio;

        foreach (YouTubeVideo youtubeVid in videoInfos)
        {
            if(youtubeVid.Resolution > 1080){
                continue;
            }

            if(resultVideo == null || resultVideo.Resolution < youtubeVid.Resolution) {
                resultVideo = youtubeVid;
            }
        }

        if(resultVideo == null)
        {
            Console.WriteLine("Video could not be found");
            return null;
        }

        switch(resultVideo.Format)
        {
            case VideoFormat.Mp4:
                resultAudio = videoInfos.First(i =>
                    (i.AudioBitrate == videoInfos.Max(j => j.AudioBitrate) && (i.AudioFormat == AudioFormat.Aac)
                ));
                break;

            case VideoFormat.WebM:
                resultAudio = videoInfos.First(i =>
                    (i.AudioBitrate == videoInfos.Max(j => j.AudioBitrate) && (i.AudioFormat == AudioFormat.Opus || i.AudioFormat == AudioFormat.Vorbis)
                ));
                break;
        }

        if(resultAudio == null)
        {
            Console.WriteLine("Audio track could not be found");
            return null;
        }

        string format = FormatExtension.AudioFormats.First(k => k.Key == resultAudio.AudioFormat.ToString()).Value;

        File.WriteAllBytes(folder + resultVideo.FullName, resultVideo.GetBytes());
        Console.WriteLine("Video downloaded...");

        File.WriteAllBytes(folder + resultAudio.FullName + "." + format, resultAudio.GetBytes());
        Console.WriteLine("Audio downloaded...");

        string videoFullName = resultVideo.FullName;

        if (resultVideo.Format == VideoFormat.WebM)
        {
            videoFullName = resultVideo.FullName.Replace("." + resultVideo.Format.ToString(), ".mp4");
            Console.WriteLine("Converting to mp4 started...");
            FFMpegCore.FFMpeg.Convert(folder + resultVideo.FullName, folder + videoFullName, FFMpeg.GetContainerFormat("mp4"));
        }

        Console.WriteLine("Merging video and audio started...");

        FFMpeg.ReplaceAudio(folder + videoFullName, folder + resultAudio.FullName + "." + format, RESULT_PATH + videoFullName);

        Console.WriteLine("Deleting useless files...");

        File.Delete(folder + resultVideo.FullName);
        File.Delete(folder + resultAudio.FullName + "." + format);

        return resultVideo;
    }

    static void ResetDownload()
    {
        finished = false;
    }
}