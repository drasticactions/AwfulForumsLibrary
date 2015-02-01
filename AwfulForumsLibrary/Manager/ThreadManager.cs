using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class ThreadManager
    {
        private readonly IWebManager _webManager;

        public ThreadManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public ThreadManager() : this(new WebManager())
        {
        }

        public async Task<bool> AddBookmarkAsync(long threadId)
        {
            await _webManager.PostData(
                    Constants.Bookmark, string.Format(
                        Constants.AddBookmark, threadId
                        ));
            return true;
        }

        public async Task<bool> RemoveBookmarkAsync(long threadId)
        {
            await _webManager.PostData(
                    Constants.Bookmark, string.Format(
                        Constants.RemoveBookmark, threadId
                        ));
            return true;
        }

        private void ParseFromThread(ForumThreadEntity threadEntity, HtmlDocument threadDocument)
        {
            var title = threadDocument.DocumentNode.Descendants("title").FirstOrDefault();

            if (title != null)
            {
                threadEntity.Name = WebUtility.HtmlDecode(title.InnerText.Replace(" - The Something Awful Forums", string.Empty));
            }

            var threadIdNode = threadDocument.DocumentNode.Descendants("body").First();
            threadEntity.ThreadId = Convert.ToInt64(threadIdNode.GetAttributeValue("data-thread", string.Empty));

            threadEntity.Location = string.Format(Constants.ThreadPage, threadEntity.ThreadId);
            var pageNavigationNode = threadDocument.DocumentNode.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("pages top"));
            if (string.IsNullOrWhiteSpace(pageNavigationNode.InnerHtml))
            {
                threadEntity.TotalPages = 1;
                threadEntity.CurrentPage = 1;
            }
            else
            {
                var lastPageNode = pageNavigationNode.Descendants("a").FirstOrDefault(node => node.GetAttributeValue("title", string.Empty).Equals("Last page"));
                if (lastPageNode != null)
                {
                    string urlHref = lastPageNode.GetAttributeValue("href", string.Empty);
                    var query = Extensions.ParseQueryString(urlHref);
                    threadEntity.TotalPages = Convert.ToInt32(query["pagenumber"]);
                }

                var pageSelector = pageNavigationNode.Descendants("select").FirstOrDefault();

                var selectedPage = pageSelector.Descendants("option")
                    .FirstOrDefault(node => node.GetAttributeValue("selected", string.Empty).Equals("selected"));

                threadEntity.CurrentPage = Convert.ToInt32(selectedPage.GetAttributeValue("value", string.Empty));
            }
        }

        public async Task<HtmlDocument> GetThreadInfo(ForumThreadEntity forumThread, string url)
        {
            WebManager.Result result = await _webManager.GetData(url);
            HtmlDocument doc = result.Document;
            try
            {
                ParseFromThread(forumThread, doc);
            }
            catch (Exception exception)
            {
                throw new Exception("Error parsing thread", exception);
            }

            try
            {
                string responseUri = result.AbsoluteUri;
                string[] test = responseUri.Split('#');
                if (test.Length > 1 && test[1].Contains("pti"))
                {
                    forumThread.ScrollToPost = Int32.Parse(Regex.Match(responseUri.Split('#')[1], @"\d+").Value) - 1;
                    forumThread.ScrollToPostString = string.Concat("#", responseUri.Split('#')[1]);
                }

                var query = Extensions.ParseQueryString(url);

                if (query.ContainsKey("pagenumber"))
                {
                    forumThread.CurrentPage = Convert.ToInt32(query["pagenumber"]);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error parsing thread", exception);
            }

            return doc;
        }

        public async Task<ObservableCollection<ForumThreadEntity>> GetBookmarksAsync(ForumEntity forumCategory, int page)
        {
            var forumThreadList = new ObservableCollection<ForumThreadEntity>();
            String url = Constants.BookmarksUrl;
            if (forumCategory.CurrentPage >= 0)
            {
                url = Constants.BookmarksUrl + string.Format(Constants.PageNumber, page);
            }

            HtmlDocument doc = (await _webManager.GetData(url)).Document;

            HtmlNode forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));


            foreach (
                HtmlNode threadNode in
                    forumNode.Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
            {
                var threadEntity = new ForumThreadEntity { ForumId = forumCategory.ForumId, ForumEntity = forumCategory, IsBookmark = true};
                ParseThreadHtml(threadEntity, threadNode);
                forumThreadList.Add(threadEntity);
            }
            return forumThreadList;
        }

        public async Task<ObservableCollection<ForumThreadEntity>> GetForumThreadsAsync(ForumEntity forumCategory, int page)
        {
            string url = forumCategory.Location + string.Format(Constants.PageNumber, page);

            HtmlDocument doc = (await _webManager.GetData(url)).Document;

            HtmlNode forumNode =
                doc.DocumentNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("threadlist"));
            var forumThreadList = new ObservableCollection<ForumThreadEntity>();
            foreach (
                HtmlNode threadNode in
                    forumNode.Descendants("tr")
                        .Where(node => node.GetAttributeValue("class", string.Empty).StartsWith("thread")))
            {
                var threadEntity = new ForumThreadEntity {ForumId = forumCategory.ForumId, ForumEntity = forumCategory};
                ParseThreadHtml(threadEntity, threadNode);
                forumThreadList.Add(threadEntity);
            }

            return forumThreadList;
        }

        public async Task<NewThreadEntity> GetThreadCookiesAsync(long forumId)
        {
            try
            {
                string url = string.Format(Constants.NewThread, forumId);
                WebManager.Result result = await _webManager.GetData(url);
                HtmlDocument doc = result.Document;

                HtmlNode[] formNodes = doc.DocumentNode.Descendants("input").ToArray();

                HtmlNode formKeyNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("formkey"));

                HtmlNode formCookieNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("form_cookie"));

                var newForumEntity = new NewThreadEntity();
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
                    throw new InvalidOperationException(string.Format("Could not parse new thread form data. {0}", exception));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ParseHasSeenThread(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.HasSeen = threadNode.GetAttributeValue("class", string.Empty).Contains("seen");
        }

        private void ParseThreadTitleAnnouncement(ForumThreadEntity threadEntity, HtmlNode threadNode)
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

        private void ParseThreadKilledBy(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.KilledBy =
                threadNode.Descendants("a")
                    .LastOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author")) != null ? threadNode.Descendants("a")
                    .LastOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author")).InnerText : string.Empty;
        }

        private void ParseThreadIsSticky(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.IsSticky =
                threadNode.Descendants("td")
                    .Any(node => node.GetAttributeValue("class", string.Empty).Contains("title_sticky"));
        }

        private void ParseThreadIsLocked(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.IsLocked = threadNode.GetAttributeValue("class", string.Empty).Contains("closed");
        }

        private void ParseThreadCanMarkAsUnread(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.CanMarkAsUnread =
                threadNode.Descendants("a").Any(node => node.GetAttributeValue("class", string.Empty).Equals("x"));
        }

        private void ParseThreadAuthor(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            threadEntity.Author =
               threadNode.Descendants("td")
                   .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author"))
                   .InnerText;
        }

        private void ParseThreadRepliesSinceLastOpened(ForumThreadEntity threadEntity, HtmlNode threadNode)
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

        private void ParseThreadReplyCount(ForumThreadEntity threadEntity, HtmlNode threadNode)
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

        private void ParseThreadViewCount(ForumThreadEntity threadEntity, HtmlNode threadNode)
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

        private void ParseThreadRating(ForumThreadEntity threadEntity, HtmlNode threadNode)
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
                threadEntity.RatingImage = string.Format("/Assets/ThreadRatings/{0}.png", Path.GetFileNameWithoutExtension(threadRatingUrl));
            }
        }

        private void ParseThreadTotalPages(ForumThreadEntity threadEntity)
        {
            threadEntity.TotalPages = (threadEntity.ReplyCount / 40) + 1;
        }

        private void ParseThreadId(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            var titleNode = threadNode.Descendants("a")
              .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("thread_title")) ??
                          threadNode.Descendants("a")
                  .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("announcement"));

            if (titleNode == null) return;

            threadEntity.Location = Constants.BaseUrl +
                                    titleNode.GetAttributeValue("href", string.Empty) + Constants.PerPage;

            threadEntity.ThreadId =
                Convert.ToInt64(
                    titleNode
                        .GetAttributeValue("href", string.Empty)
                        .Split('=')[1]);
        }

        private void ParseThreadIcon(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            HtmlNode first =
               threadNode.Descendants("td")
                   .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("icon"));
            if (first != null)
            {
                var testImageString = first.Descendants("img").FirstOrDefault().GetAttributeValue("src", string.Empty); ;
                if (!string.IsNullOrEmpty(testImageString))
                {
                    threadEntity.ImageIconLocation = string.Format("/Assets/ThreadTags/{0}.png", Path.GetFileNameWithoutExtension(testImageString));
                }
            }
        }

        private void ParseStoreThreadIcon(ForumThreadEntity threadEntity, HtmlNode threadNode)
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
                    threadEntity.StoreImageIconLocation = string.Format("/Assets/ThreadTags/{0}.png", Path.GetFileNameWithoutExtension(testImageString));
                }
            }
            catch (Exception)
            {
                threadEntity.StoreImageIconLocation = null;
            }
        }

        private void ParseThreadHtml(ForumThreadEntity threadEntity, HtmlNode threadNode)
        {
            try
            {
                ParseHasSeenThread(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Has Seen' element {0}", exception));
            }

            try
            {
                ParseThreadTitleAnnouncement(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread/Announcement' element {0}", exception));
            }

            try
            {
                ParseThreadKilledBy(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Killed By' element {0}", exception));
            }

            try
            {
                ParseThreadIsSticky(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Is Thread Sticky' element {0}", exception));
            }

            try
            {
                ParseThreadIsLocked(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread Locked' element {0}", exception));
            }

            try
            {
                ParseThreadCanMarkAsUnread(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Can mark as thread as unread' element {0}", exception));
            }

            try
            {
                threadEntity.HasBeenViewed = threadEntity.CanMarkAsUnread;
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Has Been Viewed' element {0}", exception));
            }

            try
            {
                ParseThreadAuthor(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread Author' element {0}", exception));
            }

            try
            {
                ParseThreadRepliesSinceLastOpened(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Replies since last opened' element {0}", exception));
            }

            try
            {
                ParseThreadReplyCount(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Reply count' element {0}", exception));
            }

            try
            {
                ParseThreadViewCount(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'View Count' element {0}", exception));
            }

            try
            {
                ParseThreadRating(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread Rating' element {0}", exception));
            }

            try
            {
                ParseThreadTotalPages(threadEntity);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Total Pages' element {0}", exception));
            }

            try
            {
                ParseThreadId(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread Id' element {0}", exception));
            }

            try
            {
                ParseThreadIcon(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Thread Icon' element {0}", exception));
            }

            try
            {
                ParseStoreThreadIcon(threadEntity, threadNode);
            }
            catch (Exception exception)
            {
                throw new ForumListParsingFailedException(string.Format("Failed to parse 'Store thread icon' element {0}", exception));
            }

        }

        public async Task<bool> MarkThreadUnreadAsync(long threadId)
        {
            await _webManager.PostData(
                    Constants.ResetBase, string.Format(
                        Constants.ResetSeen, threadId
                        ));
            return true;
        }

        public async Task<string> CreateNewThreadPreview(NewThreadEntity newThreadEntity)
        {
            if (newThreadEntity == null)
                return string.Empty;
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.Forum.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
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
                HttpResponseMessage response = await _webManager.PostFormData(Constants.NewThreadBase, form);
                Stream stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").ToArray();

                    HtmlNode previewNode =
                        replyNodes.FirstOrDefault(node => node.GetAttributeValue("class", "").Equals("inner postbody"));
                    return previewNode == null ? string.Empty : FixPostHtml(previewNode.OuterHtml);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to get preview HTML", exception);
            }
        }

        private static string FixPostHtml(String postHtml)
        {
            // TODO: Remove Windows specific header.
            return "<!DOCTYPE html><html>" + Constants.HtmlHeader + "<body>" + postHtml + "</body></html>";
        }

        public async Task<bool> CreateNewThreadAsync(NewThreadEntity newThreadEntity)
        {
            if (newThreadEntity == null)
                return false;
            var form = new MultipartFormDataContent
            {
                {new StringContent("postthread"), "action"},
                {new StringContent(newThreadEntity.Forum.ForumId.ToString(CultureInfo.InvariantCulture)), "forumid"},
                {new StringContent(newThreadEntity.FormKey), "formkey"},
                {new StringContent(newThreadEntity.FormCookie), "form_cookie"},
                {new StringContent(newThreadEntity.PostIcon.Id.ToString(CultureInfo.InvariantCulture)), "iconid"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Subject)), "subject"},
                {new StringContent(Extensions.HtmlEncode(newThreadEntity.Content)), "message"},
                {new StringContent(newThreadEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("Submit Reply"), "submit"}
            };
            HttpResponseMessage response = await _webManager.PostFormData(Constants.NewThreadBase, form);

            return response.IsSuccessStatusCode;
        }

        public async Task MarkPostAsLastReadAs(ForumThreadEntity forumThreadEntity, int index)
        {
            await _webManager.GetData(string.Format(Constants.LastRead, index, forumThreadEntity.ThreadId));
        }
    }
}
