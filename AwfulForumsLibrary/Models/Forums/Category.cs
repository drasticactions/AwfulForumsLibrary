using System.Collections.Generic;

namespace AwfulForumsLibrary.Models.Forums
{
    public class Category
    {
        public Category()
        {
            ForumList = new List<Forum>();
        }

        public string Name { get; set; }

        public string Location { get; set; }

        public int Id { get; set; }

        public int Order { get; set; }

        /// <summary>
        ///     The forums that belong to that category (Ex. GBS, FYAD)
        /// </summary>
        public List<Forum> ForumList { get; set; }
    }
}
