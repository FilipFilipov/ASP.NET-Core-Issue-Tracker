using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;

namespace IssueTracker.Services.Services
{
    public interface IIssuesService
    {
        Task<IssueViewModel> GetIssueAsync(int id, int projectId);

        Task<Issue> CreateIssueAsync(int projectId, IssueViewModel model);

        Task<Issue> EditIssueAsync(IssueViewModel model);
    }
}
