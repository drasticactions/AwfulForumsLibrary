using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interfaces;
using AwfulForumsLibrary.Models.Forums;
using AwfulForumsLibrary.Models.Web;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AwfulForumsLibrary.Managers
{
    public class ForumManager
    {
        private readonly IWebManager _webManager;

        public ForumManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public async Task<Result> GetForumCategoriesAsync(bool parseToJson = true)
        {
            var result = new Result();
            try
            {
                result = await _webManager.GetData(EndPoints.ForumListPage);
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, "Failed to download forum list", ex.StackTrace, ex.GetType().FullName);
            }
            if (!result.IsSuccess) return result;

            // Got the forum list HTML!
            result.Type = typeof (Category).ToString();

            if (!parseToJson)
                return result;

            try
            {
                result.ResultJson = ParseCategoryList(result.ResultHtml);
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, "Failed to parse forum list", ex.StackTrace, ex.GetType().FullName);
            }

            return result;
        }

        private string ParseCategoryList(string html)
        {
            var forumGroupList = new List<Category>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var forumNode =
                doc.DocumentNode.Descendants("select")
                    .FirstOrDefault(node => node.GetAttributeValue("name", String.Empty).Equals("forumid"));
            if (forumNode == null)
            {
                throw new ForumListParsingFailedException("Could not download main forum list.");
            }

            try
            {
                var forumNodes = forumNode.Descendants("option");
                var parentId = 0;
                var order = 1;
                foreach (var node in forumNodes)
                {
                    var value = node.Attributes["value"].Value;
                    int id;
                    if (!int.TryParse(value, out id) || id <= -1) continue;
                    if (node.NextSibling.InnerText.Contains("--"))
                    {
                        var forumName =
                            WebUtility.HtmlDecode(node.NextSibling.InnerText.Replace("-", String.Empty));
                        var substringText = node.NextSibling.InnerText.Substring(0, 5);
                        var isSubforum = substringText.Cast<char>().Count(c => c == '-') > 2;
                        var forumCategory = forumGroupList.LastOrDefault();

                        var forumSubCategory = new Forum
                        {
                            Name = forumName.Trim(),
                            Location = String.Format(EndPoints.ForumPage, value),
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
                        var forumName = WebUtility.HtmlDecode(node.NextSibling.InnerText);
                        var forumGroup = new Category()
                        {
                            Name = forumName,
                            Location = String.Format(EndPoints.ForumPage, value),
                            Id = Convert.ToInt32(value),
                            Order = order,
                            ForumList = new List<Forum>()
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

            return JsonConvert.SerializeObject(forumGroupList);
        }

        public static Forum AddDebugForum()
        {
            var forum = new Forum()
            {
                Name = "Apps In Developmental States",
                Location = EndPoints.BaseUrl + "forumdisplay.php?forumid=261",
                IsSubforum = false
            };
            SetForumId(forum);
            return forum;
        }

        private static void SetForumId(Forum forumEntity)
        {
            if (String.IsNullOrEmpty(forumEntity.Location))
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
