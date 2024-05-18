using AutoMapper;
using Contracts;
using SearchService.Models;

namespace SearchService.MappingProfile
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<AuctionCreated, Item>();
            CreateMap<AuctionUpdated, Item>();
        }
    }
}
