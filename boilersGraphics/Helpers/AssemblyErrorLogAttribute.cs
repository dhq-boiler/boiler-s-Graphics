using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class AssemblyErrorLogAttribute : Attribute
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public AssemblyErrorLogAttribute(string log)
        {
            logger.Error($"output by AssemblyErrorLogAttribute : {log}");
        }
    }
}
