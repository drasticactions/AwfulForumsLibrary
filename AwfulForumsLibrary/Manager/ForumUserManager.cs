using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Entity;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class ForumUserManager
    {
        private readonly IWebManager _webManager;

        public ForumUserManager(IWebManager webManager)
        {
            _webManager = webManager;
        }

        public ForumUserManager()
            : this(new WebManager())
        {
        }

        public static ForumUserEntity ParseNewUserFromPost(HtmlNode postNode)
        {
            var user = new ForumUserEntity
            {
                Username =
                    WebUtility.HtmlDecode(
                        postNode.Descendants("dt")
                            .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("author"))
                            .InnerHtml)
            };

            try
            {
                user.Roles = postNode.Descendants("dt")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("author"))
                .GetAttributeValue("class", string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting roles", ex);
                // This "should" never fail, but... yeah. I'm a dumb dumb :(
            }

            user.IsMod = postNode.Descendants("dt")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("role-mod")) != null;

            user.IsAdmin = postNode.Descendants("dt")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("role-admin")) != null;

            var dateTimeNode = postNode.Descendants("dd")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("registered"));
            if (dateTimeNode != null)
            {
                try
                {
                    user.DateJoined = string.IsNullOrEmpty(dateTimeNode.InnerHtml) ? DateTime.UtcNow : DateTime.Parse(dateTimeNode.InnerHtml);
                }
                catch (Exception)
                {
                    // Parsing failed, so say they joined today.
                    // I blame SA for any parsing failures.
                    user.DateJoined = DateTime.UtcNow;
                }

            }
            HtmlNode avatarTitle =
                postNode.Descendants("dd")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("title"));
            HtmlNode avatarImage =
                postNode.Descendants("dd")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("title"))
                    .Descendants("img")
                    .FirstOrDefault();

            if (avatarTitle != null)
            {
                user.AvatarTitle = WebUtility.HtmlDecode(avatarTitle.InnerText).WithoutNewLines().Trim();
            }
            if (avatarImage != null)
            {
                user.AvatarLink = avatarImage.GetAttributeValue("src", string.Empty);
            }
            var userIdNode = postNode.DescendantsAndSelf("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("userinfo")) ??
                             postNode.DescendantsAndSelf("div")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("userinfo"));
            if (userIdNode == null) return user;

            var splitString = userIdNode
                .GetAttributeValue("class", string.Empty)
                .Split('-');

            try
            {
                if (splitString.Length >= 2)
                {
                    user.Id =
                        Convert.ToInt64(splitString[1]);
                }
            }
            catch (Exception)
            {
              
            }
            if (user.Id <= 0)
            {
                if (splitString.Length >= 3)
                {
                    user.Id =
                        Convert.ToInt64(splitString[2]);
                }
            }
            // Remove the UserInfo node after we are done with it, because
            // some forums (FYAD) use it in the body of posts. Why? Who knows!11!1
            userIdNode.Remove();
            return user;
        }

        public async Task<ForumUserEntity> GetUserFromProfilePage(long userId)
        {
            string url = Constants.BaseUrl + string.Format(Constants.UserProfile, userId);
            HtmlDocument doc = (await _webManager.GetData(url)).Document;

            /*Get the user profile HTML from the user profile page,
             once we get it, get the nodes for each section of the page and parse them.*/

            HtmlNode profileNode = doc.DocumentNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("info"));

            HtmlNode threadNode = doc.DocumentNode.Descendants("td")
                .FirstOrDefault(node => node.GetAttributeValue("id", string.Empty).Contains("thread"));
            ForumUserEntity userEntity = ParseFromUserProfile(profileNode, threadNode);

            ParseFromUserProfileAvatarInformation(userEntity, threadNode);

            return userEntity;
        }

        private void ParseFromUserProfileAvatarInformation(ForumUserEntity userEntity, HtmlNode threadNode)
        {
            HtmlNode avatarTitle =
                threadNode.Descendants("dd")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("title"));
            HtmlNode avatarImage =
                threadNode.Descendants("dd")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("title"))
                    .Descendants("img")
                    .Any()
                    ? threadNode.Descendants("dd")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("title"))
                        .Descendants("img")
                        .FirstOrDefault()
                    : null;

            if (avatarTitle != null)
            {
                userEntity.AvatarTitle = WebUtility.HtmlDecode(avatarTitle.InnerText).WithoutNewLines().Trim();
            }
            if (avatarImage != null)
            {
                userEntity.AvatarLink = avatarImage.GetAttributeValue("src", string.Empty);
            }
        }

        private ForumUserEntity ParseFromUserProfile(HtmlNode profileNode, HtmlNode threadNode)
        {
            HtmlNode additionalNode =
                profileNode.Descendants("dl")
                    .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Contains("additional"));
            Dictionary<string, string> additionalProfileAttributes = ParseAdditionalProfileAttributes(additionalNode);

            var user = new ForumUserEntity
            {
                Username =
                    threadNode.Descendants("dt")
                        .FirstOrDefault(node => node.GetAttributeValue("class", string.Empty).Equals("author"))
                        .InnerText,
                AboutUser = string.Empty,
                DateJoined = DateTime.Parse(additionalProfileAttributes["Member Since"]),
                PostCount = int.Parse(additionalProfileAttributes["Post Count"]),
                PostRate = additionalProfileAttributes["Post Rate"],
                LastPostDate = DateTime.Parse(additionalProfileAttributes["Last Post"])
            };

            foreach (HtmlNode aboutParagraph in profileNode.Descendants("p"))
            {
                user.AboutUser += WebUtility.HtmlDecode(aboutParagraph.InnerText.WithoutNewLines().Trim()) +
                                  Environment.NewLine + Environment.NewLine;
            }
            if (additionalProfileAttributes.ContainsKey("Seller Rating"))
            {
                user.SellerRating = additionalProfileAttributes["Seller Rating"];
            }
            if (additionalProfileAttributes.ContainsKey("Location"))
            {
                user.Location = additionalProfileAttributes["Location"];
            }
            return user;
        }

        private Dictionary<string, string> ParseAdditionalProfileAttributes(HtmlNode additionalNode)
        {
            IEnumerable<HtmlNode> dts = additionalNode.Descendants("dt");
            IEnumerable<HtmlNode> dds = additionalNode.Descendants("dd");
            Dictionary<string, string> result =
                dts.Zip(dds, (first, second) => new Tuple<string, string>(first.InnerText, second.InnerText))
                    .ToDictionary(k => k.Item1, v => v.Item2);
            // Clean up malformed HTML that results in the "last post" value being all screwy
            string lastPostValue = result["Last Post"];
            int removalStartIndex = lastPostValue.IndexOf('\n');
            int lengthToRemove = lastPostValue.Length - removalStartIndex;
            result["Last Post"] = lastPostValue.Remove(removalStartIndex, lengthToRemove);
            return result;
        }
    }
}
