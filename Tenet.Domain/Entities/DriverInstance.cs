using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenet.Domain.Entities
{
    public class DriverInstance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public Driver Driver { get; set; }
        public string DriverId { get; set; }
        public string Key { get; set; }
        public string Ip { get; set; }
        public DateTime Expiry { get; set; }
    }
}
