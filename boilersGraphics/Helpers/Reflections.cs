using System;
using System.Diagnostics;
using System.Reflection;

namespace boilersGraphics.Helpers
{
    /// <summary>
    /// How to get Class name that is calling my method? [duplicate]
    /// https://stackoverflow.com/questions/48570573/how-to-get-class-name-that-is-calling-my-method
    /// answered Mehmet Topçu
    /// </summary>
    public static class Reflections
    {
        public static string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.FullName;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }
    }
}
