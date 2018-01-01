using System.Collections.Generic;

namespace IssueTracker.Services.Models.Issue
{
    public class IssueDetailsModel : IssueBaseModel
    {
        public int ProjectId { get; set; }

        public string Assignee { get; set; }

        public ICollection<string> Labels;

        public string LabelsString => string.Join(", ", Labels);
    }
}
