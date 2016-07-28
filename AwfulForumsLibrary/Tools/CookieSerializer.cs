using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace AwfulForumsLibrary.Tools
{
    public class CookieSerializer
    {
        public static void Serialize(CookieCollection cookies, Uri address, Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(IEnumerable<Cookie>));
            var cookieList = cookies.OfType<Cookie>();

            serializer.WriteObject(stream, cookieList);
        }

        public static CookieContainer Deserialize(Uri uri, Stream stream)
        {
            var container = new CookieContainer();
            var serializer = new DataContractSerializer(typeof(IEnumerable<Cookie>));
            var cookies = (IEnumerable<Cookie>)serializer.ReadObject(stream);
            var cookieCollection = new CookieCollection();

            foreach (var fixedCookie in cookies.Select(cookie => new Cookie(cookie.Name, cookie.Value, "/", ".somethingawful.com")))
            {
                cookieCollection.Add(fixedCookie);
            }
            container.Add(uri, cookieCollection);
            return container;
        }

    }
}
