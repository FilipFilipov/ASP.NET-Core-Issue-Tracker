using System;
using System.Collections.Generic;
using System.Text;
using IssueTracker.Data;

namespace IssueTracker.Services
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
