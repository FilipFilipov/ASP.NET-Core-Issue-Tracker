using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IssueTracker.Services.Models.Issue;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectViewModel : ProjectBaseModel
    {    
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public ICollection<IssueListModel> Issues { get; set; } = new List<IssueListModel>();
      
        [DisplayName("Issue Labels")]
        public string LabelsString
        {
            get => string.Join(", ", Labels);
            set => Labels = value == null ?
                new string[0] :
                value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }
    }
}
