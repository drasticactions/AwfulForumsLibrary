using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class ForumManager
    {
        private readonly IWebManager _webManager;

        public ForumManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public ForumManager()
            : this(new WebManager())
        {
        }

        public async Task<List<ForumCategoryEntity>> GetForumCategoryMainPage()
        {
            var forumGroupList = new List<ForumCategoryEntity>();
            var result = await _webManager.GetData(Constants.ForumListPage);
            HtmlDocument doc = result.Document;
            HtmlNode forumNode =
                doc.DocumentNode.Descendants("select")
                    .FirstOrDefault(node => node.GetAttributeValue("name", string.Empty).Equals("forumid"));
            if (forumNode == null)
            {
                throw new ForumListParsingFailedException("Could not download main forum list.");
            }

            try
            {
                IEnumerable<HtmlNode> forumNodes = forumNode.Descendants("option");
                var parentId = 0;
                var order = 1;
                foreach (HtmlNode node in forumNodes)
                {
                    string value = node.Attributes["value"].Value;
                    int id;
                    if (!int.TryParse(value, out id) || id <= -1) continue;
                    if (node.NextSibling.InnerText.Contains("--"))
                    {
                        string forumName =
                            WebUtility.HtmlDecode(node.NextSibling.InnerText.Replace("-", string.Empty));
                        var substringText = node.NextSibling.InnerText.Substring(0, 5);
                        bool isSubforum = substringText.Cast<char>().Count(c => c == '-') > 2;
                        var forumCategory = forumGroupList.LastOrDefault();
                        
                        var forumSubCategory = new ForumEntity
                        {
                            Name = forumName.Trim(),
                            Location = string.Format(Constants.ForumPage, value),
                            IsSubforum = isSubforum,
                            ForumCategory = forumCategory,
                            ForumCategoryEntityId = forumCategory.Id
                        };
                        SetForumId(forumSubCategory);
                        if (!isSubforum)
                        {
                            parentId = forumSubCategory.ForumId;
                        }
                        else
                        {
                            forumSubCategory.ParentForumId = parentId;
                        }
                        
                        forumCategory.ForumList.Add(forumSubCategory);
                    }
                    else
                    {
                        string forumName = WebUtility.HtmlDecode(node.NextSibling.InnerText);
                        var forumGroup = new ForumCategoryEntity
                        {
                            Name = forumName,
                            Location = string.Format(Constants.ForumPage, value),
                            Id = Convert.ToInt32(value),
                            Order = order,
                            ForumList = new List<ForumEntity>()
                        };
                        order++;
                        forumGroupList.Add(forumGroup);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Main Forum Parsing Error: " + ex.StackTrace);
            }

#if DEBUG
            if (forumGroupList.Any())
                forumGroupList[3].ForumList.Add(AddDebugForum());
#endif

            return forumGroupList;
        }

        public static ForumEntity AddDebugForum()
        {
            var forum = new ForumEntity()
            {
                Name = "Apps In Developmental States",
                Location = Constants.BaseUrl + "forumdisplay.php?forumid=261",
                IsSubforum = false
            };
            SetForumId(forum);
            return forum;
        }

        private static void SetForumId(ForumEntity forumEntity)
        {
            if (string.IsNullOrEmpty(forumEntity.Location))
            {
                forumEntity.ForumId = 0;
                return;
            }

            var forumId = forumEntity.Location.Split('=');
            if (forumId.Length > 1)
            {
                forumEntity.ForumId = Convert.ToInt32(forumEntity.Location.Split('=')[1]);
            }
        }
    }
}
