using AwfulForumsLibrary.Models.PostIcons;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwfulForumsLibrary.Models.Threads
{
    public class NewThread
    {
        public int ForumId { get; set; }

        public string Subject { get; set; }

        public string Content { get; set; }

        public PostIcon PostIcon { get; set; }

        public string FormKey { get; set; }

        public string FormCookie { get; set; }

        public bool ParseUrl { get; set; }
    }
}
