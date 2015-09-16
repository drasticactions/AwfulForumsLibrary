using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class PostManager
    {
        private readonly IWebManager _webManager;

        public PostManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public PostManager()
            : this(new WebManager())
        {
        }

        public async Task<ForumPostEntity> GetPost(int postId)
        {
            try
            {
                string url = string.Format(Constants.ShowPost, postId);
                WebManager.Result result = await _webManager.GetData(url);
                HtmlDocument doc = result.Document;
                HtmlNode threadNode =
                    doc.DocumentNode.Descendants("div")
                        .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));
                HtmlNode postNode =
                    threadNode.Descendants("table")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("post"));
                var post = new ForumPostEntity();
                ParsePost(post, postNode);
                return post;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting post", ex );
            }
        }

        public async Task<List<ForumPostEntity>> GetSimpleThreadPostsAsync(ForumThreadEntity forumThread)
        {
            string url = forumThread.Location;

            if (forumThread.CurrentPage > 0)
            {
                url = forumThread.Location + string.Format(Constants.PageNumber, forumThread.CurrentPage);
            }
            else if (forumThread.HasBeenViewed)
            {
                url = forumThread.Location + Constants.GotoNewPost;
            }

            var forumThreadPosts = new List<ForumPostEntity>();

            var threadManager = new ThreadManager();
            var doc = await threadManager.GetThreadInfo(forumThread, url);

            try
            {

                try
                {
                    HtmlNode pollNode =
  doc.DocumentNode.Descendants("form")
      .FirstOrDefault(node => node.GetAttributeValue("action", string.Empty).Equals("poll.php"));

                    if (pollNode != null)
                    {
                        forumThread.Poll = ParsePoll(doc);
                    }

                }
                catch (Exception)
                {

                    // Failed to get poll. Ignore and continue...
                }

                HtmlNode threadNode =
                   doc.DocumentNode.Descendants("div")
                       .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));

                foreach (
                   HtmlNode postNode in
                       threadNode.Descendants("table")
                           .Where(node => node.GetAttributeValue("class", string.Empty).Contains("post")))
                {
                    var post = new ForumPostEntity();
                    ParsePost(post, postNode);
                    forumThreadPosts.Add(post);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse thread posts {ex.Message}");
            }

            return forumThreadPosts;
        }

        public async Task<List<ForumPostEntity>> GetThreadPostsAsync(ForumThreadEntity forumThread)
        {
            string url = forumThread.Location;

            if (forumThread.CurrentPage > 0)
            {
                url = forumThread.Location + string.Format(Constants.PageNumber, forumThread.CurrentPage);
            }
            else if (forumThread.HasBeenViewed)
            {
                url = forumThread.Location + Constants.GotoNewPost;
            }

            var forumThreadPosts = new List<ForumPostEntity>();

            var threadManager = new ThreadManager();
            var doc = await threadManager.GetThreadInfo(forumThread, url);

            try
            {

                try
                {
                    HtmlNode pollNode =
  doc.DocumentNode.Descendants("form")
      .FirstOrDefault(node => node.GetAttributeValue("action", string.Empty).Equals("poll.php"));

                    if (pollNode != null)
                    {
                        forumThread.Poll = ParsePoll(doc);
                    }

                }
                catch (Exception)
                {
                    
                    // Failed to get poll. Ignore and continue...
                }

                HtmlNode threadNode =
                   doc.DocumentNode.Descendants("div")
                       .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));

                foreach (
                   HtmlNode postNode in
                       threadNode.Descendants("table")
                           .Where(node => node.GetAttributeValue("class", string.Empty).Contains("post")))
                {
                    var post = new ForumPostEntity();
                    ParsePost(post, postNode);
                    forumThreadPosts.Add(post);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse thread posts {ex.Message}");
            }

            return forumThreadPosts;
        }

        public PollGroupEntity ParsePoll(HtmlDocument pollNode)
        {
            var pollid =
               Convert.ToInt32(pollNode.DocumentNode.Descendants("input")
                    .First(node => node.GetAttributeValue("name", string.Empty).Equals("pollid")).GetAttributeValue("value", string.Empty));

            var tableRows = pollNode.DocumentNode.Descendants("table").First(node => node.GetAttributeValue("id", string.Empty).Equals("main_full")).Descendants("tr").ToArray();
            var pollEntities = new List<PollEntity>(tableRows.Length - 2);
            for (var i = 1; i <= tableRows.Length - 1; i++)
            {
                pollEntities.Add(new PollEntity
                {
                    Id = i,
                    Title = tableRows[i].InnerText.WithoutNewLines()
                });
            }

            return new PollGroupEntity()
            {
                Title = WebUtility.HtmlDecode(tableRows[0].InnerText),
                Id = pollid,
                PollList = pollEntities
            };
        }

        public void ParsePost(ForumPostEntity post, HtmlNode postNode, bool isSimple = false)
        {
            post.User = ForumUserManager.ParseNewUserFromPost(postNode);

            HtmlNode postDateNode =
                postNode.Descendants()
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("postdate"));
            string postDateString = postDateNode == null ? string.Empty : postDateNode.InnerText;
            if (postDateString != null)
            {
                post.PostDate = postDateString.WithoutNewLines().Trim();
            }

            post.PostIndex = ParseInt(postNode.GetAttributeValue("data-idx", string.Empty));

            var postId = postNode.GetAttributeValue("id", string.Empty);
            if (!string.IsNullOrEmpty(postId) && postId.Contains("#"))
            {
                post.PostId =
                    Int64.Parse(postNode.GetAttributeValue("id", string.Empty)
                        .Replace("post", string.Empty)
                        .Replace("#", string.Empty));
            }
            else if (!string.IsNullOrEmpty(postId) && postId.Contains("post"))
            {
                var testString = postNode.GetAttributeValue("id", string.Empty)
                    .Replace("post", string.Empty);
                post.PostId = !string.IsNullOrEmpty(testString) ? Int64.Parse(testString) : 0;
            }
            else
            {
                post.PostId = 0;
            }

            var postBodyNode = postNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("postbody"));
            if (!isSimple)
            {
                this.FixQuotes(postBodyNode);
                post.PostHtml = postBodyNode.InnerHtml;
            }
            else
            {
                var postElement = new PostElementsEntity {InnerText = Regex.Replace(postBodyNode.InnerText, "<!--.*?-->", string.Empty, RegexOptions.Multiline), ImageUrls = new List<string>()};
                var images = postBodyNode.Descendants("img").Where(node => node.GetAttributeValue("class", string.Empty) != "av");
                foreach (var image in images)
                {
                    var src = image.Attributes["src"].Value;
                    if (src.Contains("somethingawful.com"))
                        continue;
                    if (src.Contains("emoticons"))
                        continue;
                    if (src.Contains("smilies"))
                        continue;
                    postElement.ImageUrls.Add(image.Attributes["src"].Value);
                }
                post.PostElements = postElement;
            }

            HtmlNode profileLinksNode =
                    postNode.Descendants("td")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("postlinks"));
            HtmlNode postRow =
                postNode.Descendants("tr").FirstOrDefault();

            if (postRow != null)
            {
                post.HasSeen = postRow.GetAttributeValue("class", string.Empty).Contains("seen");
            }

            post.User.IsCurrentUserPost =
                profileLinksNode.Descendants("img")
                    .FirstOrDefault(node => node.GetAttributeValue("alt", string.Empty).Equals("Edit")) != null;
        }

        private void FixQuotes(HtmlNode postNode)
        {
            var quoteList =
                postNode.Descendants("a")
                    .Where(node => node.GetAttributeValue("class", string.Empty).Contains("quote_link"));
            foreach (var quote in quoteList)
            {
                int postId = ParseInt(quote.GetAttributeValue("href", string.Empty));
                quote.Attributes.Remove("href");
                quote.Attributes.Append("href", "javascript:void(0)");
                string postIdFormat = string.Concat("'#postId", postId, "'");
                quote.Attributes.Add("onclick", string.Format("window.ForumCommand('scrollToPost', '{0}')", postId));
            }
        }

        private int ParseInt(string postClass)
        {
            string re1 = ".*?"; // Non-greedy match on filler
            string re2 = "(\\d+)"; // Integer Number 1

            var r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match m = r.Match(postClass);
            if (!m.Success) return 0;
            String int1 = m.Groups[1].ToString();
            return Convert.ToInt32(int1);
        }

    }
}
