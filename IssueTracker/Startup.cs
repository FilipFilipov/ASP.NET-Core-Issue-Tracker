﻿using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IssueTracker.Data;
using IssueTracker.Models;
using IssueTracker.Services;
using IssueTracker.Services.Services;
using IssueTracker.Services.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<IssueTrackerDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<IssueTrackerDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IProjectsService, ProjectsService>();
            services.AddScoped<IPrioritiesService, PrioritiesService>();
            services.AddScoped<ILabelsService, LabelsService>();
            services.AddScoped<IIssuesService, IssuesService>();

            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
