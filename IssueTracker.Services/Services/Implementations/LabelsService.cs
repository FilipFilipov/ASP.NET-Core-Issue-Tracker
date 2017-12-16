using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class LabelsService : AbstractService, ILabelsService
    {
        public LabelsService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<SelectListItem[]> ListLabelsAsync(int projectId)
        {
            return await db.Labels
                .Where(l => l.ProjectLabels.Any(pl => pl.ProjectId == projectId))
                .Select(l => new SelectListItem { Text = l.Name, Value = l.Id.ToString() })
                .ToArrayAsync();
        }

        public async Task<bool> LabelsExistInProjectAsync(int projectId, IEnumerable<int> labelIds)
        {
            var dbLabelsCount = await db.ProjectLabels
                .CountAsync(pl => pl.ProjectId == projectId && labelIds.Contains(pl.LabelId));

            return dbLabelsCount == labelIds.Count();
        }
    }
}
