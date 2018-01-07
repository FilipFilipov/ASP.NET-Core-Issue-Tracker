using System;
using System.Threading.Tasks;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IssueTracker.Tests
{
    public abstract class BaseProjectTests
    {
        protected const string TestUserName1 = "User 1";
        protected const string TestUserName2 = "User 2";

        protected const string TestProjectName1 = "Project 1";
        protected const string TestProjectName2 = "Project 2";
        protected const string TestProjectName3 = "Project 3";
        protected const string TestProjectName4 = "Project 4";

        protected const string TestProjectDescription1 = "Project Description 1";
        protected const string TestProjectDescription2 = "Project Description 2";
        protected const string TestProjectDescription3 = "Project Description 3";
        protected const string TestProjectDescription4 = "Project Description 4";

        protected const string TestLabelName1 = "Label 1";
        protected const string TestLabelName2 = "Label 2";

        protected const string TestIssueTitle1 = "Issue 1";
        protected const string TestIssueDescription1 = "Issue Description 1";
        protected const string TestCommentText1 = "Comment 1";

        protected static readonly string TestUserId1 = Guid.NewGuid().ToString();
        protected static readonly string TestUserId2 = Guid.NewGuid().ToString();

        protected readonly User TestUser1 = new User
        {
            Id = TestUserId1,
            UserName = TestUserName1
        };

        protected readonly User TestUser2 = new User
        {
            Id = TestUserId2,
            UserName = TestUserName2
        };

        protected static readonly IssueTrackerDbContext Db = new IssueTrackerDbContext(
            new DbContextOptionsBuilder<IssueTrackerDbContext>()
                .UseInMemoryDatabase("IssueTracker")
                .EnableSensitiveDataLogging()
                .Options);

        [OneTimeSetUp]
        public virtual async Task Setup()
        {
            await Db.Users.AddRangeAsync(TestUser1, TestUser2);
            await Db.SaveChangesAsync();
        }

        [OneTimeTearDown]
        public virtual async Task Cleanup()
        {
            Db.Users.RemoveRange(TestUser1, TestUser2);
            await Db.SaveChangesAsync();
        }
    }
}
