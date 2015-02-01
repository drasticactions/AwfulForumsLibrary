using System.Threading.Tasks;
using AwfulForumsLibrary.Tools;

namespace AwfulForumsLibrary.Interface
{
    public interface IAuthenticationManager
    {
        string Status { get; }

        Task<bool> Authenticate(string userName, string password,
            int timeout = Constants.DefaultTimeoutInMilliseconds);
    }
}
