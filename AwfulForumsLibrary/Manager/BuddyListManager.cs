using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class BuddyListManager
    {
        private readonly IWebManager _webManager;

        public BuddyListManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public BuddyListManager() : this(new WebManager())
        {
        }

        public async Task<List<BuddyListEntity>> GetBuddyList()
        {
            try
            {
                HtmlDocument doc = (await _webManager.GetData(Constants.UserCp)).Document;

                HtmlNode buddyListNode =
                    doc.DocumentNode.Descendants("div")
                        .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("buddylist"));

                List<BuddyListEntity> list = new List<BuddyListEntity>();

                var onlineNode =
                    buddyListNode.Descendants("dl")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("online"));
                if (onlineNode != null)
                {
                    var users = onlineNode.Descendants("dd");
                    foreach (var user in users)
                    {
                        var entity = new BuddyListEntity();
                        entity.FromUserCp(user, true);
                        list.Add(entity);
                    }
                }

                var offlineNode = buddyListNode.Descendants("dl")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("offline"));

                if (offlineNode != null)
                {
                    var users = offlineNode.Descendants("dd");
                    foreach (var user in users)
                    {
                        var entity = new BuddyListEntity();
                        entity.FromUserCp(user, false);
                        list.Add(entity);
                    }
                }

                return list;
            }
            catch (Exception)
            {
                // Error when parsing. Probably a bad user ID.
                return new List<BuddyListEntity>(0);
            }
        } 
    }
}
