using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Data.Models
{
    public enum IssueStatus
    {
        Open,
        [Display(Name = "In Progress")]
        InProgress,
        Resolved,
        Closed,
        Reopened
    }
}
