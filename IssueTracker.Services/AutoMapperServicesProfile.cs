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
                .ForMember(dest => dest.PriorityIds,
                    opt => opt.MapFrom(src => src.Priorities.Select(p => p.PriorityType)))
                .ForMember(dest => dest.Labels,
                    opt => opt.MapFrom(src => src.ProjectLabels.Select(pl => pl.Label.Name)))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Priorities,
                    opt => opt.MapFrom(src => src.PriorityIds.Select(id => new Priority { PriorityType = (PriorityType) id})))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(src => 
                    string.Concat(src.Name.Split().Select(word => word.ToUpper()[0]))));
        }
    }
}
