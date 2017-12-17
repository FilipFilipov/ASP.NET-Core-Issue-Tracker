using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Services.Extensions;
using IssueTracker.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class IssuesService : AbstractService, IIssuesService
    {
        public IssuesService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<IssueViewModel> GetIssueAsync(int id, int projectId)
        {
            return await db.Issues.Where(i => i.Id == id && i.ProjectId == projectId)
                .ProjectTo<IssueViewModel>()
                .SingleOrDefaultAsync();
        }

        public async Task<Issue> CreateIssueAsync(int projectId, IssueViewModel model)
        {
            var newIssue = Mapper.Map<Issue>(model);
            newIssue.Key = string.Concat(model.Title.Split().Select(word => word.ToUpper()[0]));
            newIssue.Status = IssueStatus.Open;
            newIssue.IssueLabels = model.Labels.Select(id => new IssueLabel { LabelId = id }).ToArray();

            db.Projects.Single(p => p.Id == projectId).Issues.Add(newIssue);
            await db.SaveChangesAsync();

            return newIssue;
        }

        public async Task<Issue> EditIssueAsync(IssueViewModel model)
        {
            var issue = await db.Issues.Include(i => i.IssueLabels)
                .SingleAsync(i => i.Id == model.Id);

            Mapper.Map(model, issue);
            issue.IssueLabels.ReplaceEntityCollection(
                model.Labels.Select(id => new IssueLabel { LabelId = id }).ToArray(),
                il => il.LabelId);

            await db.SaveChangesAsync();

            return issue;
        }
    }
}