using System;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;

namespace AwfulForumsLibrary.Manager
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly ILocalStorageManager _localStorageManager;
        private readonly IWebManager _webManager;

        public AuthenticationManager(IWebManager webManager, ILocalStorageManager localStorageManager)
        {
            _webManager = webManager;
            _localStorageManager = localStorageManager;
        }

        public AuthenticationManager()
            : this(new WebManager(), new LocalStorageManager())
        {
        }

        public string Status { get; set; }

        public async Task<bool> Logout()
        {
            return await _localStorageManager.RemoveCookies(Constants.CookieFile);
        }

        public async Task<bool> Authenticate(string username, string password,
            int timeout = Constants.DefaultTimeoutInMilliseconds)
        {
            if (!_webManager.IsNetworkAvailable)
            {
                throw new LoginFailedException(
                    "The network is unavailable. Check your network settings and please try again.");
            }

            try
            {
                return await SendLoginData(username, password);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendLoginData(string username, string password)
        {
            CookieContainer cookies = await _webManager.PostData(
                Constants.LoginUrl, string.Format(
                    "action=login&username={0}&password={1}",
                    username.Replace(" ", "+"),
                    WebUtility.UrlEncode(password)));

            if (cookies.Count < 2)
            {
                return false;
            }

            var fixedCookieContainer = new CookieContainer();

            foreach (Cookie cookie in cookies.GetCookies(new Uri(Constants.CookieDomainUrl)))
            {
                var fixedCookie = new Cookie(cookie.Name, cookie.Value, "/", ".somethingawful.com");
                fixedCookieContainer.Add(new Uri(Constants.CookieDomainUrl), fixedCookie);
            }

            await _localStorageManager.SaveCookie(Constants.CookieFile, cookies, new Uri(Constants.CookieDomainUrl));
            return true;
        }
    }
}
