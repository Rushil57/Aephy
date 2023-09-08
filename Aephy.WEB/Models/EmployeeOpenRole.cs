using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class EmployeeOpenRole
    {        
        public int Id { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? JobDescription { get; set; }
        public DateTime? CreatedDateTime { get; set; }
    }
}
