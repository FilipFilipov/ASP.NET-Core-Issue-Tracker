using System.Collections.Generic;
using System.ComponentModel;

namespace IssueTracker.Services.Models.Issue
{
    public class IssueViewModel : IssueBaseModel
    {
        public bool CanEdit { get; set; }

        public bool CanDeleteComments { get; set; }

        public int ProjectId { get; set; }

        public string ProjectLeadId { get; set; }
      
        [DisplayName("Labels")]
        public ICollection<int> SelectedLabelIds { get; set; } = new List<int>();
    }
}
