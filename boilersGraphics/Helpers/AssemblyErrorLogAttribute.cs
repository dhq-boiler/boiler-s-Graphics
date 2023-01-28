#define DEBUG

using System;
using System.Diagnostics;

namespace boilersGraphics.Helpers;

[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyErrorLogAttribute : Attribute
{
    public AssemblyErrorLogAttribute(string log)
    {
        Debug.Fail(log);
    }
}