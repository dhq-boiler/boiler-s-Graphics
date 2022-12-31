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
        public AssemblyErrorLogAttribute(string log)
        {
            Console.Error.WriteLine($"output by AssemblyErrorLogAttribute : {log}");
        }
    }
}
