using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace AwfulForumsLibrary.Tools
{
    public static class Extensions
    {
		public static string RemoveAutoplayGif(string html)
		{
			var doc2 = new HtmlDocument();
			doc2.LoadHtml(html);
			HtmlNode bodyNode = doc2.DocumentNode.Descendants("body").FirstOrDefault();
			var images = bodyNode.Descendants("img").Where(node => node.GetAttributeValue("class", string.Empty) != "av");
			foreach (var image in images)
			{
				var src = image.Attributes["src"].Value;
				if (Path.GetExtension(src) != ".gif")
					continue;
				if (src.Contains("somethingawful.com"))
					continue;
				if (src.Contains("emoticons"))
					continue;
				if (src.Contains("smilies"))
					continue;
				image.Attributes.Add("data-gifffer", image.Attributes["src"].Value);
				image.Attributes.Remove("src");
			}
			html = doc2.DocumentNode.OuterHtml;
			return html;
		}
        public static Dictionary<string, string> ParseQueryString(string s)
        {
            var nvc = new Dictionary<string, string>();

            // remove anything other than query string from url
            if (s.Contains("?"))
            {
                s = s.Substring(s.IndexOf('?') + 1);
            }

            foreach (string vp in Regex.Split(s, "&"))
            {
                string[] singlePair = Regex.Split(vp, "=");
                if (singlePair.Length == 2)
                {
                    nvc.Add(singlePair[0], singlePair[1]);
                }
                else
                {
                    // only one key with no value specified in query string
                    nvc.Add(singlePair[0], string.Empty);
                }
            }

            return nvc;
        }

        public static CookieContainer ReadCookies(this HttpResponseMessage response)
        {
            var pageUri = response.RequestMessage.RequestUri;

            var cookieContainer = new CookieContainer();
            IEnumerable<string> cookies;
            if (response.Headers.TryGetValues("set-cookie", out cookies))
            {
                foreach (var c in cookies)
                {
                    cookieContainer.SetCookies(pageUri, c);
                }
            }

            return cookieContainer;
        }

        public static string WithoutNewLines(this string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (char i in text)
            {
                if (i != '\n' && i != '\r' && i != '\t' && i != '#' && i != '?')
                {
                    sb.Append(i);
                }
                else if (i == '\n')
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }

        public static string HtmlEncode(string text)
        {
            // In order to get Unicode characters fully working, we need to first encode the entire post.
            // THEN we decode the bits we can safely pass in, like single/double quotes.
            // If we don't, the post format will be screwed up.
            char[] chars = WebUtility.HtmlEncode(text).ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            result.Replace("&quot;", "\"");
            result.Replace("&#39;", @"'");
            result.Replace("&lt;", @"<");
            result.Replace("&gt;", @">");
            return result.ToString();
        }
    }
}
