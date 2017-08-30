using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using AwfulForumsLibrary.Models.Replies;
using AwfulForumsLibrary.Tools;
using AwfulForumsLibrary.Models.Posts;
using AwfulForumsLibrary.Models.Web;

namespace AwfulForumsLibrary.Managers
{
    public class ReplyManager
    {
        private readonly WebManager _webManager;

        public ReplyManager(WebManager webManager)
        {
            _webManager = webManager;
        }

        public async Task<ForumReply> GetReplyCookiesForEdit(long postId)
        {
            try
            {
                string url = string.Format(EndPoints.EditBase, postId);
                var result = await _webManager.GetData(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);

                HtmlNode[] formNodes = doc.DocumentNode.Descendants("input").ToArray();

                HtmlNode bookmarkNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("bookmark"));

                HtmlNode[] textAreaNodes = doc.DocumentNode.Descendants("textarea").ToArray();

                HtmlNode textNode =
                    textAreaNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("message"));

                var threadManager = new ThreadManager(_webManager);

                //Get previous posts from quote page.
                string url2 = string.Format(EndPoints.QuoteBase, postId);
                var result2 = await _webManager.GetData(url2);
                HtmlDocument doc2 = new HtmlDocument();
                doc2.LoadHtml(result2.ResultHtml);

                var forumThreadPosts = new List<Post>();

