using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Entity
{
    public class BuddyListEntity
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public bool IsOnline { get; set; }

        public void FromUserCp(HtmlNode userNode, bool isOnline)
        {
            var userInfo = userNode.Descendants("a").First();
            UserName = WebUtility.HtmlDecode(userInfo.InnerText);
            IsOnline = isOnline;
            UserId = GetUserId(userInfo.GetAttributeValue("href", string.Empty));
        }

        private int GetUserId(string txt)
        {
            var re1 = ".*?";
            var re2 = "(\\d+)";

            var r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var m = r.Match(txt);
            if (!m.Success) return 0;
            var int1 = m.Groups[1].ToString();
            return Convert.ToInt32(int1);
        }
    }
}
