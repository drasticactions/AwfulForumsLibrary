using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class RapSheetManager
    {
        private readonly IWebManager _webManager;

        public RapSheetManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public RapSheetManager() : this(new WebManager())
        {
        }

        public async Task<List<ForumUserRapSheetEntity>> GetRapSheet(int pageNumber)
        {
            try
            {
                HtmlDocument doc = (await _webManager.GetData(string.Format(Constants.RapSheet, pageNumber))).Document;

                HtmlNode rapSheetNode =
                    doc.DocumentNode.Descendants("table")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("standard full"));
                rapSheetNode.Descendants("tr").FirstOrDefault().Remove();

                rapSheetNode.SetAttributeValue("style", "width: 100%");
                List<ForumUserRapSheetEntity> list = new List<ForumUserRapSheetEntity>();
                foreach (var node in rapSheetNode.Descendants("tr"))
                {
                    var rapSheet = new ForumUserRapSheetEntity();
                    rapSheet.FromRapSheet(node);
                    list.Add(rapSheet);
                }
                return list;
            }
            catch (Exception)
            {
                // Error when parsing. Probably a bad user ID.
                return new List<ForumUserRapSheetEntity>(0);
            }

        }
    }
}
