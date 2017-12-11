using System.Linq;
using System.Threading.Tasks;
using IssueTracker.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Implementations
{
    public class UsersService : AbstractService, IUsersService
    {
        public UsersService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<SelectListItem[]> ListUsersAsync()
        {
            return await db.Users
                .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id })
                .ToArrayAsync();
        }
    }
}
