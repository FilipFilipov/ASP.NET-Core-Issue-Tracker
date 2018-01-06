using System.Security.Claims;
using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services.Utilities;

namespace IssueTracker.Services.Services
{
    public interface IProjectsService
    {
        Task<ProjectListModel[]> GetProjectsAsync(string userId = null);

        Task<ServiceResult<T>> GetProjectAsync<T>(int projectId) where T: ProjectBaseModel;

        Task<ServiceResult<T>> GetProjectForEditingAsync<T>(
            int projectId, ClaimsPrincipal user) where T: ProjectBaseModel;

        Task<ServiceResult<Project>> CreateProjectAsync(ProjectViewModel model);

        Task<ServiceResult<Project>> EditProjectAsync(
            ProjectViewModel model, ClaimsPrincipal user);  

        Task<ServiceResult<Project>> DeleteProjectAsync(int id);

        Task<bool> ProjectExistsAsync(string name, int? excludingId = null);
    }
}
