using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class IssuesService : AbstractService, IIssuesService
    {
        public IssuesService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<Issue> CreateIssueAsync(int projectId, IssueViewModel model)
        {
            var newIssue = Mapper.Map<Issue>(model);
            newIssue.Key = string.Concat(model.Title.Split().Select(word => word.ToUpper()[0]));
            newIssue.Status = IssueStatus.Open;

            db.Projects.Single(p => p.Id == projectId).Issues.Add(newIssue);
            await db.SaveChangesAsync();

            return newIssue;
        }

        public async Task<bool> IssueExistsAsync(int projectId, string title)
        {
            return await db.Issues.AnyAsync(i => i.ProjectId == projectId && i.Title == title);
        }
    }
}
