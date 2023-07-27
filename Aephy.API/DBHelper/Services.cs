using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class Services
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? ServicesName { get; set; }

        public bool Active { get; set; }

        public bool IsActiveFreelancer { get; set; }

        public bool IsActiveClient { get; set; }
    }

}
