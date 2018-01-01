using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Models;
using IssueTracker.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Web.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected readonly UserManager<User> userManager;
        protected readonly IProjectsService projects;

        protected BaseController(UserManager<User> userManager, IProjectsService projects)
        {
            this.userManager = userManager;
            this.projects = projects;
        }

        protected bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        protected bool IsProjectLead(string leaderId)
        {
            return leaderId != userManager.GetUserId(User);
        }

        protected bool IsIssueAsignee(string assigneeId)
        {
            return assigneeId != userManager.GetUserId(User);
        }

        protected async Task<SelectListItem[]> ListUsersAsync()
        {
            return await userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id })
                .ToArrayAsync();
        }
    }
}
