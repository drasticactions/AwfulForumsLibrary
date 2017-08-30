using AwfulForumsLibrary.Models.Threads;
using System;
using System.Collections.Generic;
using System.Text;

namespace AwfulForumsLibrary.Models.Posts
{
    public class ThreadPosts
    {
        public Thread ForumThread { get; set; }

        public List<Post> Posts { get; set; }
    }
}
