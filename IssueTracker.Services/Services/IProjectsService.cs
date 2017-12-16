using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;

namespace IssueTracker.Services.Services
{
    public interface IProjectsService
    {
        Task<ProjectListModel[]> GetProjectsAsync();

        Task<ProjectViewModel> GetProjectAsync(int projectId);

        Task<Project> CreateProjectAsync(ProjectViewModel model);

        Task<bool> ProjectExistsAsync(int id);

        Task<bool> ProjectExistsAsync(string name);
    }
}
