using System.Collections.Generic;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    public class SaclopediaNavigationEntity
    {
        public string Letter { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        public string Link { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<SaclopediaNavigationTopicEntity> TopicList { get; set; }
    }
}
