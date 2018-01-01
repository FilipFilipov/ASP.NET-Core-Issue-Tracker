using System.Collections.Generic;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Models.Project;

namespace IssueTracker.Services.Models
{
    public class DashboardModel
    {
        public ICollection<ProjectListModel> Projects { get; set; }

        public ICollection<IssueListModel> Issues { get; set; }
    }
}
