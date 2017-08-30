using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AwfulForumsLibrary.Models.Smilies
{
    public class SmileCategory
    {
        public SmileCategory()
        {
            SmileList = new List<Smile>();
        }

        public List<Smile> SmileList { get; set; }

        public string Name { get; set; }
    }

    public class Smile
    {
        public string Title { get; private set; }

        public string ImageUrl { get; private set; }

        public void Parse(HtmlNode smileNode)
        {
            Title = smileNode.Descendants("div").First().InnerText;
            ImageUrl = smileNode.Descendants("img").First().GetAttributeValue("src", string.Empty);
        }
    }
}
