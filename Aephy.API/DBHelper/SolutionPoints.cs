﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Aephy.API.DBHelper
{
    public class SolutionPoints
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? point { get; set; }

        public int SolutionId { get; set; }

        public int IndustryId { get; set; }

        public string? FreelancerId { get; set; }
    }
}