                HtmlNode threadNode =
                   doc2.DocumentNode.Descendants("div")
                       .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));


                var postManager = new PostManager(_webManager);
                foreach (
                    HtmlNode postNode in
                        threadNode.Descendants("table")
                            .Where(node => node.GetAttributeValue("class", string.Empty).Contains("post")))
                {
                    var post = new Post();
                    postManager.ParsePost(post, postNode);
                    forumThreadPosts.Add(post);
                }

                var forumReplyEntity = new ForumReply();
                try
                {
                    string quote = WebUtility.HtmlDecode(textNode.InnerText);
                    forumReplyEntity.ForumPosts = forumThreadPosts;
                    string bookmark = bookmarkNode.OuterHtml.Contains("checked") ? "yes" : "no";
                    forumReplyEntity.MapEditPostInformation(quote, postId, bookmark);
                    return forumReplyEntity;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ForumReply> GetReplyCookies(long threadId = 0, long postId = 0)
        {
            if (threadId == 0 && postId == 0) return null;
            try
            {
                string url;
                url = threadId > 0 ? string.Format(EndPoints.ReplyBase, threadId) : string.Format(EndPoints.QuoteBase, postId);
                var result = await _webManager.GetData(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);

                HtmlNode[] formNodes = doc.DocumentNode.Descendants("input").ToArray();

                HtmlNode formKeyNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("formkey"));

                HtmlNode formCookieNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("form_cookie"));

                HtmlNode bookmarkNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("bookmark"));

                HtmlNode[] textAreaNodes = doc.DocumentNode.Descendants("textarea").ToArray();

                HtmlNode textNode =
                    textAreaNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("message"));

                HtmlNode threadIdNode =
                    formNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("threadid"));

                var forumThreadPosts = new List<Post>();

                HtmlNode threadNode =
                   doc.DocumentNode.Descendants("div")
                       .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));


                var postManager = new PostManager(_webManager);
                foreach (
                    HtmlNode postNode in
                        threadNode.Descendants("table")
                            .Where(node => node.GetAttributeValue("class", string.Empty).Contains("post")))
                {
                    var post = new Post();
                    postManager.ParsePost(post, postNode);
                    forumThreadPosts.Add(post);
                }

                var forumReplyEntity = new ForumReply();
                try
                {
                    string formKey = formKeyNode.GetAttributeValue("value", "");
                    string formCookie = formCookieNode.GetAttributeValue("value", "");
                    string quote = WebUtility.HtmlDecode(textNode.InnerText);
                    string threadIdTest = threadIdNode.GetAttributeValue("value", "");
                    forumReplyEntity.MapThreadInformation(formKey, formCookie, quote, threadIdTest);
                    forumReplyEntity.ForumPosts = forumThreadPosts;
                    return forumReplyEntity;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Result> SendPost(ForumReply forumReplyEntity)
        {
            var result = new Result();
            try
            {
                var form = new MultipartFormDataContent
            {
                {new StringContent("postreply"), "action"},
                {new StringContent(forumReplyEntity.ThreadId), "threadid"},
                {new StringContent(forumReplyEntity.FormKey), "formkey"},
                {new StringContent(forumReplyEntity.FormCookie), "form_cookie"},
                {new StringContent(HtmlEncode(forumReplyEntity.Message)), "message"},
                {new StringContent(forumReplyEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("2097152"), "MAX_FILE_SIZE"},
                {new StringContent("Submit Reply"), "submit"}
            };
                result = await _webManager.PostFormData(EndPoints.NewReply, form);

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public async Task<Result> SendUpdatePost(ForumReply forumReplyEntity)
        {
            var result = new Result();
            try
            {
                var form = new MultipartFormDataContent
            {
                {new StringContent("updatepost"), "action"},
                {new StringContent(forumReplyEntity.PostId.ToString()), "postid"},
                {new StringContent(HtmlEncode(forumReplyEntity.Message)), "message"},
                {new StringContent(forumReplyEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent(forumReplyEntity.Bookmark), "bookmark"},
                {new StringContent("2097152"), "MAX_FILE_SIZE"},
                {new StringContent("Save Changes"), "submit"}
            };
                result = await _webManager.PostFormData(EndPoints.EditPost, form);
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public async Task<Result> CreatePreviewEditPost(ForumReply forumReplyEntity)
        {
            var result = new Result();
            try
            {
                var form = new MultipartFormDataContent
            {
                {new StringContent("updatepost"), "action"},
                {new StringContent(forumReplyEntity.PostId.ToString()), "postid"},
                {new StringContent(HtmlEncode(forumReplyEntity.Message)), "message"},
                {new StringContent(forumReplyEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("2097152"), "MAX_FILE_SIZE"},
                {new StringContent("Preview Post"), "preview"}
            };
                result = await _webManager.PostFormData(EndPoints.EditPost, form);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);
                HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").ToArray();

                HtmlNode previewNode =
                    replyNodes.First(node => node.GetAttributeValue("class", "").Equals("inner postbody"));
                var post = new Post { PostHtml = previewNode.OuterHtml };
                result.ResultJson = JsonConvert.SerializeObject(post);
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public async Task<Result> CreatePreviewPost(ForumReply forumReplyEntity)
        {
            var result = new Result();
            try
            {
                var form = new MultipartFormDataContent
            {
                {new StringContent("postreply"), "action"},
                {new StringContent(forumReplyEntity.ThreadId), "threadid"},
                {new StringContent(forumReplyEntity.FormKey), "formkey"},
                {new StringContent(forumReplyEntity.FormCookie), "form_cookie"},
                {new StringContent(HtmlEncode(forumReplyEntity.Message)), "message"},
                {new StringContent(forumReplyEntity.ParseUrl.ToString()), "parseurl"},
                {new StringContent("2097152"), "MAX_FILE_SIZE"},
                {new StringContent("Submit Reply"), "submit"},
                {new StringContent("Preview Reply"), "preview"}
            };
                // We post to SA the same way we would for a normal reply, but instead of getting a redirect back to the
                // thread, we'll get redirected to back to the reply screen with the preview message on it.
                // From here we can parse that preview and return it to the user.

                result = await _webManager.PostFormData(EndPoints.NewReply, form);
                var doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);
                HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").ToArray();

                HtmlNode previewNode =
                    replyNodes.First(node => node.GetAttributeValue("class", "").Equals("inner postbody"));
                var post = new Post { PostHtml = previewNode.OuterHtml };
                result.ResultJson = JsonConvert.SerializeObject(post);
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public static string HtmlEncode(string text)
        {
            // In order to get Unicode characters fully working, we need to first encode the entire post.
            // THEN we decode the bits we can safely pass in, like single/double quotes.
            // If we don't, the post format will be screwed up.
            char[] chars = WebUtility.HtmlEncode(text).ToCharArray();
            var result = new StringBuilder(text.Length + (int)(text.Length * 0.1));

            foreach (char c in chars)
            {
                int value = Convert.ToInt32(c);
                if (value > 127)
                    result.AppendFormat("&#{0};", value);
                else
                    result.Append(c);
            }

            result.Replace("&quot;", "\"");
            result.Replace("&#39;", @"'");
            result.Replace("&lt;", @"<");
            result.Replace("&gt;", @">");
            return result.ToString();
        }

        public async Task<string> GetQuoteString(long postId)
        {
            string url = string.Format(EndPoints.QuoteBase, postId);
            var result = await _webManager.GetData(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);

            HtmlNode[] textAreaNodes = doc.DocumentNode.Descendants("textarea").ToArray();

            HtmlNode textNode =
                textAreaNodes.FirstOrDefault(node => node.GetAttributeValue("name", "").Equals("message"));

            try
            {
                // TODO: Figure out why in the hell we have to decode the HTML twice for Unicode to render properly.
                return WebUtility.HtmlDecode(WebUtility.HtmlDecode(textNode.InnerText));
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse newReply form data.");
            }
        }
    }
}
