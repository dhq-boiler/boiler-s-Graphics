using boilersGraphics.Models;
using NLog;
using System;
using System.Text;

namespace boilersGraphics.Helpers
{
    public static class GoogleAnalyticsUtil
    {
        public static void Beacon(TerminalInfo terminalInfo, string action, string label = null)
        {
            try
            {
                GoogleAnalytics.Beacon(terminalInfo.TerminalId.ToString(), GetBuildComposition(), action, label);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Warn($"GoogleAnalyticsビーコンに失敗しました。");
                LogManager.GetCurrentClassLogger().Warn(ex);
            }
        }

        public static string GetStringLimit500Bytes(string message)
        {
            while (Encoding.UTF8.GetByteCount(message) > 500)
            {
                message = message.Substring(0, message.Length - 1);
            }
            return message;
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
