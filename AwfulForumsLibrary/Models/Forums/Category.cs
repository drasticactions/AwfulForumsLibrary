using System;
using System.Collections.Generic;
using System.Text;

namespace AwfulForumsLibrary.Models.Forums
{
    public class Category
    {
        public string Name { get; set; }

        public string Location { get; set; }

        public int Id { get; set; }

        public int Order { get; set; }

        public List<Forum> ForumList { get; set; }
    }
}
