using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AwfulForumsLibrary.Models.Users
{
    public class UserAuth
    {
        public int UserAuthId { get; set; }
        public string UserName { get; set; }

        public string AvatarLink { get; set; }

        public string CookiePath { get; set; }

        public CookieContainer AuthCookies { get; set; }

        public bool IsDefaultUser { get; set; }
    }
}
