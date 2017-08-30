using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using AwfulForumsLibrary.Tools;
using AwfulForumsLibrary.Models.PostIcons;

namespace AwfulForumsLibrary.Managers
{
    public class PostIconManager
    {
        private readonly WebManager _webManager;

        public PostIconManager(WebManager webManager)
        {
            _webManager = webManager;
        }

        private void Parse(PostIcon postIconEntity, HtmlNode node)
        {
            try
            {
                postIconEntity.Id = Convert.ToInt32(node.Descendants("input").First().GetAttributeValue("value", string.Empty));
                var imageUrl = node.Descendants("img").First().GetAttributeValue("src", string.Empty);
                postIconEntity.ImageUrl = imageUrl;
                postIconEntity.Title = node.Descendants("img").First().GetAttributeValue("alt", string.Empty);
            }
            catch (Exception)
            {
                // If, for some reason, it fails to get an icon, ignore the error.
                // The list view won't show it.
            }
        }

        public async Task<IEnumerable<PostIconCategory>> GetPostIcons(int forumId)
        {
            string url = string.Format(EndPoints.NewThread, forumId);
            var result = await _webManager.GetData(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIcon>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIcon();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            var postIconCategoryEntity = new PostIconCategory("Post Icon", postIconEntityList);
            var postIconCategoryList = new List<PostIconCategory> { postIconCategoryEntity };
            return postIconCategoryList;
        }

        public async Task<IEnumerable<PostIcon>> GetPostIconList(int forumId)
        {
            string url = string.Format(EndPoints.NewThread, forumId);
            var result = await _webManager.GetData(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIcon>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIcon();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            return postIconEntityList;
        }

        public async Task<IEnumerable<PostIconCategory>> GetPmPostIcons()
        {
            string url = EndPoints.NewPrivateMessage;
            var result = await _webManager.GetData(url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(result.ResultHtml);
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIcon>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIcon();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            var postIconCategoryEntity = new PostIconCategory("Post Icon", postIconEntityList);
            var postIconCategoryList = new List<PostIconCategory> { postIconCategoryEntity };
            return postIconCategoryList;
        }
    }
}
