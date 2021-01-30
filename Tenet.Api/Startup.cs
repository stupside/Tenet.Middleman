using System;
using AspNetCore.Proxy;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tenet.Domain.Configuration;
using Tenet.Infrastructure.Helpers;
using Tenet.Persistence;
using Tenet.Persistence.Contexts;

namespace Tenet.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddDbContext<Context>(opt => opt.UseInMemoryDatabase("Tenet.Self"));

            services.AddDataProtection()
                .SetDefaultKeyLifetime(TimeSpan.FromDays(7))
                .SetApplicationName("Tenet.Self.Auth")
                .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration() {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });

            services.AddHttpClient<Client>(client => {
                client.BaseAddress = new Uri("http://api.tenet/v3/");
                client.DefaultRequestHeaders.Clear();
            });

            services.AddControllers();

            services.Configure<EndpointConfiguration>(_configuration.GetSection("Endpoints"));
            services.Configure<StreamingConfiguration>(_configuration.GetSection("Streaming"));

            services.AddProxies();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterAssemblyModules(typeof(Tenet.Infrastructure.loC.Modules.Module).Assembly);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.SeedInMemory(_configuration.GetSection("Streaming").Get<StreamingConfiguration>());

            //app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { 
                endpoints.MapControllers(); 
            });
        }
    }
}
