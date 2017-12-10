﻿namespace IssueTracker.Data.Models
{
    public class ProjectLabels
    {
        public int ProjectId { get; set; }

        public int LabelId { get; set; }

        public Project Project { get; set; }

        public Label Label { get; set; }
    }
}
