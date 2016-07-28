using System.Net;

namespace AwfulForumsLibrary.Models.Web
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }

        public string Error { get; set; }

        public CookieContainer AuthenticationCookie { get; set; }
    }
}
