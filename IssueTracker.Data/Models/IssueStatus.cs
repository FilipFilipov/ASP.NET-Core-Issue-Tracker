using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

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
