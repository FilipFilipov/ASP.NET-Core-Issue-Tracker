using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;

namespace IssueTracker.Services.Services
{
    public interface IIssuesService
    {
        Task<Issue> CreateIssueAsync(int projectId, IssueViewModel model);

        Task<bool> IssueExistsAsync(int projectId, string title);
    }
}
