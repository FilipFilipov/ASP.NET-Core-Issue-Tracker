using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IssueTracker.Data.Models;

namespace IssueTracker.Services.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [DisplayName("Leader")]
        public string LeaderId { get; set; }

        [DisplayName("Issue Priorities")]
        [Required]
        public PriorityType[] Priorities { get; set; }

        public string[] Labels { get; set; } = new string[0];

        [DisplayName("Issue Labels")]
        [Required]
        public string LabelsString
        {
            get => string.Join(", ", Labels);
            set => Labels = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToArray();
        }

        public ICollection<IssueListModel> Issues { get; set; }
    }
}
