using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class Notifications
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? NotificationTitle { get; set; }
        public string? NotificationText { get; set; }

        public string? FromUserId { get; set;}

        public string? ToUserId { get; set; }

        public DateTime NotificationTime { get; set; }

        public bool IsRead { get; set; }
    }
}
