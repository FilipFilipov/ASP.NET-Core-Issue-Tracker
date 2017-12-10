using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IssueTracker.Data.Models
{
    public class Label
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<ProjectLabels> ProjectLabels { get; set; }

        public ICollection<IssueLabels> IssueLabels { get; set; }

    }
}
