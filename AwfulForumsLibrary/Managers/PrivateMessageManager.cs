using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using AwfulForumsLibrary.Models.Web;
using AwfulForumsLibrary.Tools;
using AwfulForumsLibrary.Models.Posts;
using AwfulForumsLibrary.Models.Messages;

namespace AwfulForumsLibrary.Managers
{
    public class PrivateMessageManager
    {
        private readonly WebManager _webManager;

        public PrivateMessageManager(WebManager webManager)
        {
            _webManager = webManager;
        }

        public async Task<Result> SendPrivateMessageAsync(NewPrivateMessage newPrivateMessageEntity)
        {
            Result result = new Result();
            MultipartFormDataContent form;
            try
            {
                form = new MultipartFormDataContent
            {
                {new StringContent("dosend"), "action"},
                {new StringContent(newPrivateMessageEntity.Receiver), "touser"},
                {new StringContent(Extensions.HtmlEncode(newPrivateMessageEntity.Title)), "title"},
                {new StringContent(Extensions.HtmlEncode(newPrivateMessageEntity.Body)), "message"},
                {new StringContent("yes"), "parseurl"},
                {new StringContent("yes"), "parseurl"},
                {new StringContent("Send Message"), "submit"}
            };
				if (newPrivateMessageEntity.Icon != null)
				{
					form.Add(new StringContent(newPrivateMessageEntity.Icon.Id.ToString(CultureInfo.InvariantCulture)), "iconid");
				}
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace);
                return result;
            }
            try
            {

                result = await _webManager.PostFormData(EndPoints.NewPrivateMessageBase, form);
                return result;
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace);
                return result;
            }
        }

        public async Task<Result> GetPrivateMessagesAsync(int page)
        {
            var privateMessageEntities = new List<PrivateMessage>();
            var url = EndPoints.PrivateMessages;
            if (page > 0)
            {
                url = EndPoints.PrivateMessages + string.Format(EndPoints.PageNumber, page);
            }

            var result = await _webManager.GetData(url);
            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);

                HtmlNode forumNode =
                    doc.DocumentNode.Descendants("tbody").FirstOrDefault();


                foreach (
                    HtmlNode threadNode in
                        forumNode.Descendants("tr"))
                {
                    var threadEntity = new PrivateMessage();
                    try
                    {
                        Parse(threadEntity, threadNode);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to parse private message list", ex);
                    }
                    privateMessageEntities.Add(threadEntity);
                }
                result.ResultJson = JsonConvert.SerializeObject(privateMessageEntities);
                return result;
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace);
                return result;
            }
        }

        public async Task<Result> GetPrivateMessageAsync(string url)
        {
            Result result = new Result();
            try
            {
                result = await _webManager.GetData(url);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(result.ResultHtml);

                HtmlNode[] replyNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("id", "").Equals("thread")).ToArray();

                HtmlNode threadNode = replyNodes.FirstOrDefault(node => node.GetAttributeValue("id", "").Equals("thread"));

                var postManager = new PostManager(_webManager);
                IEnumerable<HtmlNode> postNodes =
                    threadNode.Descendants("table")
                        .Where(node => node.GetAttributeValue("class", string.Empty).Contains("post"));
                List<Post> postList = new List<Post>();
                foreach (
         HtmlNode postNode in
             postNodes)
                {
                    var post = new Post();
                    postManager.ParsePost(post, postNode);
                    postList.Add(post);
                }

                result.ResultJson = JsonConvert.SerializeObject(postList.First());
                return result;
            }
            catch (Exception ex)
            {
                ErrorHandler.CreateErrorObject(result, ex.Message, ex.StackTrace);
                return result;
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

        public void Parse(PrivateMessage pmEntity, HtmlNode rowNode)
        {
            pmEntity.Status =
                rowNode.Descendants("td")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("status"))
                    .Descendants("img")
                    .FirstOrDefault()
                    .GetAttributeValue("src", string.Empty);

            var icon = rowNode.Descendants("td")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("icon"))
                    .Descendants("img")
                    .FirstOrDefault();

            if (icon != null)
            {
                pmEntity.Icon = icon.GetAttributeValue("src", string.Empty);
                pmEntity.ImageIconLocation = Path.GetFileNameWithoutExtension(icon.GetAttributeValue("src", string.Empty));
            }

            var titleNode = rowNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("title"));

            pmEntity.Title =
               titleNode
                    .InnerText.Replace("\n", string.Empty).Trim();

            string titleHref = titleNode.Descendants("a").FirstOrDefault().GetAttributeValue("href", string.Empty).Replace("&amp;", "&");

            pmEntity.MessageUrl = EndPoints.BaseUrl + titleHref;

            pmEntity.Sender = rowNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("sender"))
                .InnerText;
            pmEntity.Date = rowNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("date"))
                .InnerText;
        }
    }
}
