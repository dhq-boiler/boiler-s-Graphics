using NLog;
using System;
using Windows.Storage;

namespace boilersGraphics.Helpers;

public static class Path
{
    public static string GetRoamingDirectory()
    {
        var path = string.Empty;
        try
        {
            var roamingDir = ApplicationData.Current.RoamingFolder;
            path = roamingDir.Path;
            LogManager.GetCurrentClassLogger()
                .Debug("Successed Windows.Storage.ApplicationData.Current.RoamingFolder.");
        }
        catch (Exception)
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            LogManager.GetCurrentClassLogger().Debug("Failed Windows.Storage.ApplicationData.Current.RoamingFolder.");
        }

        return path;
    }
}