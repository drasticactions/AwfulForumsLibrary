using System;
using System.Net;
using System.Threading.Tasks;

namespace AwfulForumsLibrary.Interface
{
    public interface ILocalStorageManager
    {
        Task SaveCookie(string filename, CookieContainer rcookie, Uri uri);
        Task<CookieContainer> LoadCookie(string filename);
        Task<bool> RemoveCookies(string filename);
    }
}
