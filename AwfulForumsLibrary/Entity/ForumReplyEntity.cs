using System.Collections.ObjectModel;
using System.Net;

namespace AwfulForumsLibrary.Entity
{
    public class ForumReplyEntity
    {
        public string Message { get; set; }

        public ForumThreadEntity Thread { get; private set; }

        public ForumPostEntity Post { get; private set; }

        public bool ParseUrl { get; private set; }

        public string FormKey { get; private set; }

        public string FormCookie { get; private set; }

        public string Quote { get; private set; }

        public string ThreadId { get; private set; }

        public long PostId { get; private set; }

        public string PreviousPostsRaw { get; set; }

        public string Bookmark { get; set; }
        public ObservableCollection<ForumPostEntity> ForumPosts { get; set; }

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
