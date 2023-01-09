using NUnit.Framework;
using System;

namespace boilersGraphics.Test.UITests
{
    public class ConditionalEnvironmentVariableAttribute : Attribute
    {
        public ConditionalEnvironmentVariableAttribute(string environmentVariable) 
        {
            var value = Environment.GetEnvironmentVariable(environmentVariable);
            if (value is null)
            {
                Assert.Ignore($"{environmentVariable} が定義されていません。");
            }
            Console.WriteLine($"Defined Environment Variable {environmentVariable} = {value}");
        }
    }
}
