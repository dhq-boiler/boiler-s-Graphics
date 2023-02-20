using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class StringTest
    {
        [Test]
        public void 検索値から最後まで取得()
        {
            const string s = "abcdefghijklmnopqrstuvwxyz";
            var index = s.IndexOf("k");
            Assert.That(string.Concat(s.Skip(index).Take(s.Count() - index)), Is.EqualTo("klmnopqrstuvwxyz"));
        }

        [Test]
        public void Convert_ToBase64String()
        {
            const string s1 = "abc";
            const string s2 = "def";
            const string s3 = "abcdef";
            var stringBuilder1 = new StringBuilder();
            stringBuilder1.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(s1)));
            stringBuilder1.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(s2)));

            var stringBuilder2 = new StringBuilder();
            stringBuilder2.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(s3)));

            Assert.That(stringBuilder1.ToString(), Is.EqualTo(stringBuilder2.ToString()));
        }
    }
}
