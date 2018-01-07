using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models.Issue;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectViewModel : ProjectBaseModel
    {    
        [Required]
        [StringLength(100, MinimumLength = 1)]
        [Remote("IsNameAvailable", "Projects", AdditionalFields = "Id")]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [DisplayName("Issue Priorities")]
        [Required]
        public PriorityType[] Priorities { get; set; }

        public string[] Labels { get; set; } = new string[0];

        [DisplayName("Issue Labels")]
        public string LabelsString
        {
            get => string.Join(", ", Labels);
            set => Labels = value == null ?
                new string[0] :
                value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        }

        public ICollection<IssueListModel> Issues { get; set; } = new List<IssueListModel>();       
    }
}
