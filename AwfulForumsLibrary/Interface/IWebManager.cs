using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AwfulForumsLibrary.Manager;

namespace AwfulForumsLibrary.Interface
{
    public interface IWebManager
    {
        bool IsNetworkAvailable { get; }
        Task<WebManager.Result> GetData(string uri);

        Task<WebManager.Result> PostArchiveData(string uri, string data);
        Task<CookieContainer> PostData(string uri, string data);
        Task<HttpResponseMessage> PostFormData(string uri, MultipartFormDataContent form);
    }
}
