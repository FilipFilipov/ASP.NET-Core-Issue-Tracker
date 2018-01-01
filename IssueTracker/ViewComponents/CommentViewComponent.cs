using System.Collections.Generic;
using IssueTracker.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Web.ViewComponents
{
    public class CommentViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<CommentViewModel> comments,
            bool canComment, bool canDeleteComments = false)
        {
            ViewBag.CanComment = canComment;
            ViewBag.CanDeleteComments = canDeleteComments;

            return View(comments);
        }
    }
}
