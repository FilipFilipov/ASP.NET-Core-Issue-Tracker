using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Models;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services;
using IssueTracker.Services.Services.Utilities;
using IssueTracker.Web.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;


namespace IssueTracker.Web.Controllers
{
    [Route("Projects/{projectId:int}/Issues/[action]/{id?}")]
    public class IssuesController : BaseController
    {
        private readonly IIssuesService issues;
        private readonly IPrioritiesService priorities;
        private readonly ILabelsService labels;
        private readonly ICommentsService comments;

        public IssuesController(UserManager<User> userManager, IProjectsService projects,
            IIssuesService issues, IPrioritiesService priorities,
            ILabelsService labels, ICommentsService comments)
            : base(userManager, projects)
        {
            this.issues = issues;
            this.priorities = priorities;
            this.labels = labels;
            this.comments = comments;
        }

        [HttpGet("/Issues")]
        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            return View(await issues.GetIssuesAsync(withProject: true));
        }

        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(int projectId, int id)
        {
            var result1 = await issues.GetIssueForEditingAsync(id, User);
            if (result1.NotificationType == NotificationType.Success)
            {
                TempData["Model"] = JsonConvert.SerializeObject(result1.Value);
                return RedirectToAction("Edit", new { projectId, id });
            }

            var result2 = await issues.GetIssueAsync<IssueDetailsModel>(id, User);
            if (result2.NotificationType == NotificationType.Error)
            {
                this.AddNotification(result2.Message, result2.NotificationType);
                return RedirectToAction("Index", "Projects");
            }

            return View(result2.Value);
        }

        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId)
        {
            var projectResult =
                await projects.GetProjectForEditingAsync<ProjectBaseModel>(projectId, User);
            if (projectResult.NotificationType == NotificationType.Error)
            {
                this.AddNotification(projectResult.Message, projectResult.NotificationType);
                return RedirectToAction("Index", "Projects");
            }

            await GetDropdownValues(projectId);
            return View(new IssueViewModel { CanEdit = true });
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId, IssueViewModel model)
        {
            model.CanEdit = true;
            var projectResult =
                await projects.GetProjectForEditingAsync<ProjectViewModel>(projectId, User);
            if (projectResult.NotificationType == NotificationType.Error)
            {
                this.AddNotification(projectResult.Message, projectResult.NotificationType);
                return RedirectToAction("Index", "Projects");
            }

            await PerformServerValidationsAsync(projectResult.Value, model);
            if (!ModelState.IsValid)
            {
                await GetDropdownValues(projectId);
                return View(model);
            }

            var result = await issues.CreateIssueAsync(projectId, model, User);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Edit", "Projects", new { id = projectId });
        }

        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int projectId, int id)
        {
            IssueViewModel model;
            if (TempData.ContainsKey("Model"))
            {
                model = JsonConvert.DeserializeObject<IssueViewModel>(
                    TempData["Model"].ToString());
            }
            else
            {
                var result = await issues.GetIssueForEditingAsync(id, User);
                if (result.NotificationType == NotificationType.Error)
                {          
                    this.AddNotification(result.Message, result.NotificationType);
                    return RedirectToAction("Index", "Projects");
                }

                model = result.Value;
            }

            await GetDropdownValues(model.ProjectId, model.Status);
            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int projectId, IssueViewModel model)
        {           
            var projectResult = await projects.GetProjectAsync<ProjectViewModel>(projectId);
            if (projectResult.NotificationType == NotificationType.Error)
            {
                this.AddNotification(projectResult.Message, projectResult.NotificationType);
                return RedirectToAction("Index", "Projects");
            }

            var project = projectResult.Value;
            var issue = project?.Issues.SingleOrDefault(i => i.Id == model.Id);
            if (issue == null)
            {
                return BadRequest();
            }

            await PerformServerValidationsAsync(project, model, issue);

            if (!ModelState.IsValid)
            {
                await GetDropdownValues(projectId, issue.Status);
                return View(model);
            }

            var result = await issues.EditIssueAsync(model, User);

            this.AddNotification(result.Message, result.NotificationType);
            return RedirectToAction("Edit", "Projects", new { id = projectId });
        }

        [HttpPost]
        [ActionName("AddComment")]
        public async Task<IActionResult> AddCommentAsync(
            int id, int projectId, CommentViewModel newComment)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { projectId, id });
            }

            var result = await comments.CreateCommentAsync(
                projectId, id, newComment.Text, User);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Details", new { projectId, id });
        }

        [HttpPost]
        [ActionName("RemoveComment")]
        public async Task<IActionResult> RemoveCommentAsync(
            int id, int projectId, int commentId)
        {
            var result = await comments.DeleteCommentAsync(commentId, User);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Edit", "Issues", new { projectId, id });
        }

        private async Task GetDropdownValues(int projectId, IssueStatus? status = null)
        {
            var lists = await Task.WhenAll(
                ListUsersAsync(),
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
            IssueViewModel newIssue, IssueListModel oldIssue = null)
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

            if (!await labels.LabelsExistInProjectAsync(project.Id, newIssue.SelectedLabelIds))
            {
                ModelState.AddModelError(nameof(newIssue.SelectedLabelIds),
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