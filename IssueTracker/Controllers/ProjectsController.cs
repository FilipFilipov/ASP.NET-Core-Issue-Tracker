using System.Threading.Tasks;
using IssueTracker.Controllers;
using IssueTracker.Services;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services;
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

        public IActionResult Index()
        {
            return View();
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

            await projects.CreateProjectAsync(model);

            return RedirectToAction(nameof(Index), "Home");
        }

        private async Task GetDropdownValues()
        {
            ViewBag.UserList = await users.ListUsersAsync();
        }
    }
}