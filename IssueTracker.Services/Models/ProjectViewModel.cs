using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IssueTracker.Services.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Leader")]
        public string LeaderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [DisplayName("Issue Priorities")]
        [Required]
        public int[] PriorityIds { get; set; }

        public string[] Labels { get; set; } = new string[0];

        [DisplayName("Issue Labels")]
        [Required]
        public string LabelsString
        {
            get => string.Join(", ", Labels);
            set => Labels = value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).ToArray();
        }
        }
}
