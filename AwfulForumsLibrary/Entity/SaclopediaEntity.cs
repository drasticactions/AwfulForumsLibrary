using System;

namespace AwfulForumsLibrary.Entity
{
    public class SaclopediaEntity
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public long UserId { get; set; }

        public string PostedDate { get; set; }

        public string FormattedBody { get; set; }

        public string Body { get; set; }
    }
}
