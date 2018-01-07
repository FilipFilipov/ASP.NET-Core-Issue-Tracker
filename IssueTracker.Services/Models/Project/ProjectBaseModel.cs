using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectBaseModel
    {
        public int? Id { get; set; }

        [Required]
        [DisplayName("Leader")]
        public string LeaderId { get; set; }
    }
}
