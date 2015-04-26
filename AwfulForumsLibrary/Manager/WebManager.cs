using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AwfulForumsLibrary.Exceptions;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using HtmlAgilityPack;

namespace AwfulForumsLibrary.Manager
{
    public class WebManager : IWebManager
    {
        private const string Accept = "text/html, application/xhtml+xml, */*";

        private const string PostContentType = "application/x-www-form-urlencoded";

        private const string ReplyBoundary = "----WebKitFormBoundaryYRBJZZBPUZAdxj3S";
        private const string EditBoundary = "----WebKitFormBoundaryksMFcMGBHc3jdB0P";
        private const string ReplyContentType = "multipart/form-data; boundary=" + ReplyBoundary;
        private const string EditContentType = "multipart/form-data; boundary=" + EditBoundary;
        private readonly ILocalStorageManager _localStorageManager;

        static WebManager()
        {
            CookieJar = new List<Cookie>();
        }

        public WebManager(ILocalStorageManager localStorageManager)
        {
            _localStorageManager = localStorageManager;
        }

        public WebManager()
            : this(new LocalStorageManager())
        {
        }

        public static List<Cookie> CookieJar { get; private set; }

        public bool IsNetworkAvailable
        {
            get { return NetworkInterface.GetIsNetworkAvailable(); }
        }

        public async Task<Result> PostArchiveData(string uri, string data)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Accept = Accept;
            request.CookieContainer = await _localStorageManager.LoadCookie(Constants.CookieFile);
            request.Method = "POST";
            request.ContentType = PostContentType;
            request.UseDefaultCredentials = false;

            using (var writer = new StreamWriter(await request.GetRequestStreamAsync()))
            {
                writer.Write(data);
            }

            WebResponse response = await request.GetResponseAsync();
            using (Stream responseStream = response.GetResponseStream())
            {
                using (var reader = new StreamReader(responseStream))
                {
                    string html = reader.ReadToEnd();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    return new Result(doc, request.RequestUri.AbsoluteUri);
                }
            }
        }

        public async Task<CookieContainer> PostData(string url, string data)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = Accept;
            request.CookieContainer = await _localStorageManager.LoadCookie(Constants.CookieFile);
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

        public async Task<HttpResponseMessage> PostFormData(string url, MultipartFormDataContent form)
        {
            // TODO: This is a temp solution. Every post should use HttpWebRequest or HttpClient, but not both. 
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = await _localStorageManager.LoadCookie(Constants.CookieFile),
                UseCookies = true,
                UseDefaultCredentials = false
            };
            var httpClient = new HttpClient(handler);
            HttpResponseMessage result = await httpClient.PostAsync(url, form);
            return result;
        }


        public async Task<Result> GetData(string url)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                CookieContainer = await _localStorageManager.LoadCookie(Constants.CookieFile),
                UseCookies = true,
                UseDefaultCredentials = false
            };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.IfModifiedSince = DateTimeOffset.UtcNow;
            HttpResponseMessage result = await httpClient.GetAsync(url);
            if (!result.IsSuccessStatusCode)
            {
                throw new WebManagerException(string.Format("Failed to load page: {0}", string.Concat(result.StatusCode, Environment.NewLine, url)));
            }
            Stream stream = await result.Content.ReadAsStreamAsync();
            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return new Result(doc, result.RequestMessage.RequestUri.AbsoluteUri);
            }
        }

        public class Result
        {
            public Result(HtmlDocument document, string absoluteUri)
            {
                Document = document;
                AbsoluteUri = absoluteUri;
            }

            public HtmlDocument Document { get; private set; }
            public string AbsoluteUri { get; private set; }
        }
    }
}
