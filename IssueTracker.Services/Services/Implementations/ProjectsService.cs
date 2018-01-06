using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Models;
using IssueTracker.Services.Extensions;
using IssueTracker.Services.Models.Project;
using IssueTracker.Services.Services.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class ProjectsService : AbstractService, IProjectsService
    {
        public const string Message_Success_ProjectAdded = "Project has been added";
        public const string Message_Success_ProjectEdited = "Project has been saved";
        public const string Message_Success_ProjectDeleted = "Project has been removed";
        public const string Message_Error_ProjectNotFound = "Project not found";
        public const string Message_Error_UserNotAdminOrProjectLead =
            "Only an admin or project lead can perform this action";

        public ProjectsService(IssueTrackerDbContext db, UserManager<User> userManager) :
            base(db, userManager)
        {
        }

        public async Task<ProjectListModel[]> GetProjectsAsync(string userId = null)
        {
            IQueryable<Project> projects = db.Projects;
            if (userId != null)
            {
                projects = projects.Where(p =>
                    p.LeaderId == userId ||
                    p.Issues.Any(i => i.AssigneeId == userId));
            }

            return await projects
                .ProjectTo<ProjectListModel>()
                .ToArrayAsync();
        }

        public async Task<ServiceResult<T>> GetProjectAsync<T>(int projectId)
            where T : ProjectBaseModel
        {
            var project = await db.Projects
                .Where(p => p.Id == projectId)
                .ProjectTo<T>()
                .SingleOrDefaultAsync();

            return project != null ?
                new ServiceResult<T>(project) :
                new ServiceResult<T>(Message_Error_ProjectNotFound);
        }

        public async Task<ServiceResult<T>> GetProjectForEditingAsync<T>(
            int projectId, ClaimsPrincipal user) where T : ProjectBaseModel
        {
            var result = await GetProjectAsync<T>(projectId);

            if (result.NotificationType == NotificationType.Error)
            {
                return result;
            }

            if (!IsAdmin(user) && !IsProjectLead(result.Value.LeaderId, user))
            {
                return new ServiceResult<T>(Message_Error_UserNotAdminOrProjectLead);
            }

            return result;
        }

        public async Task<ServiceResult<Project>> CreateProjectAsync(ProjectViewModel model)
        {
            var newProject = Mapper.Map<Project>(model);
            newProject.Key = string.Concat(model.Name.Split().Select(word => word.ToUpper()[0]));
            newProject.ProjectLabels = await GetProjectLabelsAsync(model);
            newProject.Priorities = model.Priorities
                .Select(p => new Priority { PriorityType = p }).ToArray();

            db.Projects.Add(newProject);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(newProject, Message_Success_ProjectAdded);
        }

        public async Task<ServiceResult<Project>> EditProjectAsync(
            ProjectViewModel model, ClaimsPrincipal user)
        {
            var dbProject = db.Projects
                .Include(p => p.Priorities)
                .Include(p => p.ProjectLabels)
                .SingleOrDefault(p => p.Id == model.Id);
            if (dbProject == null)
            {
                return new ServiceResult<Project>(Message_Error_ProjectNotFound);
            }
            if (!IsAdmin(user) && !IsProjectLead(dbProject.LeaderId, user))
            {
                return new ServiceResult<Project>(Message_Error_UserNotAdminOrProjectLead);
            }

            Mapper.Map(model, dbProject);

            dbProject.Priorities.ReplaceEntityCollection(
                await GetProjectPrioritiesAsync(model), p => p.PriorityType);
            dbProject.ProjectLabels.ReplaceEntityCollection(
                await GetProjectLabelsAsync(model), pl => pl.LabelId);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(dbProject, Message_Success_ProjectEdited);
        }

        public async Task<ServiceResult<Project>> DeleteProjectAsync(int id)
        {
            var dbProject = db.Projects.SingleOrDefault(p => p.Id == id);
            if (dbProject == null)
            {
                return new ServiceResult<Project>(Message_Error_ProjectNotFound);
            }

            db.Projects.Remove(dbProject);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(dbProject, Message_Success_ProjectDeleted);
        }

        public async Task<bool> ProjectExistsAsync(string name, int? excludingId = null)
        {
            return await db.Projects.AnyAsync(p => p.Name == name && p.Id != excludingId);
        }

        private async Task<ProjectLabel[]> GetProjectLabelsAsync(ProjectViewModel model)
        {
            var dbLabels = await db.Labels.Where(l => model.Labels.Contains(l.Name))
                .ToArrayAsync();
            var dbLabelNames = new HashSet<string>(dbLabels.Select(l => l.Name));

            return dbLabels
                .Concat(model.Labels
                    .Where(str => !dbLabelNames.Contains(str))
                    .Select(str => db.Labels.Add(new Label { Name = str }).Entity)
                )
                .Select(l => new ProjectLabel { LabelId = l.Id })
                .ToArray();
        }

        private async Task<Priority[]> GetProjectPrioritiesAsync(ProjectViewModel model)
        {
            var dbPriorities = await db.Priorities
                .Where(p => model.Priorities.Contains(p.PriorityType) && model.Id == p.ProjectId)
                .ToArrayAsync();
            var dbPriorityTypes = new HashSet<PriorityType>(dbPriorities.Select(p => p.PriorityType));

            return dbPriorities
                .Concat(model.Priorities
                    .Where(type => !dbPriorityTypes.Contains(type))
                    .Select(type => db.Priorities.Add(new Priority { PriorityType = type }).Entity)
                )
                .ToArray();
        }
    }
}
