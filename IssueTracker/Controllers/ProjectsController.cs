using System.Threading.Tasks;
using IssueTracker.Models;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services;
using IssueTracker.Services.Services.Utilities;
using IssueTracker.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace IssueTracker.Web.Controllers
{
    public class ProjectsController : BaseController
    {
        public ProjectsController(UserManager<User> userManager, IProjectsService projects) :
            base(userManager, projects)
        {
        }

        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {
            return View(await projects.GetProjectsAsync());
        }

        [ActionName("Details")]
        public async Task<IActionResult> DetailsAsync(int id)
        {
            var result1 = await projects.GetProjectForEditingAsync<ProjectViewModel>(id, User);
            if (result1.NotificationType == NotificationType.Success)
            {
                TempData["Model"] = JsonConvert.SerializeObject(result1.Value);
                return RedirectToAction("Edit", new { id });
            }

            var result2 = await projects.GetProjectAsync<ProjectDetailsModel>(id);
            if (result2.NotificationType == NotificationType.Error)
            {          
                this.AddNotification(result2.Message, result2.NotificationType);
                return RedirectToAction("Index");
            }

            return View(result2.Value);
        }

        [ActionName("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync()
        {
            await GetDropdownValues();

            return View(new ProjectViewModel());
        }

        [HttpPost]
        [ActionName("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync(ProjectViewModel model)
        {
            if (await projects.ProjectExistsAsync(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is taken");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await projects.CreateProjectAsync(model);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Index");
        }

        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(int id)
        {
            ProjectViewModel model;
            if (TempData.ContainsKey("Model"))
            {
                model = JsonConvert.DeserializeObject<ProjectViewModel>(
                    TempData["Model"].ToString());
            }
            else
            {
                var result = await projects.GetProjectForEditingAsync<ProjectViewModel>(id, User);
                if (result.NotificationType == NotificationType.Error)
                {          
                    this.AddNotification(result.Message, result.NotificationType);
                    return RedirectToAction("Index");
                }

                model = result.Value;
            }           

            await GetDropdownValues();

            return View(model);
        }

        [HttpPost]
        [ActionName("Edit")]
        public async Task<IActionResult> EditAsync(ProjectViewModel model)
        {
            if (await projects.ProjectExistsAsync(model.Name, model.Id))
            {
                ModelState.AddModelError(nameof(model.Name), "Name is taken");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await projects.EditProjectAsync(model, User);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await projects.DeleteProjectAsync(id);
            this.AddNotification(result.Message, result.NotificationType);

            return RedirectToAction("Index");
        }

        private async Task GetDropdownValues()
        {
            ViewBag.UserList = await ListUsersAsync();
        }       
    }
}