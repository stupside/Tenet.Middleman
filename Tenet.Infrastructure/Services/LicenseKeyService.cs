using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Proxy.Options;
using Tenet.Application.Services;

namespace Tenet.Infrastructure.Services
{
    public class LicenseKeyService : ILicenseKeyService
    {
        private readonly IDriverService _drivers;

        public LicenseKeyService(IDriverService drivers)
        {
            _drivers = drivers;
        }

        public HttpProxyOptions Process()
        {
            string key = string.Empty;
            string ip = string.Empty;

            HttpProxyOptions options = HttpProxyOptionsBuilder.Instance
                .WithShouldAddForwardedHeaders(true)
                .WithBeforeSend((c, hrm) =>
                {
                    ip = c.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    key = c.Request.Headers["TN-KEY"];

                    return Task.CompletedTask;
                })
                .WithAfterReceive(async (c, hrm) =>
                {
                    if (hrm.IsSuccessStatusCode)
                    {
                        if(hrm.Headers.TryGetValues("TN-PID", out IEnumerable<string> strs))
                        {
                            if(int.TryParse(strs.FirstOrDefault(), out int pid))
                            {
                                hrm.Headers.Add("TN-IST", await _drivers.CreateInstance(pid, key, ip));
                            }
                        }
                    }

                    // Delete headers comming from tenet
                    c.Response.Headers.Remove("TN-KID");
                    c.Response.Headers.Remove("TN-PID");

                }).Build();

            return options;
        }
    }
}
