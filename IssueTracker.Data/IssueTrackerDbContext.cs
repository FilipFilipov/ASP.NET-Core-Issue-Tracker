using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public DbSet<ProjectLabels> ProjectLabels { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<IssueLabels> IssueLabels { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<ProjectLabels>()
                .HasKey(pl => new { pl.ProjectId, pl.LabelId });

            builder.Entity<ProjectLabels>()
                .HasOne(pl => pl.Project)
                .WithMany(p => p.ProjectLabels)
                .HasForeignKey(pl => pl.ProjectId);

            builder.Entity<ProjectLabels>()
                .HasOne(pl => pl.Label)
                .WithMany(l => l.ProjectLabels)
                .HasForeignKey(pl => pl.LabelId);

            builder.Entity<IssueLabels>()
                .HasKey(il => new { il.IssueId, il.LabelId });

            builder.Entity<IssueLabels>()
                .HasOne(il => il.Issue)
                .WithMany(i => i.IssueLabels)
                .HasForeignKey(il => il.IssueId);

            builder.Entity<IssueLabels>()
                .HasOne(il => il.Label)
                .WithMany(l => l.IssueLabels)
                .HasForeignKey(il => il.LabelId);
        }
    }
}
