using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IssueTracker.Services
{
    public interface IUsersService
    {
        Task<SelectListItem[]> ListUsersAsync();
    }
}
