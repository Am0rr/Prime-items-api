using AutoMapper;
using PI.DAL.Entities.Catalog;
using PI.BLL.DTOs.Catalog;

namespace PI.BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
    }
}