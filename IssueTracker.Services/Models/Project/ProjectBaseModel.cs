using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Data.Models;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectBaseModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Leader")]
        public string LeaderId { get; set; }

        [DisplayName("Issue Priorities")]
        [Required]
        public PriorityType[] Priorities { get; set; }

        [DisplayName("Issue Labels")]
        public string[] Labels { get; set; } = new string[0];
    }
}
