﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aephy.API.DBHelper
{
    public class ProjectReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string ClientId { get; set; }
        public int SolutionId { get; set; }
        public int IndustryId { get; set; }
        public string? Feedback_Message { get; set; }

        public int? WellDefinedProjectScope { get; set; }

        public int? AdherenceToProjectScope { get; set; }

        public int? DeliverablesQuality { get; set; }

        public int? MeetingTimeliness { get; set; }

        public int? Clientsatisfaction { get; set; }

        public int? AdherenceToBudget { get; set; }

        public int? LikeToRecommend { get; set; }

        public DateTime CreateDateTime { get; set; }
    }
}
