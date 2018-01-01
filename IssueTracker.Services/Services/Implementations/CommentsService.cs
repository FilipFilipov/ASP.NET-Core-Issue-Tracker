using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class CommentsService : AbstractService, ICommentsService
    {
        public CommentsService(IssueTrackerDbContext db, UserManager<User> userManager) :
            base(db, userManager)
        {
        }

        public async Task<ICollection<CommentViewModel>> GetIssueCommentsAsync(int issueId)
        {
            return await db.Comments.Where(c => c.IssueId == issueId)
                .OrderByDescending(c => c.Created)
                .ProjectTo<CommentViewModel>()
                .ToArrayAsync();
        }

        public async Task<ServiceResult<Comment>> CreateCommentAsync(
            int projectId, int issueId, string text, ClaimsPrincipal user)
        {
            var project = await db.Projects
                .Include(p => p.Issues)
                .SingleOrDefaultAsync(p => p.Id == projectId);
            if (project == null || project.Issues.All(i => i.Id != issueId))
            {
                return new ServiceResult<Comment>("Issue not found");
            }

            if (!IsAdmin(user) &&
                !IsProjectLead(project.LeaderId, user) &&
                !IsProjectAsignee(project, user))
            {
                return new ServiceResult<Comment>(
                    "Only an admin, project lead or issue asignee");
            }

            var comment = db.Comments.Add(new Comment
            {
                AuthorId = userManager.GetUserId(user),
                IssueId = issueId,
                Text = text,
                Created = DateTime.UtcNow
            }).Entity;

            await db.SaveChangesAsync();

            return new ServiceResult<Comment>(comment, "Comment has been saved.");
        }

        public async Task<ServiceResult<Comment>> DeleteCommentAsync(
            int id, ClaimsPrincipal user)
        {
            var comment = await db.Comments
                .Include(c => c.Issue.Project)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return new ServiceResult<Comment>("Comment not found");
            }

            if (!IsAdmin(user) && !IsProjectLead(comment.Issue.Project.LeaderId, user))
            {
                return new ServiceResult<Comment>(
                    "Only an admin or project lead can perform this action");
            }
            
            db.Comments.Remove(comment);

            await db.SaveChangesAsync();

            return new ServiceResult<Comment>(comment, "Comment has been deleted.");
        }

        private bool IsProjectAsignee(Project project, ClaimsPrincipal user)
        {
            var userId = userManager.GetUserId(user);
            return project.Issues.Any(i => i.AssigneeId == userId);
        }
    }
}
