using System.Net.Http;
using System.Threading.Tasks;
using Tenet.Application.Helpers.Communications;

namespace Tenet.Application.Helpers
{
    public interface IClient : IHttpClientFactory
    {
        Task<ClientResponse<T>> Post<T, TPostType>(string url, TPostType obj);

        Task<ClientResponse<T>> Put<T, TPutType>(string url, TPutType obj);

        Task<ClientResponse<T>> Get<T>(string url);

        Task<ClientResponse<T>> Delete<T>(string url);

        Task<ClientResponse<T>> HandleResponse<T>(HttpResponseMessage response);
    }
}