using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tenet.Domain.Entities
{
    public class DriverContent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Driver Driver { get; set; }
        public string DriverId { get; set; }
        public string Bytes { get; set; }
    }
}
