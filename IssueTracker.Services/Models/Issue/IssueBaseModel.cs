using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Data.Models;

namespace IssueTracker.Services.Models.Issue
{
    public abstract class IssueBaseModel
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Assignee")]
        public string AssigneeId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public IssueStatus Status { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

        public PriorityType? Priority { get; set; }

        public bool CanComment { get; set; }

        public ICollection<CommentViewModel> Comments;
    }
}
