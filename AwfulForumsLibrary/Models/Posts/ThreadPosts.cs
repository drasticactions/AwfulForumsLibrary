using System.Collections.Generic;
using AwfulForumsLibrary.Models.Threads;

namespace AwfulForumsLibrary.Models.Posts
{
    public class ThreadPosts
    {
        public Thread ForumThread { get; set; }

        public List<Post> Posts { get; set; } 
    }
}
