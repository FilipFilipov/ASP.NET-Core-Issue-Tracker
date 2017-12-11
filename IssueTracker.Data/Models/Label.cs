using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Data.Models
{
    public class Label
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<ProjectLabel> ProjectLabels { get; set; } = new List<ProjectLabel>();

        public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();

    }
}
