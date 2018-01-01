using System.Diagnostics;
using System.Threading.Tasks;
using IssueTracker.Models;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services;
using IssueTracker.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IIssuesService issues;

        public HomeController(
            UserManager<User> userManager, IProjectsService projects, IIssuesService issues) :
            base(userManager, projects)
        {
            this.issues = issues;
        }

        [ActionName("Index")]
        public async Task<IActionResult> IndexAsync()
        {          
            var userId = userManager.GetUserId(User);
            var projectsTask = projects.GetProjectsAsync(userId);
            var issuesTask = issues.GetIssuesAsync(userId, true);
            await Task.WhenAll(projectsTask, issuesTask);

            return View(new DashboardModel
            {
                Projects = projectsTask.Result,
                Issues = issuesTask.Result
            });
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
