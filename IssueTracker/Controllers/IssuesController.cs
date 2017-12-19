using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Models;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Services;
using IssueTracker.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace IssueTracker.Web.Controllers
{
    [Authorize]
    [Route("Projects/{projectId:int}/Issues/[action]/{id?}")]
    public class IssuesController : Controller
    {
        private readonly IProjectsService projects;
        private readonly IIssuesService issues;
        private readonly IUsersService users;
        private readonly IPrioritiesService priorities;
        private readonly ILabelsService labels;
        private readonly ICommentsService comments;
        private readonly UserManager<User> userManager;

        public IssuesController(IProjectsService projects, IIssuesService issues,
            IUsersService users, IPrioritiesService priorities, ILabelsService labels,
            ICommentsService comments, UserManager<User> userManager)
        {
            this.projects = projects;
            this.issues = issues;
            this.users = users;
            this.priorities = priorities;
            this.labels = labels;
            this.comments = comments;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(int projectId, int id)
        {
            var currentUserId = userManager.GetUserId(User);
            if (User.IsInRole("Admin") ||
                await projects.IsProjectLeadAsync(projectId, currentUserId) ||
                await issues.IsIssueAsigneeAsync(id, currentUserId))
            {
                return RedirectToAction("Edit", new { projectId, id });
            }

            var issue = await issues.GetIssueAsync<IssuesDetailsModel>(id);
            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }

        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId)
        {
            if (!await projects.ProjectExistsAsync(projectId))
            {
                return NotFound();
            }

            if (!await projects.IsProjectLeadAsync(projectId, userManager.GetUserId(User)) ||
                !User.IsInRole("Admin"))
            {
                return Unauthorized();
            }

            await GetDropdownValues(projectId);
            return View(new IssueViewModel());
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId, IssueViewModel model)
        {
            var project = await projects.GetProjectAsync(projectId);
            if (project == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") ||
                !await projects.IsProjectLeadAsync(projectId, userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            await PerformServerValidationsAsync(project, model);
            if (!ModelState.IsValid)
            {
                await GetDropdownValues(projectId);
                return View(model);
            }

            await issues.CreateIssueAsync(projectId, model);
            this.AddNotification("Issue saved!", NotificationType.Success);

            return RedirectToAction("Edit", "Projects", new { id = projectId });
        }

        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int projectId, int id)
        {
            var issue = await issues.GetIssueAsync<IssueViewModel>(id);
            if (issue == null)
            {
                return NotFound();
            }

            issue.CanDeleteComments = User.IsInRole("Admin") ||
                await projects.IsProjectLeadAsync(projectId, userManager.GetUserId(User));

            await GetDropdownValues(projectId, issue.Status);
            return View(issue);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int projectId, IssueViewModel model)
        {
            var issue = await issues.GetIssueAsync<IssueViewModel>(model.Id);
            if (issue == null)
            {
                return NotFound();
            }

            var project = await projects.GetProjectAsync(projectId);
            await PerformServerValidationsAsync(project, model, issue);

            if (!ModelState.IsValid)
            {
                model.CanDeleteComments = User.IsInRole("Admin") ||
                    await projects.IsProjectLeadAsync(projectId, userManager.GetUserId(User));
                await GetDropdownValues(projectId, issue.Status);
                return View(model);
            }

            await issues.EditIssueAsync(model);

            this.AddNotification("Issue saved!", NotificationType.Success);
            return RedirectToAction("Edit", "Projects", new { id = projectId });
        }

        [HttpPost]
        [ActionName("AddComment")]
        public async Task<IActionResult> AddCommentAsync(
            int id, int projectId, CommentViewModel newComment)
        {
            var issue = await issues.GetIssueAsync<IssueViewModel>(id);
            if (issue == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await GetDropdownValues(projectId);
                return View("Edit", await issues.GetIssueAsync<IssueViewModel>(id));
            }

            var currentUserId = userManager.GetUserId(User);
            if (!await projects.IsProjectLeadAsync(projectId, currentUserId) &&
                !await projects.IsProjectAssigneeAsync(projectId, currentUserId))
            {
                return Unauthorized();
            }

            await comments.CreateCommentAsync(id, currentUserId, newComment.Text);

            return RedirectToAction("Edit", new { projectId, id });
        }

        [HttpPost]
        [ActionName("RemoveComment")]
        public async Task<IActionResult> RemoveCommentAsync(
            int id, int projectId, int commentId)
        {
            if (!await comments.CommentExistsAsync(projectId, id, commentId))
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                !await projects.IsProjectLeadAsync(projectId, userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            await comments.DeleteCommentAsync(commentId);
            this.AddNotification("Comment has been deleted.", NotificationType.Success);

            return RedirectToAction("Edit", "Issues", new { projectId, id });
        }

        private async Task GetDropdownValues(int projectId, IssueStatus? status = null)
        {
            var lists = await Task.WhenAll(
                users.ListUsersAsync(),
                labels.ListLabelsAsync(projectId),
                priorities.ListPrioritiesAsync(projectId));

            ViewBag.UserList = lists[0];
            ViewBag.LabelList = lists[1];
            ViewBag.PriorityList = lists[2];

            if (status.HasValue)
            {
                ViewBag.Statuses = GetAvailableStatuses(status.Value).Select(s =>
                    new SelectListItem
                    {
                        Value = ((int) s).ToString(),
                        Text = s.GetDisplayName()
                    }).ToArray();
            }
        }

        private static IEnumerable<IssueStatus> GetAvailableStatuses(IssueStatus status)
        {
            var availableStatues = new List<IssueStatus> { status };

            switch (status)
            {
                case IssueStatus.Open:
                    availableStatues.AddRange(new []
                    {
                        IssueStatus.InProgress,
                        IssueStatus.Resolved,
                        IssueStatus.Closed

                    });
                    break;
                case IssueStatus.InProgress:
                    availableStatues.AddRange(new[]
                    {
                        IssueStatus.Open,
                        IssueStatus.Resolved,
                        IssueStatus.Closed
                    });
                    break;
                case IssueStatus.Resolved:
                    availableStatues.AddRange(new[]
                    {
                        IssueStatus.Reopened,
                        IssueStatus.Closed
                    });
                    break;
                case IssueStatus.Closed:
                    availableStatues.AddRange(new[]
                    {
                        IssueStatus.Reopened
                    });
                    break;
                case IssueStatus.Reopened:
                    availableStatues.AddRange(new[]
                    {
                        IssueStatus.InProgress,
                        IssueStatus.Resolved,
                        IssueStatus.Closed
                    });
                    break;
            }

            return availableStatues;
        }

        private async Task PerformServerValidationsAsync(ProjectViewModel project,
            IssueViewModel newIssue, IssueViewModel oldIssue = null)
        {
            if (project.Issues.Any(i => i.Title == newIssue.Title && i.Id != newIssue.Id))
            {
                ModelState.AddModelError(nameof(newIssue.Title),
                    "Project already has an issue with this Title");
            }

            if (newIssue.DueDate <= DateTime.Today)
            {
                ModelState.AddModelError(nameof(newIssue.DueDate),
                    "Date must be in the futere");
            }

            if (project.Priorities.All(p => p != newIssue.Priority))
            {
                ModelState.AddModelError(nameof(newIssue.Priority),
                    "Priority is not valid for this Project");
            }

            if (!await labels.LabelsExistInProjectAsync(project.Id, newIssue.Labels))
            {
                ModelState.AddModelError(nameof(newIssue.Labels),
                    "Not all Labels are valid for this Project");
            }

            if (oldIssue != null &&
                !GetAvailableStatuses(oldIssue.Status).Contains(newIssue.Status))
            {
                ModelState.AddModelError(nameof(newIssue.Status),
                    "Status transition is not valid");
            }
        }
    }
}