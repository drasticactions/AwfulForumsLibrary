using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace AwfulForumsLibrary.Entity
{
    public class ForumCategoryEntity
    {
        public ForumCategoryEntity()
        {
            ForumList = new List<ForumEntity>();
        }

        public string Name { get; set; }

        public string Location { get; set; }

        [PrimaryKey]
        public int Id { get; set; }

        public int Order { get; set; }

        /// <summary>
        ///     The forums that belong to that category (Ex. GBS, FYAD)
        /// </summary>
        /// 
        [OneToMany(CascadeOperations = CascadeOperation.All)]

        [JsonIgnore]
        public List<ForumEntity> ForumList { get; set; }
    }
}
