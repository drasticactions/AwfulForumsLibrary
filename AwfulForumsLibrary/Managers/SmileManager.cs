using HtmlAgilityPack;
using AwfulForumsLibrary.Models.Smilies;
using AwfulForumsLibrary.Models.Web;
using AwfulForumsLibrary.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AwfulForumsLibrary.Managers
{
    public class SmileManager
    {
        private readonly WebManager _webManager;

        public SmileManager(WebManager webManager)
        {
            _webManager = webManager;
        }

        public async Task<List<SmileCategory>> GetSmileList()
        {
            var smileCategoryList = new List<SmileCategory>();
            try
            {

                //inject this
                var result = await _webManager.GetData(EndPoints.SmileUrl);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);
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
                    var smileList = new List<Smile>();
                    IEnumerable<HtmlNode> smileIcons = smileNode.Descendants("li");
                    foreach (HtmlNode smileIcon in smileIcons)
                    {
                        var smileEntity = new Smile();
                        smileEntity.Parse(smileIcon);
                        smileList.Add(smileEntity);
                    }
                    smileCategoryList.Add(new SmileCategory()
                    {
                        Name = categoryTitles[smileCount],
                        SmileList = smileList
                    });
                    smileCount++;
                }
            }
            catch (Exception)
            {
                // TODO: Add error handling. For whatever reason serializing the list returns null?
            }
            return smileCategoryList;
        }

    }
}
