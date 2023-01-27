using System;

namespace boilersGraphics.Helpers;

public class ToolBoxData
{
    public ToolBoxData(string imageUrl, Type type)
    {
        ImageUrl = imageUrl;
        Type = type;
    }

    public string ImageUrl { get; }
    public Type Type { get; }
}