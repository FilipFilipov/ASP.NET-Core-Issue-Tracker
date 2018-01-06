using System.Linq;
using AutoMapper;
using IssueTracker.Data.Models;
using IssueTracker.Services.Extensions;
using IssueTracker.Services.Models;
using IssueTracker.Services.Models.Issue;
using IssueTracker.Services.Models.Project;

namespace IssueTracker.Services
{
    public class AutoMapperServicesProfile : Profile
    {
        public AutoMapperServicesProfile()
        {
            CreateMap<Project, ProjectBaseModel>();

            CreateMap<Project, ProjectDetailsModel>()
                .ForMember(dest => dest.Priorities,
                    opt => opt.MapFrom(src => src.Priorities.Select(p => p.PriorityType)))
                .ForMember(dest => dest. Labels,
                    opt => opt.MapFrom(src => src.ProjectLabels.Select(pl => pl.Label.Name)))
                .ForMember(dest => dest.LeaderName,
                    opt => opt.MapFrom(src => src.Leader.UserName));

            CreateMap<Project, ProjectViewModel>()
                .ForMember(dest => dest.Priorities,
                    opt => opt.MapFrom(src => src.Priorities.Select(p => p.PriorityType)))
                .ForMember(dest => dest. Labels,
                    opt => opt.MapFrom(src => src.ProjectLabels.Select(pl => pl.Label.Name)))
                .ReverseMap()
                .ForMember(dest => dest.Priorities, opt => opt.Ignore())
                .ForMember(dest => dest.Issues, opt => opt.Ignore());

            CreateMap<Project, ProjectListModel>()
                .ForMember(dest => dest.LeaderName,
                    opt => opt.MapFrom(src => src.Leader.UserName))
                .ForMember(dest => dest.Issues,
                    opt => opt.MapFrom(src => src.Issues.Count));

            CreateMap<Issue, IssueListModel>()
                .ForMember(dest => dest.Project, opt => opt.ExplicitExpansion())
                .ForMember(dest => dest.Assignee,
                    opt => opt.MapFrom(src => src.Assignee.UserName))
                .ForMember(dest => dest.Project,
                    opt => opt.MapFrom(src => src.Project.Name));

            CreateMap<Issue, IssueBaseModel>()
                .ForMember(dest => dest.Comments,
                    opt => opt.MapFrom(src => src.Comments.OrderByDescending(c => c.Created)));

            var currentUserId = string.Empty;
            CreateMap<Issue, IssueDetailsModel>()
                .IncludeBase<Issue, IssueBaseModel>()
                .ForMember(dest => dest.Assignee,
                    opt => opt.MapFrom(src => src.Assignee.UserName))
                .ForMember(dest => dest.Labels,
                    opt => opt.MapFrom(src => src.IssueLabels.Select(li => li.Label.Name)))
                .ForMember(dest => dest.CanComment,
                    opt => opt.MapFrom(src => src.Project.Issues.Any(i => i.AssigneeId == currentUserId)));

            CreateMap<Issue, IssueViewModel>()
                .IncludeBase<Issue, IssueBaseModel>()
                .ForMember(dest => dest.CanComment, opt => opt.UseValue(true))
                .ForMember(dest => dest.ProjectLeadId,
                    opt => opt.MapFrom(src => src.Project.LeaderId))
                .ForMember(dest => dest.SelectedLabelIds,
                    opt => opt.MapFrom(src => src.IssueLabels.Select(li => li.Label.Id)))
                .ReverseMap()
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.IssueLabels.ReplaceEntityCollection(
                    src.SelectedLabelIds.Select(id => new IssueLabel { LabelId = id }).ToArray(),
                    il => il.LabelId
                ));

            CreateMap<Comment, CommentViewModel>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.UserName));
        }
    }
}
