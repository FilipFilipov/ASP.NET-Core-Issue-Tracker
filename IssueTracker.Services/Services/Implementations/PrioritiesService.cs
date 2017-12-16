using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class PrioritiesService : AbstractService, IPrioritiesService
    {
        public PrioritiesService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<SelectListItem[]> ListPrioritiesAsync(int projectId)
        {
            return await db.Priorities.Where(p => p.ProjectId == projectId)
                .Select(p => new SelectListItem
                {
                    Text = p.PriorityType.ToString(),
                    Value = ((int) p.PriorityType).ToString()
                })
                .ToArrayAsync();
        }
    }
}
