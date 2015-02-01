using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AwfulForumsLibrary.Tools
{
    public static class Extensions
    {
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

        public static Dictionary<string, string> ParseQueryString(String query)
        {
            var queryDict = new Dictionary<string, string>();
            foreach (String token in query.TrimStart(new char[] { '?' }).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    queryDict[parts[0].Trim()] = WebUtility.UrlDecode(parts[1]).Trim();
                else
                    queryDict[parts[0].Trim()] = "";
            }
            return queryDict;
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
