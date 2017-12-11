using IssueTracker.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IssueTracker.Models;

namespace IssueTracker.Data
{
    public class IssueTrackerDbContext : IdentityDbContext<User>
    {
        public IssueTrackerDbContext(DbContextOptions<IssueTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<ProjectLabel> ProjectLabels { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueLabel> IssueLabels { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Priority>()
                .HasKey(p => new { p.ProjectId, p.PriorityType });

            builder.Entity<ProjectLabel>()
                .HasKey(pl => new { pl.ProjectId, pl.LabelId });

            builder.Entity<ProjectLabel>()
                .HasOne(pl => pl.Project)
                .WithMany(p => p.ProjectLabels)
                .HasForeignKey(pl => pl.ProjectId);

            builder.Entity<ProjectLabel>()
                .HasOne(pl => pl.Label)
                .WithMany(l => l.ProjectLabels)
                .HasForeignKey(pl => pl.LabelId);

            builder.Entity<IssueLabel>()
                .HasKey(il => new { il.IssueId, il.LabelId });

            builder.Entity<IssueLabel>()
                .HasOne(il => il.Issue)
                .WithMany(i => i.IssueLabels)
                .HasForeignKey(il => il.IssueId);

            builder.Entity<IssueLabel>()
                .HasOne(il => il.Label)
                .WithMany(l => l.IssueLabels)
                .HasForeignKey(il => il.LabelId);
        }
    }
}
