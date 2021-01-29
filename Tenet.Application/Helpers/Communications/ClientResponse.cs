using System.Net;

namespace Tenet.Application.Helpers.Communications
{
    public class ClientResponse<T>
    {
        public ClientResponse(bool success, string message, HttpStatusCode code = HttpStatusCode.OK, T data = default)
        {
            this.Success = success;
            this.Message = message ?? string.Empty;
            this.Data = data;
        }

        /// <summary>
        /// Produces a failure response.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ClientResponse(string message) : this(false, message, HttpStatusCode.BadRequest, default(T))
        {
        }

        /// <summary>
        /// Producess a successful response.
        /// </summary>
        /// <param name="data">Returned data.</param>
        public ClientResponse(T data) : this(true, string.Empty, HttpStatusCode.OK, data)
        {
        }

        public bool Success { get; protected set; }
        public string Message { get; protected set; }
        public HttpStatusCode Code { get; protected set; }
        public T Data { get; protected set; }
    }
}
