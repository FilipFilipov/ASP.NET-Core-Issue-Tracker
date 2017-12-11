using System.Threading.Tasks;

namespace IssueTracker.Services
{
    public interface ILabelsService
    {
        Task<string[]> GetLabelsAsync(string search, int? projectId);
    }
}
