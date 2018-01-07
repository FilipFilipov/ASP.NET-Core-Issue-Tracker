using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Models;

namespace IssueTracker.Data.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        public string LeaderId { get; set; }

        public User Leader { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Key { get; set; }

        public ICollection<Priority> Priorities { get; set; } = new List<Priority>();

        public ICollection<ProjectLabel> ProjectLabels { get; set; } = new List<ProjectLabel>();

        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    }
}
