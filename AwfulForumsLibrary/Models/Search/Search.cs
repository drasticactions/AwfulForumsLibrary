using System.Collections.Generic;

namespace AwfulForumsLibrary.Models.Search
{
    public class SearchEntity
    {
        public string ResultNumber { get; set; }

        public string ThreadTitle { get; set; }

        public string ThreadLink { get; set; }

        public string Username { get; set; }

        public string ForumName { get; set; }

        public string Blurb { get; set; }
    }

    public class SearchEntityObject
    {
        public List<SearchEntity> SearchEntities { get; set; }

        public string LinkUrl { get; set; }
    }
}
