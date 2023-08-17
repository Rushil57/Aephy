using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class SolutionSuccessfullProject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionId { get; set; }
        public int IndustryId { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

    }

    public class SolutionSuccessfullProjectResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SolutionSuccessfullProjectId { get; set; }

        public string? ResultKey { get; set; }

        public string? ResultValue { get; set; }
    }
}
