using System.ComponentModel;

namespace IssueTracker.Services.Models.Project
{
    public class ProjectListModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DisplayName("Leader")]
        public string LeaderName { get; set; }

        public int Issues { get; set; }
    }
}
