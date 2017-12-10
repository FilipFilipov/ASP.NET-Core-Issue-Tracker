using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Data.Models
{
    public class Priority
    {
        public int ProjectId { get; set; }

        public PriorityType PriorityType { get; set; }
    }
}
