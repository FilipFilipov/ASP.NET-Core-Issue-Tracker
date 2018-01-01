using System.Security.Claims;
using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Services.Utilities;

namespace IssueTracker.Services.Services
{
    public interface IIssuesService
    {
        Task<IssueListModel[]> GetIssuesAsync(string userId = null, bool withProject = false);

        Task<ServiceResult<T>> GetIssueAsync<T>(
            int id, ClaimsPrincipal user) where T : IssueBaseModel;

        Task<ServiceResult<IssueViewModel>> GetIssueForEditingAsync(
            int id, ClaimsPrincipal user);

        Task<ServiceResult<Issue>> CreateIssueAsync(
            int projectId, IssueViewModel model, ClaimsPrincipal user);

        Task<ServiceResult<Issue>> EditIssueAsync(
            IssueViewModel model, ClaimsPrincipal user);

        Task<bool> IsIssueAsigneeAsync(int issueId, ClaimsPrincipal user);
    }
}
