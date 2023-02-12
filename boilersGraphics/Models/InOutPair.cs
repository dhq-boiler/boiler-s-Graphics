using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Models
{
    public class InOutPair
    {
        public InOutPair(int @in, int @out)
        {
            In = @in;
            Out = @out;
        }

        public int In { get; set; }
        public int Out { get; set; }

        public override string ToString()
        {
            return $"{In} => {Out}";
        }
    }
}
