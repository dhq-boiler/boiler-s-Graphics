using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    public static class Path
    {
        public static string GetRoamingDirectory()
        {
            string path = string.Empty;
            try
            {
                var roamingDir = Windows.Storage.ApplicationData.Current.RoamingFolder;
                path = roamingDir.Path;
                LogManager.GetCurrentClassLogger().Debug($"Successed Windows.Storage.ApplicationData.Current.RoamingFolder.");
            }
            catch (Exception)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                LogManager.GetCurrentClassLogger().Debug($"Failed Windows.Storage.ApplicationData.Current.RoamingFolder.");
            }
            return path;
        }
    }
}
