using boilersE2E;
using boilersE2E.NUnit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests
{
    public class E2ETest : E2ETestFixture
    {
        public override string AppPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boilersGraphics.exe");

        public override Size WindowSize => new Size(1980, 1080);

        static E2ETest()
        {
            boilersE2ETestEnvironmentVariableName = "BOILERSGRAPHICS_TEST_IS_VALID";
        }
    }
}
