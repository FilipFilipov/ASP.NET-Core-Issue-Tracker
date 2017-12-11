using System.Threading.Tasks;
using IssueTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Web.Controllers
{
    [Route("api/Labels")]
    public class LabelsController : Controller
    {
        private readonly ILabelsService labels;

        public LabelsController(ILabelsService labels)
        {
            this.labels = labels;
        }

        [HttpGet]
        public async Task<IActionResult> GetLabels(string search, int? projectId = null)
        {
            return Ok(await labels.GetLabelsAsync(search, projectId));
        }
    }
}