using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Services.Services
{
    public interface IUsersService
    {
        Task<SelectListItem[]> ListUsersAsync();
    }
}
