using System.Collections.Generic;

namespace AwfulForumsLibrary.Models.PostIcons
{
    public class PostIconCategory
    {
        public PostIconCategory(string category, List<PostIcon> list)
        {
            List = list;
            Category = category;
        }

        public virtual ICollection<PostIcon> List { get; private set; }

        public string Category { get; private set; }
    }
}
