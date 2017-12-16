using System.Threading.Tasks;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services;
using IssueTracker.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Web.Controllers
{
    [Authorize]
    [Route("Projects/{projectId:int}/Issues/[action]/{id:int?}")]
    public class IssuesController : Controller
    {
        private readonly IProjectsService projects;
        private readonly IIssuesService issues;
        private readonly IUsersService users;
        private readonly IPrioritiesService priorities;
        private readonly ILabelsService labels;

        public IssuesController(IProjectsService projects, IIssuesService issues,
            IUsersService users, IPrioritiesService priorities, ILabelsService labels)
        {
            this.projects = projects;
            this.issues = issues;
            this.users = users;
            this.priorities = priorities;
            this.labels = labels;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId)
        {
            if (!await projects.ProjectExistsAsync(projectId))
            {
                return BadRequest();
            }

            await GetDropdownValues(projectId);
            return View(new IssueViewModel());
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(int projectId, IssueViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await GetDropdownValues(projectId);
                return View(model);
            }

            if (!await projects.ProjectExistsAsync(projectId))
            {
                return BadRequest();
            }

            if (await issues.IssueExistsAsync(projectId, model.Title))
            {
                ModelState.AddModelError(nameof(model.Title),
                    "Project already has an issue with this Title");

                await GetDropdownValues(projectId);
                return View(model);
            }

            await issues.CreateIssueAsync(projectId, model);
            this.AddNotification("Issue added!", NotificationType.Success);

            return RedirectToAction("Edit", "Projects", new { id = projectId });
        }

        private async Task GetDropdownValues(int projectId)
        {
            var lists = await Task.WhenAll(
                users.ListUsersAsync(),
                labels.ListLabelsAsync(projectId),
                priorities.ListPrioritiesAsync(projectId));

            ViewBag.UserList = lists[0];
            ViewBag.LabelList = lists[1];
            ViewBag.PriorityList = lists[2];
        }
    }
}