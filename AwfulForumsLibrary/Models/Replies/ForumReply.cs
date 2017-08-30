using AwfulForumsLibrary.Models.Posts;
using AwfulForumsLibrary.Models.Threads;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AwfulForumsLibrary.Models.Replies
{
    public class ForumReply
    {
        public string Message { get; set; }

        public Thread Thread { get; private set; }

        public Post Post { get; private set; }

        public bool ParseUrl { get; private set; }

        public string FormKey { get; private set; }

        public string FormCookie { get; private set; }

        public string Quote { get; private set; }

        public string ThreadId { get; private set; }

        public long PostId { get; private set; }

        public string PreviousPostsRaw { get; set; }

        public string Bookmark { get; set; }
        public List<Post> ForumPosts { get; set; }

        public void MapMessage(string message)
        {
            Message = message;
            ParseUrl = true;
        }

        public void MapThreadInformation(string formKey, string formCookie, string quote, string threadId)
        {
            FormKey = formKey;
            FormCookie = formCookie;
            ThreadId = threadId;
            Quote = WebUtility.HtmlDecode(quote);
        }

        public void MapEditPostInformation(string quote, long postId, string bookmark)
        {
            Quote = WebUtility.HtmlDecode(quote);
            PostId = postId;
            Bookmark = bookmark;
        }
    }
}
