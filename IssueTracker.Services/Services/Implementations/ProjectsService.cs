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
                new ServiceResult<T>("Project not found");
        }

        public async Task<ServiceResult<T>> GetProjectForEditingAsync<T>(
            int projectId, ClaimsPrincipal user) where T : ProjectBaseModel
        {
            var result = await GetProjectAsync<T>(projectId);

            if (result.NotificationType == NotificationType.Error)
            {
                return result;
            }

            if (!IsAdmin(user) || !IsProjectLead(result.Value.LeaderId, user))
            {
                return new ServiceResult<T>(
                    "Only admin or project lead can perform this action");
            }

            return result;
        }

        public async Task<ServiceResult<Project>> CreateProjectAsync(
            ProjectViewModel model, ClaimsPrincipal user)
        {
            var newProject = Mapper.Map<Project>(model);
            newProject.Key = string.Concat(model.Name.Split().Select(word => word.ToUpper()[0]));
            db.Projects.Add(newProject);

            newProject.ProjectLabels = await GetProjectLabelsAsync(model);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(newProject, "Project has been added");
        }

        public async Task<ServiceResult<Project>> EditProjectAsync(
            ProjectViewModel model, ClaimsPrincipal user)
        {
            var dbProject = db.Projects.SingleOrDefault(p => p.Id == model.Id);
            if (dbProject == null)
            {
                return new ServiceResult<Project>("Project not found");
            }
            if (!IsAdmin(user) && !IsProjectLead(dbProject.LeaderId, user))
            {
                return new ServiceResult<Project>(
                    "You must be an admin or project lead to do this");
            }

            Mapper.Map(model, dbProject);

            dbProject.ProjectLabels.ReplaceEntityCollection(
                await GetProjectLabelsAsync(model), pl => pl.LabelId);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(dbProject, "Project has been saved");
        }

        public async Task<ServiceResult<Project>> DeleteProjectAsync(int id, ClaimsPrincipal user)
        {
            var dbProject = db.Projects.SingleOrDefault(p => p.Id == id);
            if (dbProject == null)
            {
                return new ServiceResult<Project>("Project not found");
            }

            db.Projects.Remove(dbProject);

            await db.SaveChangesAsync();

            return new ServiceResult<Project>(dbProject, "Project has been removed");
        }

        public async Task<bool> ProjectExistsAsync(int id)
        {
            return await db.Projects.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ProjectExistsAsync(string name, int? excludingId = null)
        {
            return await db.Projects.AnyAsync(p => p.Name == name && p.Id != excludingId);
        }

        public async Task<bool> IsProjectLeadAsync(int projectId, ClaimsPrincipal user)
        {
            var project = await db.Projects.SingleOrDefaultAsync(p => p.Id == projectId);

            return IsProjectLead(project?.LeaderId, user);
        }

        public async Task<bool> IsProjectAsigneeAsync(int projectId, ClaimsPrincipal user)
        {
            var project = await db.Projects
                .Include(p => p.Issues)
                .SingleOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return false;
            }

            var userId = userManager.GetUserId(user);
            return project.Issues.Any(i => i.AssigneeId == userId);
        }
       
        private async Task<ProjectLabel[]> GetProjectLabelsAsync(ProjectViewModel model)
        {
            var dbLabels = await db.Labels.Where(l => model.Labels.Contains(l.Name))
                .ToArrayAsync();
            var dbLabelNames = new HashSet<string>(dbLabels.Select(l => l.Name));

            return dbLabels
                .Concat(model.Labels
                    .Where(str => !dbLabelNames.Contains(str))
                    .Select(str => db.Labels.Add(new Label {Name = str}).Entity)
                )
                .Select(l => new ProjectLabel { Label = l })
                .ToArray();
        }
    }
}
