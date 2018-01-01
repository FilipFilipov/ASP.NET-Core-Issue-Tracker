using System.Security.Claims;
using System.Security.Principal;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Identity;

namespace IssueTracker.Services.Services
{
    public abstract class AbstractService
    {
        protected IssueTrackerDbContext db;
        protected UserManager<User> userManager;

        protected AbstractService(IssueTrackerDbContext db, UserManager<User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }

        protected bool IsAdmin(ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }

        protected bool IsProjectLead(string projectLeaderId, ClaimsPrincipal user)
        {
            return projectLeaderId == userManager.GetUserId(user);
        }
    }
}
