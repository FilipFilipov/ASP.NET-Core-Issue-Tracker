using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IssueTracker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Implementations
{
    public class PrioritiesService : AbstractService, IPrioritiesService
    {
        public PrioritiesService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<SelectListItem[]> ListProjectPriorities(int projectId)
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
