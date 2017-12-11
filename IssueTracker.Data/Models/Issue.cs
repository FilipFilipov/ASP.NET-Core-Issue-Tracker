using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;
using IssueTracker.Models;

namespace IssueTracker.Data.Models
{
    public class Issue
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(50)]
        public string Key { get; set; }

        public string AssigneeId { get; set; }

        public User Assignee { get; set; }

        public IssueStatus Status { get; set; }

        public DateTime DueDate { get; set; }

        public PriorityType? Priority { get; set; }

        public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
