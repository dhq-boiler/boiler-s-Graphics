using System;

namespace boilersGraphics.Helpers;

[Serializable]
public class ClipboardDTO
{
    public ClipboardDTO(string root)
    {
        Root = root;
    }

    public string Root { get; set; }
}