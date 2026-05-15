using AutoMapper;
using PI.DAL.Entities.Catalog;
using PI.DAL.Entities.Orders;
using PI.BLL.DTOs.Catalog;
using PI.BLL.DTOs.Orders;

namespace PI.BLL;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<Category, CategoryResponse>();

        CreateMap<OrderItem, OrderItemResponse>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

        CreateMap<Order, OrderResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}