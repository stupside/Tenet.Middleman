using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tenet.Domain.Configuration;

namespace Tenet.Domain.Entities
{
    public class Driver
    {
        public Driver() { }

        public Driver(string path, DriverConfiguration configuration)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(Path));

            if (string.IsNullOrEmpty(configuration.Secret))
                throw new ArgumentNullException(nameof(Secret));

            if (string.IsNullOrEmpty(configuration.Bytes))
                throw new ArgumentNullException(nameof(DriverContent.Bytes));

            if (configuration.Expiry < 5000 || configuration.Expiry > 15000)
                throw new ArgumentException("Expiry must be between 5000 and 15 000");

            this.Path = path;
            this.Pid = configuration.Pid;
            this.Name = configuration.Name;
            this.Secret = configuration.Secret;
            this.DriverContent = new DriverContent()
            {
                Bytes = configuration.Bytes
            };
            this.Expiry = configuration.Expiry;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public int Pid { get; set; }
        public string Name { get; set; }
        public DriverContent DriverContent { get; set; }
        public string Secret { get; set; }
        public double Expiry { get; set; }
        public string Path { get; set; }
    }
}
