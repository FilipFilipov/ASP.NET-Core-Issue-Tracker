using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using IssueTracker.Data;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Services.Services.Implementations
{
    public class ProjectsService : AbstractService, IProjectsService
    {
        public ProjectsService(IssueTrackerDbContext db) : base(db)
        {
        }

        public async Task<ProjectListModel[]> GetProjectsAsync()
        {
            return await db.Projects                
                .ProjectTo<ProjectListModel>()
                .ToArrayAsync();
        }

        public async Task<ProjectViewModel> GetProjectAsync(int projectId)
        {
            return await db.Projects
                .Where(p => p.Id == projectId)
                .ProjectTo<ProjectViewModel>(p => p.Issues.Select(i => i.Assignee))
                .SingleOrDefaultAsync();
        }

        public async Task<Project> CreateProjectAsync(ProjectViewModel model)
        {
            var newProject = Mapper.Map<Project>(model);
            newProject.Key = string.Concat(model.Name.Split().Select(word => word.ToUpper()[0]));
            db.Projects.Add(newProject);

            var dbLabels = await db.Labels.Where(l => model.Labels.Contains(l.Name))
                .ToArrayAsync();
            foreach (var dbLabel in dbLabels)
            {
                dbLabel.ProjectLabels.Add(new ProjectLabel { Project = newProject });
            }

            var newLabelValues = model.Labels
                .Where(l => !dbLabels.Select(dl => dl.Name).Contains(l));
            foreach (var newLabelValue in newLabelValues)
            {
                db.Labels.Add(new Label
                {
                    Name = newLabelValue,
                    ProjectLabels = new List<ProjectLabel> {
                        new ProjectLabel { Project = newProject }                     
                    }
                });
            }
            
            await db.SaveChangesAsync();

            return newProject;
        }

        public async Task<bool> ProjectExistsAsync(int id)
        {
            return await db.Projects.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ProjectExistsAsync(string name)
        {
            return await db.Projects.AnyAsync(p => p.Name == name);
        }
    }
}
