using System;
using System.Collections.Generic;
using System.Text;

namespace IssueTracker.Data.Models
{
    public class IssueLabels
    {
        public int IssueId { get; set; }

        public int LabelId { get; set; }

        public Issue Issue { get; set; }

        public Label Label { get; set; }
    }
}
