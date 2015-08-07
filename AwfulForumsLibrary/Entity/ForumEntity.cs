using PropertyChanged;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    [ImplementPropertyChanged]
    public class ForumEntity
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public int CurrentPage { get; set; }

        public bool IsSubforum { get; set; }

        public int TotalPages { get; set; }

        public int ForumId { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        [ForeignKey(typeof(ForumCategoryEntity))]
        public int ForumCategoryEntityId { get; set; }

        [ForeignKey(typeof(ForumEntity))]
        public int ParentForumId { get; set; }

        [ManyToOne]
        public ForumEntity ParentForum { get; set; }

        [ManyToOne]
        public ForumCategoryEntity ForumCategory { get; set; }

        public bool IsBookmarks { get; set; }
    }
}
