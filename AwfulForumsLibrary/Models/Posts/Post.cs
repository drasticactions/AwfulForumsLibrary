using AwfulForumsLibrary.Models.Users;

namespace AwfulForumsLibrary.Models.Posts
{
    public class Post
    {
        public User User { get; set; }

        public PostElements PostElements { get; set; }

        public string PostDate { get; set; }

        public string PostReportLink { get; set; }

        public string PostQuoteLink { get; set; }

        public string PostLink { get; set; }

        public string PostFormatted { get; set; }

        public string PostHtml { get; set; }

        public string PostMarkdown { get; set; }

        public long PostId { get; set; }

        public long PostIndex { get; set; }

        public string PostDivString { get; set; }

        public bool HasSeen { get; set; }

        public bool IsQuoting { get; set; }
    }
}
