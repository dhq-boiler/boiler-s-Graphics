using boilersGraphics.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    public static class GoogleAnalyticsUtil
    {
        public static void Beacon(TerminalInfo terminalInfo, string action)
        {
            try
            {
                GoogleAnalytics.Beacon(terminalInfo.TerminalId.ToString(), GetBuildComposition(), action);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Warn($"GoogleAnalyticsビーコンに失敗しました。");
                LogManager.GetCurrentClassLogger().Warn(ex);
            }
        }

        private static string GetBuildComposition()
        {
            if (App.IsTest)
                return "TEST";
#if DEBUG
            return "Debug";
#else
            return "Production";
#endif
        }
    }
}
