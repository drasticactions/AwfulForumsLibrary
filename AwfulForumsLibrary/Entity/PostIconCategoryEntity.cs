using System.Collections.Generic;

namespace AwfulForumsLibrary.Entity
{
    public class PostIconCategoryEntity
    {
        public PostIconCategoryEntity(string category, List<PostIconEntity> list)
        {
            List = list;
            Category = category;
        }

        public virtual ICollection<PostIconEntity> List { get; private set; }

        public string Category { get; private set; }
    }
}
