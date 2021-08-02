using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    [Serializable]
    public class ClipboardDTO
    {
        public string Root { get; set; }

        public ClipboardDTO(string root)
        {
            Root = root;
        }
    }
}
