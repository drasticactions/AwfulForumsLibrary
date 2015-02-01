using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AwfulForumsLibrary.Interface;
using AwfulForumsLibrary.Tools;
using PCLStorage;

namespace AwfulForumsLibrary.Manager
{
    public class LocalStorageManager : ILocalStorageManager
    {
        public async Task SaveCookie(string filename, CookieContainer rcookie, Uri uri)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            IFile file = await rootFolder.CreateFileAsync(Constants.CookieFile, CreationCollisionOption.ReplaceExisting);

            try
            {
                using (var transaction = await file.OpenAsync(FileAccess.ReadAndWrite))
                {
                    CookieSerializer.Serialize(rcookie.GetCookies(uri), uri, transaction);
                    await transaction.WriteAsync(ReadFully(transaction), 0, (int)transaction.Length - 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Failed to save cookies used for logging in. {0}", ex.Message));
            }
        }

        public async Task<CookieContainer> LoadCookie(string filename)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            try
            {
                IFile file = await rootFolder.GetFileAsync(Constants.CookieFile);
                using (Stream stream = await file.OpenAsync(FileAccess.Read))
                {
                    return CookieSerializer.Deserialize(new Uri(Constants.CookieDomainUrl), stream);
                }
            }
            catch
            {
                //Ignore, we will ask for log in information.
            }
            return new CookieContainer();
        }

        public async Task<bool> RemoveCookies(string filename)
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            try
            {
                IFile file = await rootFolder.GetFileAsync(Constants.CookieFile);
                await file.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
