using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class IssuesService : AbstractService, IIssuesService
    {
        public IssuesService(IssueTrackerDbContext db, UserManager<User> userManager) :
            base(db, userManager)
        {
        }

        public async Task<IssueListModel[]> GetIssuesAsync(
            string userId = null, bool withProject = false)
        {
            IQueryable<Issue> issues = db.Issues;
            if (userId != null)
            {
                issues = issues.Where(i => i.AssigneeId == userId);
            }

            return await issues
                .ProjectTo<IssueListModel>(i => withProject ? i.Project : null)
                .ToArrayAsync();
        }

        public async Task<ServiceResult<T>> GetIssueAsync<T>(int id, ClaimsPrincipal user)
            where T : IssueBaseModel
        {
            var issue = await db.Issues.Where(i => i.Id == id)
                .ProjectTo<T>(new { currentUserId = userManager.GetUserId(user) })
                .SingleOrDefaultAsync();

            return issue != null ?
                new ServiceResult<T>(issue) : 
                new ServiceResult<T>("Issue not found");
        }

        public async Task<ServiceResult<IssueViewModel>> GetIssueForEditingAsync(
            int id, ClaimsPrincipal user)
        {
            var result = await GetIssueAsync<IssueViewModel>(id, user);

            if (result.NotificationType == NotificationType.Error)
            {
                return result;
            }

            var isAdmin = IsAdmin(user);
            var isProjectLead = IsProjectLead(result.Value.ProjectLeadId, user);
            var isIssueAsignee = IsIssueAsignee(result.Value.AssigneeId, user);

            if (!isAdmin && !isProjectLead && !isIssueAsignee)
            {
                return new ServiceResult<IssueViewModel>(
                    "Only admin, project lead or issue asignee can perform this action");
            }

            result.Value.CanEdit = isAdmin || isProjectLead;
            result.Value.CanDeleteComments = isAdmin || isProjectLead;

            return result;
        }

        public async Task<ServiceResult<Issue>> CreateIssueAsync(
            int projectId, IssueViewModel model, ClaimsPrincipal user)
        {
            var project = db.Projects.SingleOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return new ServiceResult<Issue>("Project not found");
            }

            if (!IsAdmin(user) && !IsProjectLead(project.LeaderId, user))
            {
                return new ServiceResult<Issue>(
                    "Only admin or project lead can perform this action");
            }

            var newIssue = Mapper.Map<Issue>(model);
            newIssue.Key = string.Concat(model.Title.Split().Select(word => word.ToUpper()[0]));
            newIssue.Status = IssueStatus.Open;

            project.Issues.Add(newIssue);
            await db.SaveChangesAsync();

            return new ServiceResult<Issue>(newIssue, "Issue has been added");
        }

        public async Task<ServiceResult<Issue>> EditIssueAsync(
            IssueViewModel model, ClaimsPrincipal user)
        {
            var dbIssue = await db.Issues
                .Include(i => i.Project)
                .Include(i => i.IssueLabels)
                .SingleOrDefaultAsync(i => i.Id == model.Id);

            if (dbIssue == null)
            {
                return new ServiceResult<Issue>("Issue not found");
            }

            var isAdmin = IsAdmin(user);
            var isProjectLead = IsProjectLead(dbIssue.Project.LeaderId, user);
            var isIssueAsignee = IsIssueAsignee(dbIssue.AssigneeId, user);

            if (!isAdmin && !isProjectLead && !isIssueAsignee)
            {
                return new ServiceResult<Issue>(
                    "Only admin, project lead or issue asignee can perform this action");
            }

            if (isAdmin || isProjectLead)
            {
                Mapper.Map(model, dbIssue);
            }
            else
            {
                dbIssue.Status = model.Status;
            }
            
            await db.SaveChangesAsync();

            return new ServiceResult<Issue>(dbIssue, "Issue has been saved");
        }

        public async Task<bool> IsIssueAsigneeAsync(int issueId, ClaimsPrincipal user)
        {
            var issue = await db.Issues.SingleOrDefaultAsync(i => i.Id == issueId);

            return IsIssueAsignee(issue?.AssigneeId, user);
        }

        private bool IsIssueAsignee(string assigneeId, ClaimsPrincipal user)
        {         
            return assigneeId == userManager.GetUserId(user);
        }
    }
}