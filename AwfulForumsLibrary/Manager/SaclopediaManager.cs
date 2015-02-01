using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class SaclopediaManager
    {
        private readonly IWebManager _webManager;

        public SaclopediaManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public SaclopediaManager()
            : this(new WebManager())
        {
        }

        public async Task<List<SaclopediaNavigationEntity>>  GetSaclopediaNavigationBar()
        {
            try
            {
                var result = await _webManager.GetData(Constants.SAclopediaBase);
                HtmlNode forumNode =
                    result.Document.DocumentNode.Descendants("ul")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("letternav"));
                IEnumerable<HtmlNode> navNodes = forumNode.Descendants("li");
                return navNodes.Select(CreateNavigationEntity).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get navigation links", ex);
            }
        }

        public async Task<List<SaclopediaNavigationTopicEntity>> GetSaclopediaTopics(string url)
        {
            try
            {
                var result = await _webManager.GetData(url);
                HtmlNode forumNode =
                    result.Document.DocumentNode.Descendants("ul")
                        .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Equals("topiclist"));
                IEnumerable<HtmlNode> navNodes = forumNode.Descendants("li");
                return navNodes.Select(CreateNavigationTopicEntity).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get navigation links", ex);
            }
        }

        public async Task<SaclopediaEntity> GetSaclopediaEntity(string url)
        {
            try
            {
                var saclopediaEntity = new SaclopediaEntity();
                var result = await _webManager.GetData(url);
                HtmlNode forumNode =
                    result.Document.DocumentNode.Descendants("h1")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("topic"));
                saclopediaEntity.Title = forumNode.InnerHtml;
                forumNode = result.Document.DocumentNode.Descendants("ul")
                        .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Equals("posts"));
                saclopediaEntity.Body = forumNode.OuterHtml;
                return saclopediaEntity;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse entry", ex);
            }
        }

        private SaclopediaNavigationTopicEntity CreateNavigationTopicEntity(HtmlNode navNode)
        {
            var navEntity = new SaclopediaNavigationTopicEntity();
            var resultNode =
                navNode.Descendants("a").First();
            navEntity.Topic = WebUtility.HtmlDecode(resultNode.InnerText);
            navEntity.Link = WebUtility.HtmlDecode(resultNode.GetAttributeValue("href", string.Empty));
            return navEntity;
        }


        private SaclopediaNavigationEntity CreateNavigationEntity(HtmlNode navNode)
        {
            var navEntity = new SaclopediaNavigationEntity();
            var resultNode =
                navNode.Descendants("a").First();
            navEntity.Letter = WebUtility.HtmlDecode(resultNode.InnerText);
            navEntity.Link = WebUtility.HtmlDecode(resultNode.GetAttributeValue("href", string.Empty));
            return navEntity;
        }


    }
}
