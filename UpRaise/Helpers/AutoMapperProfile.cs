using UpRaise.DTOs;
using UpRaise.Entities;
using AutoMapper;
using UpRaise.DTOs.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;
using UpRaise.Models.Enums;
using UpRaise.Extensions;

namespace UpRaise.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IDUser, UserDTO>()
                .ForMember(i => i.Username, opt => opt.ConvertUsing(new LowercaseStringFormatter()))
                .ReverseMap();

            CreateMap<SignUpUserDTO, IDUser>()
                .ForMember(i => i.UserName, opt => opt.ConvertUsing(new LowercaseStringFormatter()));

            CreateMap<Campaign, CampaignDTO>()
                //Convert from Campaign -> CampaignDTO
                .ForMember(x => x.DistributionTerms, y => y.MapFrom(z => StringExtensions.ConvertJSONToList(z.DistributionTerms)))
                .ReverseMap()
                //Convert from CampaignDTO -> Campaign
                .ForMember(x => x.DistributionTerms, y => y.MapFrom(z => StringExtensions.ConvertListToJson(z.DistributionTerms)))
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.TypeId, y => y.Ignore())
                .ForMember(x => x.CreatedAt, y => y.Ignore())
                .ForMember(x => x.CreatedByUserId, y => y.Ignore());
                

            CreateMap<PeopleCampaign, PeopleCampaignDTO>()
              .ReverseMap();

            CreateMap<OrganizationCampaign, OrganizationCampaignDTO>()
              .ReverseMap();

            CreateMap<Campaign, YourCampaignDTO>()
                .ReverseMap();



            //
            //Search 
            //
            CreateMap<Campaign, UpRaise.Models.Search.Campaign>();
            CreateMap<PeopleCampaign, UpRaise.Models.Search.PeopleCampaign>();
            CreateMap<OrganizationCampaign, UpRaise.Models.Search.OrganizationCampaign>();

            //
            //Public
            //


            /*
            CreateMap<Company, CompanyDTO>()
                .ReverseMap();

            CreateMap<IDUser, UserCompanyDTO>();

            CreateMap<Claim, ClaimDTO>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.ClaimId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ApprovedByUserName, opt => opt.MapFrom(src => src.ApprovedByUserId.HasValue ? src.ApprovedByUser.FullName : string.Empty));
            */

        }

      

    }


    public class LowercaseStringFormatter : IValueConverter<string, string>
    {
        public string Convert(string source, ResolutionContext ctx)
        {
            if (!string.IsNullOrWhiteSpace(source))
                return source.ToLower();

            return string.Empty;
        }

    }

}