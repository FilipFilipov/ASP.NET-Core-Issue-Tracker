using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;
using IssueTracker.Services.Services.Utilities;

namespace IssueTracker.Services.Services
{
    public interface ICommentsService
    {
        Task<ICollection<CommentViewModel>> GetIssueCommentsAsync(int issueId);

        Task<ServiceResult<Comment>> CreateCommentAsync(
            int projectId, int issueId, string text, ClaimsPrincipal user);

        Task<ServiceResult<Comment>> DeleteCommentAsync(int id, ClaimsPrincipal user);
    }
}
