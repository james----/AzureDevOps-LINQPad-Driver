using AutoMapper;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;

namespace AzureDevOpsDataContextDriver
{
    public class AzureWorkItemProfile : Profile
    {
        public AzureWorkItemProfile(AzureDevOpsConnectionInfo connectionInfo)
        {
            CreateMap<WorkItem, AzureWorkItem>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => (int)src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => (string)src.Fields["System.Title"]))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => (string)src.Fields["System.WorkItemType"]))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => (string)src.Fields["System.State"]))
                .ForMember(dest => dest.IterationPath, opt => opt.MapFrom(src => (string)src.Fields["System.IterationPath"]))
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.Fields.ContainsKey("System.AssignedTo") ? (string)src.Fields["System.AssignedTo"] : string.Empty))
                .ForMember(dest => dest.Blocked, opt => opt.MapFrom(src => src.Fields.ContainsKey("Microsoft.VSTS.CMMI.Blocked") ? (string)src.Fields["Microsoft.VSTS.CMMI.Blocked"] : string.Empty))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.Fields.ContainsKey("System.CreatedDate") ? (DateTime)src.Fields["System.CreatedDate"] : DateTime.MinValue))
                .ForMember(dest => dest.ClosedDate, opt => opt.MapFrom(src => src.Fields.ContainsKey("Microsoft.VSTS.Common.ClosedDate") ? (DateTime)src.Fields["Microsoft.VSTS.Common.ClosedDate"] : DateTime.MinValue))
                .ForMember(dest => dest.BacklogPriority, opt => opt.MapFrom(src => src.Fields.ContainsKey("Microsoft.VSTS.Common.BacklogPriority") ? (double)src.Fields["Microsoft.VSTS.Common.BacklogPriority"] : double.MaxValue))
                .ForMember(dest => dest.Elapsed, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForCtorParam("conn", opt => opt.MapFrom(x => connectionInfo));
        }
    }
}
