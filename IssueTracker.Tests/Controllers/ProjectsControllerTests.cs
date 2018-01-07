using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services;
using IssueTracker.Services.Services.Implementations;
using IssueTracker.Services.Services.Utilities;
using IssueTracker.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IssueTracker.Tests.Controllers
{
    [TestFixture]
    public class ProjectsControllerTests : BaseProjectTests
    {
        private static readonly Type controllerType = typeof(ProjectsController);

        private readonly Mock<UserManager<User>> usersMock;
        private readonly Mock<IProjectsService> projectsMock;
        private readonly ProjectsController controller;

        public ProjectsControllerTests()
        {
            usersMock = new Mock<UserManager<User>>(
                new UserStore<User>(Db), null, null, null, null, null, null, null, null)
            {
                CallBase = true
            };

            projectsMock = new Mock<IProjectsService>();

            projectsMock.Setup(p => p.GetProjectsAsync(It.IsAny<string>()))
                .ReturnsAsync(new ProjectListModel[0]);

            projectsMock.Setup(p => p.GetProjectAsync<ProjectDetailsModel>(It.IsAny<int>()))
                .ReturnsAsync((int id) => id == 0 ?
                    new ServiceResult<ProjectDetailsModel>(new ProjectDetailsModel { Id = id }) :
                    new ServiceResult<ProjectDetailsModel>(ProjectsService.Message_Error_ProjectNotFound));

            projectsMock.Setup(p => p.GetProjectForEditingAsync<ProjectViewModel>(
                It.IsAny<int>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((int id, ClaimsPrincipal claimsPrincipal) => id == 1 ?
                    new ServiceResult<ProjectViewModel>(new ProjectViewModel { Id = id}) :
                    new ServiceResult<ProjectViewModel>(ProjectsService.Message_Error_ProjectNotFound));

            projectsMock.Setup(p => p.CreateProjectAsync(It.IsAny<ProjectViewModel>()))
                .ReturnsAsync(new ServiceResult<Project>(null,
                    ProjectsService.Message_Success_ProjectAdded));

            projectsMock.Setup(p => p.EditProjectAsync(
                It.IsAny<ProjectViewModel>(), It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new ServiceResult<Project>(null,
                    ProjectsService.Message_Success_ProjectEdited));

            projectsMock.Setup(p => p.DeleteProjectAsync(It.IsAny<int>()))
                .ReturnsAsync(new ServiceResult<Project>(null,
                    ProjectsService.Message_Success_ProjectDeleted));

            projectsMock.Setup(p => p.ProjectExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync((string name, int? id) => name == TestProjectName1);

            controller = new ProjectsController(usersMock.Object, projectsMock.Object)
            {
                TempData = new TempDataDictionary(
                    new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        [TearDown]
        public void Reset()
        {
            usersMock.ResetCalls();
            projectsMock.ResetCalls();
            controller.ModelState.Clear();
            controller.TempData.Clear();
        }

        [Test]
        public void ProjectController_ShouldBeAuthorized()
        {
            typeof(ProjectsController)
                .GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
        }

        [Test]
        public void ProjectController_ShouldHaveIndexAction()
        {
            ControllerShouldHaveAction(
                nameof(ProjectsController.IndexAsync), "Index");
        }

        [Test]
        public async Task ProjectController_IndexAction_ShouldCallServiceAndReturnView()
        {
            var result = await controller.IndexAsync();

            projectsMock.Verify(p => p.GetProjectsAsync(null), Times.Once);

            AssertViewWithModel(result, new ProjectListModel[0]);
        }

        [Test]
        public void ProjectController_ShouldHaveDetailsAction()
        {
            ControllerShouldHaveAction(
                nameof(ProjectsController.DetailsAsync), "Details");
        }

        [Test]
        public async Task ProjectController_DetailsActionGet_ShouldReturnView()
        {
            var result = await controller.DetailsAsync(0);

            projectsMock.Verify(
                p => p.GetProjectForEditingAsync<ProjectViewModel>(0, null), Times.Once);
            projectsMock.Verify(
                p => p.GetProjectAsync<ProjectDetailsModel>(0), Times.Once);

            AssertViewWithModel(result, new ProjectDetailsModel { Id = 0 });
        }

        [Test]
        public async Task ProjectController_DetailsActionsGet_ShouldRedirectIfItCantGetProject()
        {
            var result = await controller.DetailsAsync(-1);

            projectsMock.Verify(
                p => p.GetProjectForEditingAsync<ProjectViewModel>(-1, null), Times.Once);
            projectsMock.Verify(
                p => p.GetProjectAsync<ProjectDetailsModel>(-1), Times.Once);

            AssertRedirectToAction(result);
            AssertNotificationMessage(ProjectsService.Message_Error_ProjectNotFound, true);
        }

        [Test]
        public async Task ProjectController_DetailsActionsGet_ShouldRedirectToEditIfAllowed()
        {
            var result = await controller.DetailsAsync(1);

            projectsMock.Verify(
                p => p.GetProjectForEditingAsync<ProjectViewModel>(1, null), Times.Once);
            projectsMock.Verify(
                p => p.GetProjectAsync<ProjectDetailsModel>(It.IsAny<int>()), Times.Never);

            AssertRedirectToAction(
                result, "Edit", null, new RouteValueDictionary { { "id", 1 } });

            controller.TempData.Keys.Should().Contain("Model");
            controller.TempData["Model"].Should().Be(
                JsonConvert.SerializeObject(new ProjectViewModel { Id = 1 }));
        }

        [Test]
        public void ProjectController_ShouldHaveCreateActions()
        {
            // Get action
            ControllerShouldHaveAction(
                nameof(ProjectsController.CreateAsync), "Create", false, "Admin");

            // Post action
            ControllerShouldHaveAction(
                nameof(ProjectsController.CreateAsync), "Create", true, "Admin");
        }

        [Test]
        public async Task ProjectController_CreateActionGet_ShouldReturnView()
        {
            var result = await controller.CreateAsync();


            AssertViewWithModel(result, new ProjectViewModel());
            AssertViewBagIsFilled();
        }

        [Test]
        public async Task ProjectController_CreateActionPost_ShouldPassModelToServiceAndRedirect()
        {
            var model = new ProjectViewModel { Name = TestProjectName2 };
            var result = await controller.CreateAsync(model);

            VerifyNameCheckCalled(model.Name);
            projectsMock.Verify(p => p.CreateProjectAsync(model), Times.Once);

            AssertRedirectToAction(result);
            AssertNotificationMessage(ProjectsService.Message_Success_ProjectAdded);
        }

        [Test]
        public async Task ProjectController_CreateActionPost_ShouldFailWithInvalidModel()
        {
            var model = new ProjectViewModel();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.CreateAsync(model);

            AssertViewWithModel(result, model);
            AssertViewBagIsFilled();
        }

        [Test]
        public async Task ProjectController_CreateActionPost_ShouldFailWithTakenName()
        {
            var model = new ProjectViewModel { Name = TestProjectName1 };
            const string errorKey = nameof(ProjectViewModel.Name);
            const string errorValue = ProjectsController.NameTakenErrorMessage;

            var result = await controller.CreateAsync(model);

            VerifyNameCheckCalled(TestProjectName1);

            AssertModelStateError(errorKey, errorValue);
            AssertViewWithModel(result, model);
            AssertViewBagIsFilled();
        }

        [Test]
        public void ProjectController_ShouldHaveEditActions()
        {
            // Get action
            ControllerShouldHaveAction(
                nameof(ProjectsController.EditAsync), "Edit");

            // Post action
            ControllerShouldHaveAction(
                nameof(ProjectsController.EditAsync), "Edit", true);
        }

        [Test]
        public async Task ProjectController_EditActionsGet_ShouldReturnView()
        {
            await ProjectController_EditActionsGet_ShouldReturnView(false);
        }

        [Test]
        public async Task ProjectController_EditActionsGet_ShouldFetchModelFromTempData()
        {
            controller.TempData["Model"] =
                JsonConvert.SerializeObject(new ProjectViewModel { Id = 1 });

            await ProjectController_EditActionsGet_ShouldReturnView(true);
        }

        [Test]
        public async Task ProjectController_EditActionsGet_ShouldRedirectIfItCantGetProject()
        {
            var result = await controller.EditAsync(0);

            projectsMock.Verify(
                p => p.GetProjectForEditingAsync<ProjectViewModel>(0, null), Times.Once);

            AssertRedirectToAction(result);
            AssertNotificationMessage(ProjectsService.Message_Error_ProjectNotFound, true);
        }

        [Test]
        public async Task ProjectController_EditActionPost_ShouldPassModelToServiceAndRedirect()
        {
            var model = new ProjectViewModel
            {
                Id = 1,
                Name = TestProjectName2
            };

            var result = await controller.EditAsync(model);

            VerifyNameCheckCalled(model.Name, model.Id);
            projectsMock.Verify(p => p.EditProjectAsync(model, null), Times.Once);

            AssertRedirectToAction(result);
            AssertNotificationMessage(ProjectsService.Message_Success_ProjectEdited);
        }

        [Test]
        public async Task ProjectController_EditActionPost_ShouldFailWithInvalidModel()
        {
            var model = new ProjectViewModel();
            controller.ModelState.AddModelError("", "Error");

            var result = await controller.EditAsync(model);

            AssertViewWithModel(result, model);
            AssertViewBagIsFilled();
        }

        [Test]
        public async Task ProjectController_EditActionPost_ShouldFailWithTakenName()
        {
            var model = new ProjectViewModel
            {
                Id = 1,
                Name = TestProjectName1
            };
            const string errorKey = nameof(ProjectViewModel.Name);
            const string errorValue = ProjectsController.NameTakenErrorMessage;

            var result = await controller.EditAsync(model);

            VerifyNameCheckCalled(model.Name, model.Id);

            AssertModelStateError(errorKey, errorValue);
            AssertViewWithModel(result, model);
            AssertViewBagIsFilled();
        }

        [Test]
        public void ProjectController_ShouldHaveDeleteAction()
        {
            ControllerShouldHaveAction(
                nameof(ProjectsController.DeleteAsync), "Delete", true, "Admin");
        }

        [Test]
        public async Task ProjectController_DeleteAction_ShouldCallServiceAndRedirect()
        {
            var result = await controller.DeleteAsync(1);

            projectsMock.Verify(p => p.DeleteProjectAsync(1), Times.Once);

            AssertRedirectToAction(result);
            AssertNotificationMessage(ProjectsService.Message_Success_ProjectDeleted);
        }

        [Test]
        public void ProjectController_ShouldHaveIsNameAvailableAction()
        {
            ControllerShouldHaveAction(
                nameof(ProjectsController.IsNameAvailableAsync), "IsNameAvailable");
        }

        [Test]
        public async Task ProjectController_IsNameAvailableAction_ShouldReturnTrue()
        {
            var result = await controller.IsNameAvailableAsync(TestProjectName2, null);

            VerifyNameCheckCalled(TestProjectName2);

            AssertJsonResult(result, true);
        }

        [Test]
        public async Task ProjectController_IsNameAvailableAction_ShouldReturnError()
        {
            var result = await controller.IsNameAvailableAsync(TestProjectName1, null);

            VerifyNameCheckCalled(TestProjectName1);

            AssertJsonResult(result, ProjectsController.NameTakenErrorMessage);
        }

        private static void ControllerShouldHaveAction(string methodName,
            string actionName, bool isPost = false, params string[] roles)
        {
            var actionMethod = controllerType.GetMethods().SingleOrDefault(mi =>
                mi.Name == methodName &&
                (isPost ?
                    mi.GetCustomAttribute<HttpPostAttribute>() != null :
                    mi.GetCustomAttribute<HttpPostAttribute>() == null
                ));

            actionMethod.Should().NotBeNull();

            var actionNameAttribute = actionMethod.GetCustomAttribute<ActionNameAttribute>();

            actionNameAttribute.Should().NotBeNull();
            actionNameAttribute.Name.Should().Be(actionName);

            if (roles.Any())
            {
                actionMethod.GetCustomAttributes<AuthorizeAttribute>()
                    .SingleOrDefault(a => a.Roles == string.Join(", ", roles))
                    .Should().NotBeNull();
            }
        }

        private void AssertViewBagIsFilled()
        {
            usersMock.Verify(um => um.Users, Times.Once);

            var users = controller.ViewBag.UserList as SelectListItem[];
            users.ShouldBeEquivalentTo(new[]
            {
                new SelectListItem
                {
                    Value = TestUserId1,
                    Text = TestUserName1
                },
                new SelectListItem
                {
                    Value = TestUserId2,
                    Text = TestUserName2
                }
            }, options => options.WithStrictOrdering());
        }

        private static void AssertViewWithModel(IActionResult result, object model)
        {
            result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType(model.GetType());
            result.As<ViewResult>().Model.ShouldBeEquivalentTo(model);
        }

        private static void AssertRedirectToAction(IActionResult result,
            string redirectAction = "Index",
            string redirectController = null,
            RouteValueDictionary redirectRouteValues = null)
        {
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ShouldBeEquivalentTo(new
                {
                    ActionName = redirectAction,
                    ControllerName = redirectController,
                    RouteValues = redirectRouteValues
                }, options => options.ExcludingMissingMembers());
        }

        private static void AssertJsonResult(IActionResult result, object value)
        {
            result.Should().BeOfType<JsonResult>()
                .Which.Value.Should().Be(value);
        }

        private void AssertNotificationMessage(string message, bool isError = false)
        {
            controller.TempData.Should()
                .ContainKey($"App.Notifications.{(isError ? "Error" : "Success")}")
                .WhichValue.Should().Be(
                    JsonConvert.SerializeObject(new HashSet<string> { message }));
        }

        private void AssertModelStateError(string key, string value)
        {
            controller.ModelState.Keys.Should().Contain(key);
            controller.ModelState[key].Errors.Should().Contain(e => e.ErrorMessage == value);
        }

        private void VerifyNameCheckCalled(string projectName, int? projectId = null)
        {
            projectsMock.Verify(
                p => p.ProjectExistsAsync(projectName, projectId), Times.Once);
        }

        private async Task ProjectController_EditActionsGet_ShouldReturnView(
            bool modelFromTempData)
        {
            var result = await controller.EditAsync(1);

            projectsMock.Verify(
                p => p.GetProjectForEditingAsync<ProjectViewModel>(1, null),
                modelFromTempData ? Times.Never() : Times.Once());

            AssertViewWithModel(result, new ProjectViewModel { Id = 1 });
            AssertViewBagIsFilled();
        }
    }
}

