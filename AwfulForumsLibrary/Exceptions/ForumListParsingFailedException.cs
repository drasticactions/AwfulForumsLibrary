using System;

namespace AwfulForumsLibrary.Exceptions
{
    public class ForumListParsingFailedException : Exception
    {
        public ForumListParsingFailedException()
        {
        }

        public ForumListParsingFailedException(string message)
            : base(message)
        {
        }
    }
}
