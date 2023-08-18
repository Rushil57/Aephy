using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class LevelRange
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string? Level { get;set; }

        public decimal minLevel { get; set; }

        public decimal maxLevel { get; set; }

    }
}
