using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Tenet.Application.Helpers;
using Tenet.Application.Helpers.Communications;

namespace Tenet.Infrastructure.Helpers
{
    public class Client : IClient
    {
        private readonly HttpClient _httpClient;

        public Client(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ClientResponse<T>> Post<T, TPostType>(string url, TPostType obj)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, obj);
                return await HandleResponse<T>(response);
            }
            catch
            {
                return new ClientResponse<T>("Cannot load ressource");
            }
        }

        public async Task<ClientResponse<T>> Put<T, TPutType>(string url, TPutType obj)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, obj);
                return await HandleResponse<T>(response);
            }
            catch
            {
                return new ClientResponse<T>("Cannot load ressource");
            }

        }

        public async Task<ClientResponse<T>> Get<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return await HandleResponse<T>(response);
            }
            catch
            {
                return new ClientResponse<T>("Cannot load ressource");
            }

        }

        public async Task<ClientResponse<T>> Delete<T>(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                return await HandleResponse<T>(response);
            }
            catch
            {
                return new ClientResponse<T>("Cannot load ressource");
            }
        }

        Task<ClientResponse<T>> IClient.HandleResponse<T>(HttpResponseMessage response)
        {
            throw new System.NotImplementedException();
        }

        public HttpClient CreateClient(string name)
        {
            throw new System.NotImplementedException();
        }

        private static async Task<ClientResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content?.ReadFromJsonAsync<T>();
                return new ClientResponse<T>(content);
            }
            return new ClientResponse<T>(false, await response.Content.ReadAsStringAsync(), response.StatusCode);
        }
    }
}
