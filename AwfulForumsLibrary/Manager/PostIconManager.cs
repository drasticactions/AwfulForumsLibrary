using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class PostIconManager
    {
        private readonly IWebManager _webManager;

        public PostIconManager()
            : this(new WebManager())
        {
        }

        public PostIconManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        private void Parse(PostIconEntity postIconEntity, HtmlNode node)
        {
            try
            {
                postIconEntity.Id = Convert.ToInt32(node.Descendants("input").First().GetAttributeValue("value", string.Empty));
                var imageUrl = node.Descendants("img").First().GetAttributeValue("src", string.Empty);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    postIconEntity.ImageUrl = string.Format("/Assets/ThreadTags/{0}.png", Path.GetFileNameWithoutExtension(imageUrl));
                }
                postIconEntity.Title = node.Descendants("img").First().GetAttributeValue("alt", string.Empty);
            }
            catch (Exception)
            {
                // If, for some reason, it fails to get an icon, ignore the error.
                // The list view won't show it.
            }
        }

        public async Task<IEnumerable<PostIconCategoryEntity>> GetPostIcons(ForumEntity forum)
        {
            string url = string.Format(Constants.NewThread, forum.ForumId);
            WebManager.Result result = await _webManager.GetData(url);
            HtmlDocument doc = result.Document;
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIconEntity>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIconEntity();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            var postIconCategoryEntity = new PostIconCategoryEntity("Post Icon", postIconEntityList);
            var postIconCategoryList = new List<PostIconCategoryEntity> { postIconCategoryEntity };
            return postIconCategoryList;
        }

        public async Task<IEnumerable<PostIconEntity>> GetPostIconList(ForumEntity forum)
        {
            string url = string.Format(Constants.NewThread, forum.ForumId);
            WebManager.Result result = await _webManager.GetData(url);
            HtmlDocument doc = result.Document;
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIconEntity>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIconEntity();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            return postIconEntityList;
        }

        public async Task<IEnumerable<PostIconCategoryEntity>> GetPmPostIcons()
        {
            string url = Constants.NewPrivateMessage;
            WebManager.Result result = await _webManager.GetData(url);
            HtmlDocument doc = result.Document;
            HtmlNode[] pageNodes = doc.DocumentNode.Descendants("div").Where(node => node.GetAttributeValue("class", string.Empty).Equals("posticon")).ToArray();
            var postIconEntityList = new List<PostIconEntity>();
            foreach (var pageNode in pageNodes)
            {
                var postIconEntity = new PostIconEntity();
                Parse(postIconEntity, pageNode);
                postIconEntityList.Add(postIconEntity);
            }
            var postIconCategoryEntity = new PostIconCategoryEntity("Post Icon", postIconEntityList);
            var postIconCategoryList = new List<PostIconCategoryEntity> { postIconCategoryEntity };
            return postIconCategoryList;
        }
    }
}
