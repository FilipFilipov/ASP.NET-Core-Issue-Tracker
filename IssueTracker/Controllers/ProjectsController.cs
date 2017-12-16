using System.Threading.Tasks;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services;
using IssueTracker.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Web.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectsService projects;
        private readonly IUsersService users;

        public ProjectsController(IProjectsService projects, IUsersService users)
        {
            this.projects = projects;
            this.users = users;
        }

        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            return View(await projects.GetProjectsAsync());
        }

        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync()
        {
            await GetDropdownValues();

            return View(new ProjectViewModel());
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateAsync(ProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await projects.ProjectExistsAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is taken");
                return View(model);
            }

            await projects.CreateProjectAsync(model);

            this.AddNotification("Project created!", NotificationType.Success);

            return RedirectToAction("Index");
        }

        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var model = await projects.GetProjectAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            await GetDropdownValues();

            return View(model);
        }

        private async Task GetDropdownValues()
        {
            ViewBag.UserList = await users.ListUsersAsync();
        }
    }
}