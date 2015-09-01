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
    public class RankEntity
    {
        public int Rank { get; set; }

        public string Change { get; set; }

        public string UserName { get; set; }

        public int UserId { get; set; }

        public virtual void FromTable(List<HtmlNode> data)
        {
            Rank = Convert.ToInt32(data[0].InnerText);
            Change = data[1].InnerText;
            UserName = data[2].InnerText;
            var test = WebUtility.HtmlDecode(data[2].Descendants("b")
                    .FirstOrDefault()
                    .Descendants("a")
                    .FirstOrDefault()
                    .GetAttributeValue("href", string.Empty));
            UserId = GetUserId(test);
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

    public class MostPostsPerHourRankEntity : RankEntity
    {
        public int Posts { get; set; }

        public int Days { get; set; }

        public float PostsPerDay { get; set; }

        public override void FromTable(List<HtmlNode> data)
        {
            base.FromTable(data);
            Posts = Convert.ToInt32(data[3].InnerText);
            Days = Convert.ToInt32(data[4].InnerText);
            PostsPerDay = string.IsNullOrEmpty(data[5].InnerText) ? 0 : Convert.ToInt64(data[5].InnerText);
        }
    }

    public class MostStartedThreadRankEntity : RankEntity
    {
        public int Threads { get; set; }

        public int Views { get; set; }

        public float AverageRating { get; set; }

        public override void FromTable(List<HtmlNode> data)
        {
            base.FromTable(data);
            Threads = Convert.ToInt32(data[3].InnerText);
            Views = Convert.ToInt32(data[4].InnerText);
            AverageRating = string.IsNullOrEmpty(data[5].InnerText) ? 0 : Convert.ToInt64(data[5].InnerText);
        }
    }

    public class GlobalRankEntity : RankEntity
    {
        public int Count { get; set; }

        public override void FromTable(List<HtmlNode> data)
        {
            base.FromTable(data);
            Count = Convert.ToInt32(data[3].InnerText);
        }
    }

}
