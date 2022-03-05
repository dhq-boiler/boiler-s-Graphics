using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace boilersGraphics.Helpers
{
    internal static class GoogleAnalytics
    {
        public static void Beacon(string uniqueUserIdentifier, string category, string action, string path, string label = null)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://www.google-analytics.com/collect");
            request.Method = "POST";

            // the request body we want to send 
            var postData = new Dictionary<string, string>
            {
                 { "v", "1" }, //analytics protocol version 
                 { "tid", "UA-217962720-1" }, //analytics tracking property id 
                 { "cid", uniqueUserIdentifier }, //unique user identifier 
                 { "t", "pageview" }, //event type 
                 { "ec", category },
                 { "ea", action },
                 { "dp", path }
            };
            if (!string.IsNullOrEmpty(label))
            {
                postData.Add("el", label);
            }

            var postDataString = postData
             .Aggregate("", (data, next) => string.Format("{0}&{1}={2}", data, next.Key,
                        Uri.EscapeDataString(next.Value)))
             .TrimEnd('&');

            // set the Content-Length header to the correct value 
            request.ContentLength = Encoding.UTF8.GetByteCount(postDataString);

            // write the request body to the request 
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(postDataString);
            }

            var webResponse = (HttpWebResponse)request.GetResponse();
            if (webResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Google Analytics tracking did not return OK 200. Returned: {webResponse.StatusCode}");
            }
        }
    }
}
