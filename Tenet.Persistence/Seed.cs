using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tenet.Domain.Configuration;
using Tenet.Domain.Entities;
using Tenet.Persistence.Contexts;

namespace Tenet.Persistence
{
    public static class Seed
    {
        public static void SeedInMemory(this IApplicationBuilder app, StreamingConfiguration configuration)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();


            var context = (Context)scope.ServiceProvider.GetService(typeof(Context));
            if (context == null)
                throw new InvalidOperationException("Can't resolve " + typeof(Context));

            context.SeedDrivers(configuration.Drivers);
        }

        private static void SeedDrivers(this Context context, IEnumerable<string> drivers)
        {
            foreach(string driver in drivers)
            {
                if (string.IsNullOrEmpty(driver))
                    throw new NullReferenceException(nameof(DriverConfiguration));

                var content = File.ReadAllText(driver);
                if (string.IsNullOrEmpty(content))
                    throw new NullReferenceException(nameof(DriverConfiguration));

                DriverConfiguration configuration;
                try
                {
                    configuration = JsonSerializer.Deserialize<DriverConfiguration>(content);
                    if (configuration == null) 
                        throw new ArgumentNullException(nameof(DriverConfiguration));

                    Console.WriteLine($"Driver {configuration.Name} with path {driver} registered.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }

                context.Drivers.Add(new Driver(driver, configuration));
            }
            context.SaveChanges();
        }
    }
}
