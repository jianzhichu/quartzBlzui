using AutoMapper;
using QM.Model;
using QM.Utility.Extensions;
using QM.Utility.Quartz;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace QM.BlazorAdmin
{
    public class AutoMapperConfig : Profile
    {
        //添加你的实体映射关系.
        public AutoMapperConfig()
        {
            CreateMap<QuartzModel , QuartzOption>()
            .ForMember(p => p.DisplayState , x => x.Ignore())
            .ForMember(p => p.DisplayExecuteType , x => x.Ignore());
            //.ForMember(p => p.DisplayIntervalType, x => x.Ignore());
            CreateMap<QuartzOption , QuartzModel>();
            //.ForMember(p => p.Id, x => x.Ignore());



            CreateMap<QuartzModel , QuartzOptionDTO>()
.ForMember(p => p.DisplayState , x => x.Ignore())
.ForMember(p => p.DisplayExecuteType , x => x.Ignore());
            //.ForMember(p => p.DisplayIntervalType, x => x.Ignore());
            CreateMap<QuartzOptionDTO , QuartzModel>();


            CreateMap<QuartzOption , QuartzOptionDTO>();
            CreateMap<QuartzOptionDTO , QuartzOption>();
        }
    }
}
