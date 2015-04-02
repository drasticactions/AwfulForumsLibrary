using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    public class SaclopediaNavigationTopicEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        [ForeignKey(typeof(SaclopediaNavigationEntity))]
        public int ParentNavId { get; set; }

        [ManyToOne]
        public virtual SaclopediaNavigationEntity ParentNav { get; set; }
        public string Topic { get; set; }

        public string Link { get; set; }
    }
}
