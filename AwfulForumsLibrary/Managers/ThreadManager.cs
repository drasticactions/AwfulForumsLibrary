using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interfaces;
using AwfulForumsLibrary.Models.Forums;
using AwfulForumsLibrary.Models.Posts;
using AwfulForumsLibrary.Models.Threads;
using AwfulForumsLibrary.Models.Web;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace AwfulForumsLibrary.Managers
{
    public class ThreadManager
    {
        private readonly IWebManager _webManager;

        public ThreadManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public async Task<bool> MarkThreadUnreadAsync(long threadId)
        {
            var dic = new Dictionary<string, string>
            {
                ["json"] = "1",
                ["action"] = "resetseen",
                ["threadid"] = threadId.ToString()
            };
            var header = new FormUrlEncodedContent(dic);
            await _webManager.PostData(EndPoints.ResetBase, header);
            return true;
        }

        public async Task<Result> GetBookmarksAsync(int page)
        {
            var forumThreadList = new List<Thread>();
            var forum = new Forum()
            {
                Name = "Bookmarks",
                IsSubforum = false,
                Location = EndPoints.UserCp
            };
            String url = EndPoints.BookmarksUrl;
            if (page >= 0)
            {
                url = EndPoints.BookmarksUrl + string.Format(EndPoints.PageNumber, page);
            }

            var result = (await _webManager.GetData(url));
            var doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);

            HtmlNode forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));


            foreach (
                HtmlNode threadNode in
                    forumNode.Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
            {
                var threadEntity = new Thread { ForumId = 0, IsBookmark = true };
                ParseThreadHtml(threadEntity, threadNode);
                forumThreadList.Add(threadEntity);
            }
            result.ResultJson = JsonConvert.SerializeObject(forumThreadList);
            return result;
        }

        public async Task<Result> AddBookmarkAsync(long threadId)
        {
            var dic = new Dictionary<string, string>
            {
                ["json"] = "1",
                ["action"] = "add",
                ["threadid"] = threadId.ToString()
            };
            var header = new FormUrlEncodedContent(dic);
            return await _webManager.PostData(EndPoints.Bookmark, header);
        }

        public async Task<Result> RemoveBookmarkAsync(long threadId)
        {
            var dic = new Dictionary<string, string>
            {
                ["json"] = "1",
                ["action"] = "remove",
                ["threadid"] = threadId.ToString()
            };
            var header = new FormUrlEncodedContent(dic);
            return await _webManager.PostData(EndPoints.Bookmark, header);
        }

        public async Task<NewThread> GetThreadCookiesAsync(int forumId)
        {
            try
            {
                string url = string.Format(EndPoints.NewThread, forumId);
                var result = await _webManager.GetData(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);
                HtmlNode[] formNodes = doc.DocumentNode.Descendants("input").ToArray();

                HtmlNode formKeyNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("formkey"));

                HtmlNode formCookieNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("form_cookie"));

                var newForumEntity = new NewThread();
                try
                {
                    string formKey = formKeyNode.GetAttributeValue("value", "");
                    string formCookie = formCookieNode.GetAttributeValue("value", "");
                    newForumEntity.FormKey = formKey;
                    newForumEntity.FormCookie = formCookie;
                    return newForumEntity;
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException($"Could not parse new thread form data. {exception}");
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Result> CreateNewThreadAsync(NewThread newThreadEntity)
        {
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
                {new StringContent(newThreadEntity.FormKey), "formkey"},
                {new StringContent(newThreadEntity.FormCookie), "form_cookie"},
                {new StringContent(newThreadEntity.PostIcon.Id.ToString(CultureInfo.InvariantCulture)), "iconid"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Subject)), "subject"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Content)), "message"},
                {new StringContent(newThreadEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("Submit Reply"), "submit"}
            };
            return await _webManager.PostFormData(EndPoints.NewThreadBase, form);
        }

        public async Task<Result> CreateNewThreadPreview(NewThread newThreadEntity)
        {
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
                {new StringContent(newThreadEntity.FormKey), "formkey"},
                {new StringContent(newThreadEntity.FormCookie), "form_cookie"},
                {new StringContent(newThreadEntity.PostIcon.Id.ToString(CultureInfo.InvariantCulture)), "iconid"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Subject)), "subject"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Content)), "message"},
                {new StringContent(newThreadEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("Submit Post"), "submit"},
                {new StringContent("Preview Post"), "preview"}
            };

            // We post to SA the same way we would for a normal reply, but instead of getting a redirect back to the
            // thread, we'll get redirected to back to the reply screen with the preview message on it.
            // From here we can parse that preview and return it to the user.
            try
            {
                var result = await _webManager.PostFormData(EndPoints.NewThreadBase, form);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);
                HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").ToArray();

                HtmlNode previewNode =
                    replyNodes.FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("inner postbody"));
                var post = new Post { PostHtml = previewNode.OuterHtml };
                result.ResultJson = JsonConvert.SerializeObject(post);
                return result;
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to get preview HTML", exception);
            }
        }



        public async Task<Result> GetForumThreadsAsync(string forumLocation, int forumId, int page, bool parseToJson = true)
        {
            string url = forumLocation + string.Format(EndPoints.PageNumber, page);
            var result = await _webManager.GetData(url);
            if (!result.IsSuccess)
            {
                ErrorHandler.CreateErrorObject(result, "Failed to get threads", string.Empty);
                return result;
            }

            if (!parseToJson)
            {
                return result;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);

            var forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));

            var forumThreadList = new List<Thread>();
            try
            {
                foreach (
    HtmlNode threadNode in
        forumNode.Descendants("tr")
            .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
                {
                    var threadEntity = new Thread { ForumId = forumId };
                    ParseThreadHtml(threadEntity, threadNode);
                    forumThreadList.Add(threadEntity);
                }
            }
            catch (Exception ex)
            {
                if (
                    doc.DocumentNode.InnerText.Contains(
                        "Sorry, you must be a registered forums member to view this page."))
                {
                    ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace, "paywall", true);
                }
                else
                {
                    ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace);
                }
                return result;
            }

            result.ResultJson = JsonConvert.SerializeObject(forumThreadList);
            return result;
        }

        public async Task<Result> MarkPostAsLastReadAs(long threadId, int index)
        {
           return await _webManager.GetData(string.Format(EndPoints.LastRead, index, threadId));
        }

        private void ParseHasSeenThread(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.HasSeen = threadNode.GetAttributeValue("class", string.Empty).Contains("seen");
        }

        private void ParseThreadTitleAnnouncement(Thread threadEntity, HtmlNode threadNode)
        {
            var titleNode = threadNode.Descendants("a")
               .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("thread_title")) ??
                           threadNode.Descendants("a")
                   .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("announcement"));

            threadEntity.IsAnnouncement = titleNode != null &&
                titleNode.GetAttributeValue("class", string.Empty).Equals("announcement");

            threadEntity.Name =
                titleNode != null ? WebUtility.HtmlDecode(titleNode.InnerText) : "BLANK TITLE?!?";
        }

        private void ParseThreadKilledBy(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.KilledBy =
                threadNode.Descendants("a")
                    .LastOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author")) != null ? threadNode.Descendants("a")
                    .LastOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author")).InnerText : string.Empty;
        }

        private void ParseThreadIsSticky(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.IsSticky =
                threadNode.Descendants("td")
                    .Any(node => node.GetAttributeValue("class", string.Empty).Contains("title_sticky"));
        }

        private void ParseThreadIsLocked(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.IsLocked = threadNode.GetAttributeValue("class", string.Empty).Contains("closed");
        }

        private void ParseThreadCanMarkAsUnread(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.CanMarkAsUnread =
                threadNode.Descendants("a").Any(node => node.GetAttributeValue("class", string.Empty).Equals("x"));
        }

        private void ParseThreadAuthor(Thread threadEntity, HtmlNode threadNode)
        {
            threadEntity.Author =
                WebUtility.HtmlDecode(threadNode.Descendants("td")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author"))
                    .InnerText);
        }

        private void ParseThreadRepliesSinceLastOpened(Thread threadEntity, HtmlNode threadNode)
        {
            if (threadNode.Descendants("a").Any(node => node.GetAttributeValue("class", string.Empty).Equals("count")))
            {
                threadEntity.RepliesSinceLastOpened =
                    Convert.ToInt32(
                        threadNode.Descendants("a")
                            .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("count"))
                            .InnerText);
            }
        }

        private void ParseThreadReplyCount(Thread threadEntity, HtmlNode threadNode)
        {
            try
            {
                threadEntity.ReplyCount =
                threadNode.Descendants("td")
                    .Any(node => node.GetAttributeValue("class", string.Empty).Contains("replies"))
                    ? Convert.ToInt32(
                        threadNode.Descendants("td")
                            .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("replies"))
                            .InnerText)
                    : 1;
            }
            catch (Exception)
            {
                threadEntity.ReplyCount = 0;
            }
        }

        private void ParseThreadViewCount(Thread threadEntity, HtmlNode threadNode)
        {
            try
            {
                threadEntity.ViewCount =
               threadNode.Descendants("td")
                   .Any(node => node.GetAttributeValue("class", string.Empty).Contains("views"))
                   ? Convert.ToInt32(
                       threadNode.Descendants("td")
                           .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("views"))
                           .InnerText)
                   : 1;
            }
            catch (Exception)
            {
                threadEntity.ViewCount = 0;
            }
        }

        private void ParseThreadRating(Thread threadEntity, HtmlNode threadNode)
        {
            var threadRatingUrl = threadNode.Descendants("td")
                .Any(node => node.GetAttributeValue("class", string.Empty).Contains("rating")) &&
                                  threadNode.Descendants("td")
                                      .FirstOrDefault(
                                          node => node.GetAttributeValue("class", string.Empty).Equals("rating"))
                                      .Descendants("img")
                                      .Any()
                ? threadNode.Descendants("td")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("rating"))
                    .Descendants("img")
                    .FirstOrDefault()
                    .GetAttributeValue("src", string.Empty) : null;

            if (!string.IsNullOrEmpty(threadRatingUrl))
            {
                threadEntity.RatingImageUrl = threadRatingUrl;
                threadEntity.RatingImage = Path.GetFileNameWithoutExtension(threadRatingUrl);
            }
        }

        private void ParseThreadTotalPages(Thread threadEntity)
        {
            threadEntity.TotalPages = (threadEntity.ReplyCount / 40) + 1;
        }

        private void ParseThreadId(Thread threadEntity, HtmlNode threadNode)
        {
            var titleNode = threadNode.Descendants("a")
              .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("thread_title")) ??
                          threadNode.Descendants("a")
                  .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("announcement"));

            if (titleNode == null) return;

            threadEntity.Location = EndPoints.BaseUrl +
                                    titleNode.GetAttributeValue("href", string.Empty) + EndPoints.PerPage;

            threadEntity.ThreadId =
                Convert.ToInt64(
                    titleNode
                        .GetAttributeValue("href", string.Empty)
                        .Split('=')[1]);
        }

        private void ParseThreadIcon(Thread threadEntity, HtmlNode threadNode)
        {
            HtmlNode first =
               threadNode.Descendants("td")
                   .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("icon"));
            if (first != null)
            {
                var testImageString = first.Descendants("img").FirstOrDefault().GetAttributeValue("src", string.Empty); ;
                if (!string.IsNullOrEmpty(testImageString))
                {
                    threadEntity.ImageIconUrl = testImageString.Replace("http:", "https:");
                    threadEntity.ImageIconLocation = Path.GetFileNameWithoutExtension(testImageString);
                }
            }
        }

        private void ParseStoreThreadIcon(Thread threadEntity, HtmlNode threadNode)
        {
            HtmlNode second =
    threadNode.Descendants("td")
        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("icon2"));
            if (second == null) return;
            try
            {
                var testImageString = second.Descendants("img").FirstOrDefault().GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrEmpty(testImageString))
                {
                    threadEntity.StoreImageIconUrl = testImageString;
                    threadEntity.StoreImageIconLocation = Path.GetFileNameWithoutExtension(testImageString);
                }
            }
            catch (Exception)
            {
                threadEntity.StoreImageIconLocation = null;
            }
        }

        private void ParseThreadHtml(Thread threadEntity, HtmlNode threadNode)
        {
            try
            {
                ParseHasSeenThread(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Has Seen' element {exception}");
            }

            try
            {
                ParseThreadTitleAnnouncement(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread/Announcement' element {exception}");
            }

            try
            {
                ParseThreadKilledBy(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Killed By' element {exception}");
            }

            try
            {
                ParseThreadIsSticky(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Is Thread Sticky' element {exception}");
            }

            try
            {
                ParseThreadIsLocked(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread Locked' element {exception}");
            }

            try
            {
                ParseThreadCanMarkAsUnread(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(
                    $"Failed to parse 'Can mark as thread as unread' element {exception}");
            }

            try
            {
                threadEntity.HasBeenViewed = threadEntity.CanMarkAsUnread;
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Has Been Viewed' element {exception}");
            }

            try
            {
                ParseThreadAuthor(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread Author' element {exception}");
            }

            try
            {
                ParseThreadRepliesSinceLastOpened(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(
                    $"Failed to parse 'Replies since last opened' element {exception}");
            }

            try
            {
                ParseThreadReplyCount(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Reply count' element {exception}");
            }

            try
            {
                ParseThreadViewCount(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'View Count' element {exception}");
            }

            try
            {
                ParseThreadRating(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread Rating' element {exception}");
            }

            try
            {
                ParseThreadTotalPages(threadEntity);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Total Pages' element {exception}");
            }

            try
            {
                ParseThreadId(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread Id' element {exception}");
            }

            try
            {
                ParseThreadIcon(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Thread Icon' element {exception}");
            }

            try
            {
                ParseStoreThreadIcon(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException($"Failed to parse 'Store thread icon' element {exception}");
            }

        }

        
    }
}
