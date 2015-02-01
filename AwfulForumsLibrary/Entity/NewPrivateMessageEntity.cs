namespace AwfulForumsLibrary.Entity
{
    public class NewPrivateMessageEntity
    {
        public PostIconEntity Icon { get; set; }

        public string Title { get; set; }

        public string Receiver { get; set; }

        public string Body { get; set; }
    }
}
