using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    static class LoggerHelper
    {
        public static Logger GetLogger()
        {
            return LogManager.GetCurrentClassLogger();
        }
    }
}
