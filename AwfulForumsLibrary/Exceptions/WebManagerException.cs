using System;

namespace AwfulForumsLibrary.Exceptions
{
    public class WebManagerException : Exception
    {
        public WebManagerException()
        {
        }

        public WebManagerException(string message)
            : base(message)
        {
        }
    }
}
