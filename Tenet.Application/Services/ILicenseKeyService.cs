using AspNetCore.Proxy.Options;

namespace Tenet.Application.Services
{
    public interface ILicenseKeyService
    {
        HttpProxyOptions Process();
    }
}
