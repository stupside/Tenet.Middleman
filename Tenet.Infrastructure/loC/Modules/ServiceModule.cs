using Autofac;
using Tenet.Application.Helpers;
using Tenet.Application.Services;
using Tenet.Infrastructure.Services;

namespace Tenet.Infrastructure.loC.Modules
{
    public class ServiceModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType(typeof(LicenseKeyService))
                .As(typeof(ILicenseKeyService));

            builder.RegisterType(typeof(DriverService))
                .As(typeof(IDriverService));

            builder.RegisterType(typeof(Client))
                .As(typeof(IClient));

            base.Load(builder);
        }
    }
}