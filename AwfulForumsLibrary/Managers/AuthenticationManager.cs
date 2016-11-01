using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AwfulForumsLibrary.Models.Web;
using AwfulForumsLibrary.Tools;

namespace AwfulForumsLibrary.Managers
{
    public class AuthenticationManager
    {
        public async Task<AuthResult> LogoutAsync(CookieContainer cookies)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false,
                CookieContainer = cookieContainer
            };
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Referrer = new Uri("http://forums.somethingawful.com");
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2593.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept-Language", "ja,en-US;q=0.8,en;q=0.6");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch");
                client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                var response = await client.GetAsync(new Uri(EndPoints.LoginUrl + "action=logout&ma=9bb6fb52"));
                var authResult = new AuthResult()
                {
                    IsSuccess = response.IsSuccessStatusCode
                };
                return authResult;
            }
        }

        /// <summary>
        /// Authenticate a Something Awful user. This does not use the normal "WebManager" for handling the request
        /// because it requires we return the cookie container, so it can be used for actual authenticated requests.
        /// </summary>
        /// <param name="username">The Something Awful username.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="checkResult">Check the query string for login errors. Default is True.</param>
        /// <returns>An auth result object.</returns>
        public async Task<AuthResult> AuthenticateAsync(string username, string password, bool checkResult = true)
        {
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false
            };
            using (var client = new HttpClient(handler))
            {
                var dic = new Dictionary<string, string>
                {
                    ["action"] = "login",
                    ["username"] = username,
                    ["password"] = password
                };
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue()
                {
                    NoCache = true
                };
                var header = new FormUrlEncodedContent(dic);
                var response = await client.PostAsync(new Uri(EndPoints.LoginUrl), header);
                var authResult = new AuthResult()
                {
                    IsSuccess = cookieContainer.Count >= 3,
                    AuthenticationCookie = cookieContainer
                };
                if (!checkResult)
                {
                    return authResult;
                }
                try
                {
                    var location = "http:" + response.Headers.Location.OriginalString;
                    var uri = new Uri(location);
                    var queryString = Extensions.ParseQueryString(uri.Query);
                    if (!queryString.ContainsKey("loginerror")) return authResult;
                    if (queryString["loginerror"] == null) return authResult;
                    switch (queryString["loginerror"])
                    {
                        case "1":
                            authResult.Error = "Failed to enter phrase from the security image.";
                            break;
                        case "2":
                            authResult.Error = "The password you entered is wrong. Remember passwords are case-sensitive! Be careful... too many wrong passwords and you will be locked out temporarily.";
                            break;
                        case "3":
                            authResult.Error = "The username you entered is wrong, maybe you should try 'idiot' instead? Watch out... too many failed login attempts and you will be locked out temporarily.";
                            break;
                        case "4":
                            authResult.Error =
                                "You've made too many failed login attempts. Your IP address is temporarily blocked.";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    authResult.Error = ex.Message;
                }

                return authResult;
            }
        }

    
    }
}
