namespace AwfulForumsLibrary.Entity
{
    public class ForumPostEntity
    {
        public ForumUserEntity User { get; set; }

        public string PostDate { get; set; }

        public string PostReportLink { get; set; }

        public string PostQuoteLink { get; set; }

        public string PostLink { get; set; }

        public string PostFormatted { get; set; }

        public string PostHtml { get; set; }

        public int PostHeight { get; set; }

        public long PostId { get; set; }

        public long PostIndex { get; set; }

        public string PostDivString { get; set; }

        public bool HasSeen { get; set; }

        public bool IsQuoting { get; set; }
    }
}
