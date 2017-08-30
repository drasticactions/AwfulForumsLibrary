using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AwfulForumsLibrary.Models.Web
{
    public class AuthResult
    {
        /// <summary>
        /// If the request we've recieved was gotten successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// If errored on authentication, will contain the error message.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The authentication cookie from logging in.
        /// </summary>
        public CookieContainer AuthenticationCookie { get; set; }
    }
}
