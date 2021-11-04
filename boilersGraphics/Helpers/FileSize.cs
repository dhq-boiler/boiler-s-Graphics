using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    public static class FileSize
    {
        public static string ConvertFileSizeUnit(long bytes)
        {
            var kbytes = bytes / 1024d;
            if (kbytes < 1.0)
                return $"{bytes:#,0} bytes";
            if (kbytes < 10)
                return $"{kbytes:#,0.0} KB";
            if (kbytes < 100)
                return $"{kbytes:#,0} KB";
            var mbytes = kbytes / 1024d;
            if (mbytes < 1.0)
                return $"{kbytes:#,0} KB";
            if (mbytes < 10)
                return $"{mbytes:#,0.0} MB";
            if (mbytes < 100)
                return $"{mbytes:#,0} MB";
            var gbytes = mbytes / 1024d;
            if (gbytes < 1.0)
                return $"{mbytes:#,0} MB";
            if (gbytes < 10)
                return $"{gbytes:#,0.0} GB";
            return $"{gbytes:#,0} GB";
        }
    }
}
