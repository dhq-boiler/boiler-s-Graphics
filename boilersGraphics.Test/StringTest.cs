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
    }
}
