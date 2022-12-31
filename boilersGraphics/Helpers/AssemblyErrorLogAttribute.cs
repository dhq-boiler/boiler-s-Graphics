
#define DEBUG

using System;
using System.Diagnostics;

namespace boilersGraphics.Helpers
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class AssemblyErrorLogAttribute : Attribute
    {
        public AssemblyErrorLogAttribute(string log)
        {
            Debug.Fail(log);
        }
    }
}
