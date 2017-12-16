using System;
using System.ComponentModel.DataAnnotations;
using IssueTracker.Data.Models;

namespace IssueTracker.Services.Models
{
    public class IssueListModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Assignee { get; set; }

        public string Project { get; set; }

        public IssueStatus Status { get; set; }

        [DisplayFormat(DataFormatString = "{0:d}")]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        public PriorityType? Priority { get; set; }

        public string[] Labels { get; set; }
    }
}
