using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AwfulForumsLibrary.Interfaces;
using AwfulForumsLibrary.Models.Web;

namespace AwfulForumsLibrary.Managers
{
    public class WebManager : IWebManager
    {
        public WebManager(CookieContainer authenticationCookie = null)
        {
            AuthenticationCookie = authenticationCookie;
        }
        public CookieContainer AuthenticationCookie { get; set; }
        private const string Accept = "text/html, application/xhtml+xml, */*";

        private const string PostContentType = "application/x-www-form-urlencoded";

        private const string ReplyBoundary = "----WebKitFormBoundaryYRBJZZBPUZAdxj3S";
        private const string EditBoundary = "----WebKitFormBoundaryksMFcMGBHc3jdB0P";
        private const string ReplyContentType = "multipart/form-data; boundary=" + ReplyBoundary;
        private const string EditContentType = "multipart/form-data; boundary=" + EditBoundary;

        public async Task<Result> GetData(string uri)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false
            };
            if (AuthenticationCookie != null)
            {
                handler.CookieContainer = AuthenticationCookie;
            }
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.UtcNow;
                var result = await client.GetAsync(new Uri(uri));
                var stream = await result.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    string html = reader.ReadToEnd();
                    return new Result(result.IsSuccessStatusCode, html, "", "", result.RequestMessage.RequestUri.AbsoluteUri);
                }
            }
        }

        public Task<Result> PostArchiveData(string uri, string data)
        {
            throw new NotImplementedException();
        }

        public async Task<Result> PostData(string uri, FormUrlEncodedContent data)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false
            };
            if (AuthenticationCookie != null)
            {
                handler.CookieContainer = AuthenticationCookie;
            }
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.UtcNow;
                var result = await client.PostAsync(new Uri(uri), data);
                var stream = await result.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    string html = reader.ReadToEnd();
                    return new Result(result.IsSuccessStatusCode, html);
                }
            }
        }

        public async Task<CookieContainer> PostDataLogin(string url, string data)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = Accept;
            request.CookieContainer = new CookieContainer();
            request.Method = "POST";
            request.ContentType = PostContentType;
            request.UseDefaultCredentials = false;

            using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                writer.Write(data);
            }

            WebResponse response = await request.GetResponseAsync();
            return request.CookieContainer;
        }

        public async Task<Result> PostFormData(string uri, MultipartFormDataContent form)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false
            };
            if (AuthenticationCookie != null)
            {
                handler.CookieContainer = AuthenticationCookie;
            }
            using (var client = new HttpClient(handler))
            {
                var result = await client.PostAsync(new Uri(uri), form);
                var stream = await result.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream, Encoding.GetEncoding("ISO-8859-1")))
                {
                    string html = reader.ReadToEnd();
                    var newResult = new Result(result.IsSuccessStatusCode, html)
                    {
                        RequestUri = result.RequestMessage.RequestUri.ToString()
                    };
                    return newResult;
                }
            }
        }
    }
}
