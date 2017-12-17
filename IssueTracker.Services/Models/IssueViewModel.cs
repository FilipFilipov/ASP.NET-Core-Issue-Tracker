using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IssueTracker.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Services.Models
{
    public class IssueViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Assignee")]
        public string AssigneeId { get; set; }

        public IssueStatus Status { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

        public PriorityType? Priority { get; set; }

        public ICollection<int> Labels { get; set; } = new List<int>();

        public IEnumerable<SelectListItem> RemoveInvalidStatusTransitions(
            IEnumerable<SelectListItem> allStatuses)
        {
            string[] invalidStatuses = null;
            

            return allStatuses.Where(sli => !invalidStatuses.Contains(sli.Text));
        }
    }
}
