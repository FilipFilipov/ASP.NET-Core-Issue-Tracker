using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Services.Services
{
    public interface IPrioritiesService
    {
        Task<SelectListItem[]> ListPrioritiesAsync(int projectId);
    }
}
