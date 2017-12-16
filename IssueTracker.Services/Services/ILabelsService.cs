using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Services.Services
{
    public interface ILabelsService
    {
        Task<SelectListItem[]> ListLabelsAsync(int projectId);

        Task<bool> LabelsExistInProjectAsync(int projectId, IEnumerable<int> labelIds);
    }
}
