using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLibrary;

namespace ConvaConsole.Converters
{
    public interface Converter
    {
        bool GetAudioFile();
        string GetName();
        string GetFormat();
        string GetExtensionFormatName();
        int Download(Client<YouTubeVideo> service, string saveDirectory, string videoLink);
    }
}
