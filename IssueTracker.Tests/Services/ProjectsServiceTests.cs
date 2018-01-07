using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services.Implementations;
using IssueTracker.Services.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IssueTracker.Tests.Services
{
    [TestFixture]
    public class ProjectsServiceTests : BaseProjectTests
    {
        private static readonly Label TestLabel1 = new Label
        {
            Name = TestLabelName1
        };

        private static readonly Comment TestComment1 = new Comment
        {
            AuthorId = TestUserId1,
            Created = DateTime.Today,
            Text = TestCommentText1
        };

        private static readonly Issue TestIssue1 = new Issue
        {
            Title = TestIssueTitle1,
            Description = TestIssueDescription1,
            AssigneeId = TestUserId1,
            DueDate = DateTime.Today,
            Priority = PriorityType.Low,
            Status = IssueStatus.Open,
            Comments = new List<Comment> { TestComment1 },
            IssueLabels = new List<IssueLabel>
            {
                new IssueLabel { Label = TestLabel1 }
            }
        };

        private static readonly Issue TestIssue2 = new Issue
        {
            AssigneeId = TestUserId1
        };

        private static readonly Project TestProject1 = new Project
        {
            Name = TestProjectName1,
            Description = TestProjectDescription1,
            LeaderId = TestUserId1,
            ProjectLabels = new List<ProjectLabel>
            {
                new ProjectLabel
                {
                    Label = TestLabel1
                }
            },
            Priorities = new List<Priority>
            {
                new Priority { PriorityType = PriorityType.Low },
                new Priority { PriorityType = PriorityType.High }
            },
            Issues = new List<Issue> { TestIssue1 }
        };

        private static readonly Project TestProject2 = new Project
        {
            Name = TestProjectName2,
            Description = TestProjectDescription2,
            LeaderId = TestUserId2,
            Issues = new List<Issue> { TestIssue2 }
        };

        private static readonly Project TestProject3 = new Project
        {
            Name = TestProjectName3,
            Description = TestProjectDescription3,
            LeaderId = TestUserId2
        };

        private static readonly UserManager<User> UserManager = new UserManager<User>(
            new UserStore<User>(Db), null, null, null, null, null, null, null, null);

        [OneTimeSetUp]
        public override async Task Setup()
        {
            await base.Setup();

            await Db.Projects.AddRangeAsync(TestProject1, TestProject2, TestProject3);
            await Db.SaveChangesAsync();
            
            Mapper.Initialize(c => c.AddProfile(new AutoMapperServicesProfile()));
        }

        [Test]
        public async Task GetProjectsAsync_ShouldSucceed()
        {
            await GetProjectsAsync_ShouldSucceed(true);
        }

        [Test]
        public async Task GetProjectsAsync_ShouldSucceed_ForSingleUser()
        {
            await GetProjectsAsync_ShouldSucceed(false);
        }

        [Test]
        public async Task GetProjectAsync_ShouldSucceed()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.GetProjectAsync<ProjectDetailsModel>(1);

            result.NotificationType.Should().Be(NotificationType.Success);
            AssertProjectModelProperties(result.Value);
        }

        [Test]
        public async Task GetProjectAsync_ShouldFail_IfProjectNotFound()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.GetProjectAsync<ProjectBaseModel>(0);

            result.NotificationType.Should().Be(NotificationType.Error);
            result.Message.Should().Be(ProjectsService.Message_Error_ProjectNotFound);
        }

        [Test]
        public async Task GetProjectForEditingAsync_ShouldSucceed_ForAdmin()
        {
            await GetProjectForEditingAsync_ShouldSucceed(
                CreateUserRoleClaimPrincipal("Admin"));
        }

        [Test]
        public async Task GetProjectForEditingAsync_ShouldSucceed_ForProjectLead()
        {
            await GetProjectForEditingAsync_ShouldSucceed(
                CreateUserIdClaimsPrincipal(TestUserId1));
        }

        [Test]
        public async Task GetProjectForEditingAsync_ShouldFail_IfProjectNotFound()
        {
            await GetProjectForEditingAsync_ShouldFail(
                0, ProjectsService.Message_Error_ProjectNotFound);
        }

        [Test]
        public async Task GetProjectForEditingAsync_ShouldFail_IfUserIsNotAdminOrProjectLead()
        {
            await GetProjectForEditingAsync_ShouldFail(
                1, ProjectsService.Message_Error_UserNotAdminOrProjectLead);
        }

        [Test]
        public async Task CreateProjectAsync_ShouldSucceed()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.CreateProjectAsync(new ProjectViewModel
            {
                LeaderId = TestUserId1,
                Name = TestProjectName4,
                Description = TestProjectDescription4,
                Priorities = new[] { PriorityType.Lowest, PriorityType.Highest },
                LabelsString = "Label 1, Label 2"
            });

            result.NotificationType.Should().Be(NotificationType.Success);
            result.Message.Should().Be(ProjectsService.Message_Success_ProjectAdded);

            var expectedProject = new Project
            {
                Id = 4,
                Name = TestProjectName4,
                Description = TestProjectDescription4,
                Key = "P4",
                LeaderId = TestUserId1
            };
            var expectedPriorities = new[] { PriorityType.Lowest, PriorityType.Highest };
            var expectedLabels = new[] { TestLabelName1, TestLabelName2 };

            AssertProjectProperties(
                result.Value, expectedProject, expectedPriorities, expectedLabels);

            var project = await Db.Projects.SingleOrDefaultAsync(
                p => p.Id == result.Value.Id);

            AssertProjectProperties(
                project, expectedProject, expectedPriorities, expectedLabels);

            Db.Projects.Remove(project);
            await Db.SaveChangesAsync();
        }

        [Test]
        public async Task EditProjectAsync_ShouldSuceed_ForAdmin()
        {
            await EditProjectAsync_ShouldSuceed(CreateUserRoleClaimPrincipal("Admin"));
        }

        [Test]
        public async Task EditProjectAsync_ShouldSuceed_ForProjectLead()
        {
            await EditProjectAsync_ShouldSuceed(CreateUserIdClaimsPrincipal(TestUserId1));
        }

        [Test]
        public async Task EditProjectAsync_ShouldFail_IfProjectNotFound()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.EditProjectAsync(
                new ProjectViewModel { Id = 0 }, CreateUserRoleClaimPrincipal("Admin"));

            result.NotificationType.Should().Be(NotificationType.Error);
            result.Message.Should().Be(ProjectsService.Message_Error_ProjectNotFound);
        }

        [Test]
        public async Task EditProjectAsync_ShouldFail_UserIsNotAdminOrProjectLead()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.EditProjectAsync(
                new ProjectViewModel { Id = 1 }, new ClaimsPrincipal());

            result.NotificationType.Should().Be(NotificationType.Error);
            result.Message.Should().Be(ProjectsService.Message_Error_UserNotAdminOrProjectLead);
        }

        [Test]
        public async Task DeleteProjectAsync_ShouldSucceed()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.DeleteProjectAsync(3);

            result.NotificationType.Should().Be(NotificationType.Success);
            result.Message.Should().Be(ProjectsService.Message_Success_ProjectDeleted);

            var project = await Db.Projects.SingleOrDefaultAsync(p => p.Id == 3);

            project.Should().BeNull();

            await Db.Projects.AddAsync(TestProject3);
            await Db.SaveChangesAsync();
        }

        [Test]
        public async Task DeleteProjectAsync_ShouldFail_IfProjectNotFound()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.DeleteProjectAsync(0);

            result.NotificationType.Should().Be(NotificationType.Error);
            result.Message.Should().Be(ProjectsService.Message_Error_ProjectNotFound);
        }

        [Test]
        public async Task ProjectExistsAsync_ShouldReturnTrue()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.ProjectExistsAsync(TestProjectName1);

            result.Should().BeTrue();
        }

        [Test]
        public async Task ProjectExistsAsync_ShouldReturnFalse_ForExcludedId()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.ProjectExistsAsync(TestProjectName1, 1);

            result.Should().BeFalse();
        }

        [Test]
        public async Task ProjectExistsAsync_ShouldReturnFalse_ForNonExistentName()
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.ProjectExistsAsync("Project 0");

            result.Should().BeFalse();
        }

        private static async Task GetProjectsAsync_ShouldSucceed(bool forAll)
        {
            var service = new ProjectsService(Db, UserManager);

            var projects = await service.GetProjectsAsync(forAll ? null : TestUserId1);
            var expectedProjects = new List<ProjectListModel>
            {
                new ProjectListModel
                {
                    Id = 1,
                    Name = TestProjectName1,
                    LeaderName = TestUserName1,
                    Issues = 1
                },
                new ProjectListModel
                {
                    Id = 2,
                    Name = TestProjectName2,
                    LeaderName = TestUserName2,
                    Issues = 1
                }
            };

            if (forAll)
            {
                expectedProjects.Add(new ProjectListModel
                {
                    Id = 3,
                    Name = TestProjectName3,
                    LeaderName = TestUserName2,
                    Issues = 0
                });
            }

            projects.ShouldBeEquivalentTo(expectedProjects);
        }

        private static async Task GetProjectForEditingAsync_ShouldSucceed(
            ClaimsPrincipal claimsPrincipal)
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.GetProjectForEditingAsync<ProjectViewModel>(
                1, claimsPrincipal);

            result.NotificationType.Should().Be(NotificationType.Success);
            AssertProjectModelProperties(result.Value);
        }

        private static async Task GetProjectForEditingAsync_ShouldFail(int id, string error)
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.GetProjectForEditingAsync<ProjectViewModel>(
                id, new ClaimsPrincipal());

            result.NotificationType.Should().Be(NotificationType.Error);
            result.Message.Should().Be(error);
        }

        private static void AssertProjectModelProperties(object projectModel)
        {
            projectModel.ShouldBeEquivalentTo(new
            {
                Id = 1,
                Name = TestProjectName1,
                Description = TestProjectDescription1,
                LeaderId = TestUserId1,
                Labels = new[] { TestLabelName1 },
                Priorities = new[] { PriorityType.Low, PriorityType.High },
                Issues = new List<IssueListModel>
                {
                    new IssueListModel
                    {
                        Id = 1,
                        Title = TestIssueTitle1,
                        Project = null,
                        ProjectId = 1,
                        Assignee = TestUserName1,
                        DueDate = DateTime.Today,
                        Status = IssueStatus.Open,
                        Priority = PriorityType.Low
                    }
                }
            }, options => options.ExcludingMissingMembers());

            if (projectModel is ProjectDetailsModel model)
            {
                model.LeaderName.Should().Be(TestUserName1);
            }
        }

        private static ClaimsPrincipal CreateUserIdClaimsPrincipal(string userId)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(new ClaimsIdentityOptions().UserIdClaimType, userId)
            }));
        }

        private static ClaimsPrincipal CreateUserRoleClaimPrincipal(string role)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(new ClaimsIdentityOptions().RoleClaimType, role)
            }));
        }

        
        private static void AssertProjectProperties(Project project,
            Project expectedProject, PriorityType[] expectedPriorities, string[] expectedLabels)
        {
            project.Should().NotBeNull();
            project.ShouldBeEquivalentTo(expectedProject, opt =>
                opt.Excluding(p => p.Leader)
                .Excluding(p => p.Issues)
                .Excluding(p => p.Priorities)
                .Excluding(p => p.ProjectLabels));

            project.Priorities.Select(p => p.PriorityType).ShouldAllBeEquivalentTo(
                expectedPriorities);

            project.ProjectLabels.Select(pl => pl.Label.Name).ShouldBeEquivalentTo(
                expectedLabels
            );
        }

        private static async Task EditProjectAsync_ShouldSuceed(
            ClaimsPrincipal claimsPrincipal)
        {
            var service = new ProjectsService(Db, UserManager);

            var result = await service.EditProjectAsync(
                new ProjectViewModel
                {
                    Id = 1,
                    Name = TestProjectName4,
                    Description = TestProjectDescription1,
                    Priorities = new[] {PriorityType.Low, PriorityType.Medium},
                    LabelsString = $"{TestLabelName1},{TestLabelName2}",
                    LeaderId = TestUserId1
                },
                claimsPrincipal);

            result.NotificationType.Should().Be(NotificationType.Success);
            result.Message.Should().Be(ProjectsService.Message_Success_ProjectEdited);

            var expectedProject = new Project
            {
                Id = 1,
                Name = TestProjectName4,
                Description = TestProjectDescription1,
                LeaderId = TestUserId1
            };
            var expectedPriorities = new[] {PriorityType.Low, PriorityType.Medium};
            var expectedLabels = new[] {TestLabelName1, TestLabelName2};

            AssertProjectProperties(
                result.Value, expectedProject, expectedPriorities, expectedLabels);

            var project = await Db.Projects.SingleOrDefaultAsync(
                p => p.Id == result.Value.Id);

            AssertProjectProperties(
                project, expectedProject, expectedPriorities, expectedLabels);

            project.Name = TestProjectName1;
            project.Priorities.Remove(
                project.Priorities.Single(p => p.PriorityType == PriorityType.Medium));
            project.Priorities.Add(
                new Priority {PriorityType = PriorityType.High});
            project.ProjectLabels.Remove(
                project.ProjectLabels.Single(l => l.Label.Name == TestLabelName2));

            await Db.SaveChangesAsync();
        }
    }
}
