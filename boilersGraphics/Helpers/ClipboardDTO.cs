namespace boilersGraphics.Helpers;

public class ClipboardDTO
{
    public const string ClipboardFormat = "boilersGraphics.ClipboardDTO";

    public ClipboardDTO(string root)
    {
        Root = root;
    }

    public string Root { get; set; }
}