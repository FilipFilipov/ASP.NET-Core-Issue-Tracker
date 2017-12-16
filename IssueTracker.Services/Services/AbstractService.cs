using IssueTracker.Data;

namespace IssueTracker.Services.Services
{
    public abstract class AbstractService
    {
        protected IssueTrackerDbContext db;

        protected AbstractService(IssueTrackerDbContext db)
        {
            this.db = db;
        }
    }
}
