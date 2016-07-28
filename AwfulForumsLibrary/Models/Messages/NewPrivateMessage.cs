using AwfulForumsLibrary.Models.PostIcons;

namespace AwfulForumsLibrary.Models.Messages
{
    public class NewPrivateMessage
    {
        public PostIcon Icon { get; set; }

        public string Title { get; set; }

        public string Receiver { get; set; }

        public string Body { get; set; }
    }
}
