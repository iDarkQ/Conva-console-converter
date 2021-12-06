using ConvaConsole.Converters.Video;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvaConsole.Converters
{
    public class ConverterManager
    {
        SortedDictionary<int, Converter> converters = new SortedDictionary<int, Converter> { };

        public void init()
        {
            converters.Add(0, new Mp4Converter());
            converters.Add(1, new Mp3Converter());
        }

        public Converter? GetConverter(string converterName)
        {
            foreach(Converter converter in converters.Values) {
                if(converter.GetName().Equals(converterName))
                {
                    return converter;
                }
            }

            return null;
        }

        public Converter? GetConverter(int converterId)
        {
            foreach (int id in converters.Keys)
            {
                if (id.Equals(converterId))
                {
                    return converters[id];
                }
            }

            return null;
        }

        public SortedDictionary<int, Converter> GetConverters()
        {
            return converters;
        }
    }
}
