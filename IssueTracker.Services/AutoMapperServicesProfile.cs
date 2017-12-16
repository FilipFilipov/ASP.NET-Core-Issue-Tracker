using System.Linq;
using AutoMapper;
using IssueTracker.Data.Models;
using IssueTracker.Services.Models;

namespace IssueTracker.Services
{
    public class AutoMapperServicesProfile : Profile
    {
        public AutoMapperServicesProfile()
        {
            CreateMap<Project, ProjectViewModel>()
                .ForMember(dest => dest.Priorities,
                    opt => opt.MapFrom(src => src.Priorities.Select(p => p.PriorityType)))
                .ForMember(dest => dest.Labels,
                    opt => opt.MapFrom(src => src.ProjectLabels.Select(pl => pl.Label.Name)))
                .ReverseMap()
                .ForMember(dest => dest.Priorities,
                    opt => opt.MapFrom(src => src.Priorities.Select(id => new Priority { PriorityType = id})));

            CreateMap<Project, ProjectListModel>()
                .ForMember(dest => dest.Leader,
                    opt => opt.MapFrom(src => src.Leader.UserName))
                .ForMember(dest => dest.Issues,
                    opt => opt.MapFrom(src => src.Issues.Count));

            CreateMap<Issue, IssueListModel>()
                .ForMember(dest => dest.Assignee, opt => opt.ExplicitExpansion())
                .ForMember(dest => dest.Project, opt => opt.ExplicitExpansion())
                .ForMember(dest => dest.Assignee,
                    opt => opt.MapFrom(src => src.Assignee.UserName))
                .ForMember(dest => dest.Project,
                    opt => opt.MapFrom(src => src.Project.Name))
                .ForMember(dest => dest.Labels,
                    opt => opt.MapFrom(src => src.IssueLabels.Select(li => li.Label.Name)));

            CreateMap<Issue, IssueViewModel>()
                .ForMember(dest => dest.Labels,
                    opt => opt.MapFrom(src => src.IssueLabels.Select(li => li.Label.Id)))
                .ReverseMap()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.IssueLabels,
                    opt => opt.MapFrom(src => src.Labels.Select(id => new IssueLabel { LabelId = id })));
        }
    }
}
