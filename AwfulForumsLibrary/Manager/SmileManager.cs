using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class SmileManager
    {
        private readonly IWebManager _webManager;

        public SmileManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public SmileManager()
            : this(new WebManager())
        {
        }

        public async Task<List<SmileCategoryEntity>> GetSmileList()
        {
            var smileCategoryList = new List<SmileCategoryEntity>();

            //inject this
            HtmlDocument doc = (await _webManager.GetData(Constants.SmileUrl)).Document;

            IEnumerable<HtmlNode> smileCategoryTitles =
                doc.DocumentNode.Descendants("div")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("inner"))
                    .Descendants("h3");
            List<string> categoryTitles =
                smileCategoryTitles.Select(smileCategoryTitle => WebUtility.HtmlDecode(smileCategoryTitle.InnerText))
                    .ToList();
            IEnumerable<HtmlNode> smileNodes =
                doc.DocumentNode.Descendants("ul")
                    .Where(node => node.GetAttributeValue("class", string.Empty).Contains("smilie_group"));
            int smileCount = 0;
            foreach (HtmlNode smileNode in smileNodes)
            {
                var smileList = new List<SmileEntity>();
                IEnumerable<HtmlNode> smileIcons = smileNode.Descendants("li");
                foreach (HtmlNode smileIcon in smileIcons)
                {
                    var smileEntity = new SmileEntity();
                    smileEntity.Parse(smileIcon);
                    smileList.Add(smileEntity);
                }
                smileCategoryList.Add(new SmileCategoryEntity()
                {
                    Name = categoryTitles[smileCount],
                    SmileList = smileList
                });
                smileCount++;
            }
            return smileCategoryList;
        }
    }
}
