using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Entity
{
    public class SmileCategoryEntity
    {
        public SmileCategoryEntity()
        {
            SmileList = new List<SmileEntity>();
        }

        public virtual ICollection<SmileEntity> SmileList { get; set; }

        public string Name { get; set; }
    }

    public class SmileEntity
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
