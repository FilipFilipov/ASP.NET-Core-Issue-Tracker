using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Implementations
{
    public class LabelsService : AbstractService, ILabelsService
    {
        public LabelsService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<string[]> GetLabelsAsync(string search, int? projectId)
        {
            return await db.Labels
                .Where(l =>
                    (string.IsNullOrEmpty(search) || l.Name.StartsWith(search)) &&
                    (!projectId.HasValue || l.ProjectLabels.Any(pl => pl.ProjectId == projectId)))
                .Select(l => l.Name)
                .ToArrayAsync();
        }
    }
}
