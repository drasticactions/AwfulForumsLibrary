using Newtonsoft.Json;
using SQLite.Net.Attributes;

namespace AwfulForumsLibrary.Models.Forums
{
    public class Forum
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int CurrentPage { get; set; }

        public bool IsSubforum { get; set; }

        public int TotalPages { get; set; }

        public int ForumId { get; set; }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ForumCategoryEntityId { get; set; }

        public int ParentForumId { get; set; }

        [JsonIgnore]
        public Forum ParentForum { get; set; }

        [JsonIgnore]
        public Category ForumCategory { get; set; }
    }
}
