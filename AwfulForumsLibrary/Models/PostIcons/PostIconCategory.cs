using System;
using System.Collections.Generic;
using System.Text;

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
