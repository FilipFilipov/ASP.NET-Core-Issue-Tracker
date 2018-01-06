using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models.Issue;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectDetailsModel : ProjectBaseModel
    {   
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DisplayName("Leader")]
        public string LeaderName { get; set; }
        
        [DisplayName("Issue Priorities")]
        [Required]
        public PriorityType[] Priorities { get; set; }

        [DisplayName("Issue Labels")]
        public string[] Labels { get; set; } = new string[0];

        public ICollection<IssueListModel> Issues { get; set; }
    }
}
