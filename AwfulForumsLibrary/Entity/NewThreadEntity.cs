namespace AwfulForumsLibrary.Entity
{
    public class NewThreadEntity
    {
        public ForumEntity Forum { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }

        public PostIconEntity PostIcon { get; set; }

        public string FormKey { get; set; }

        public string FormCookie { get; set; }

        public bool ParseUrl { get; set; }
    }
}
