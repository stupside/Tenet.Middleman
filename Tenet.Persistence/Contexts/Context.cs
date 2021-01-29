using Microsoft.EntityFrameworkCore;
using Tenet.Domain.Entities;

namespace Tenet.Persistence.Contexts
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        public DbSet<Driver> Drivers { get; set; }

        public DbSet<DriverInstance> DriverInstances { get; set; }

        public DbSet<DriverContent> DriverContents { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Driver>()
                .HasOne(m => m.DriverContent)
                .WithOne(m => m.Driver)
                .HasForeignKey<DriverContent>(m => m.DriverId);

            builder.Entity<Driver>()
                .HasMany<DriverInstance>()
                .WithOne(m => m.Driver);

            builder.Entity<Driver>()
                .HasIndex(m => new { m.Pid })
                .IsUnique();

            builder.Entity<DriverInstance>()
                .HasIndex(m => new { m.DriverId, m.Ip })
                .IsUnique();
        }
    }
}
