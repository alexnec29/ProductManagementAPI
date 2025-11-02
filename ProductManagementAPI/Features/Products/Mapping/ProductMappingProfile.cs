using AutoMapper;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Features.Products.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<CreateProductProfileRequest, Product>();
            CreateMap<Product, ProductProfileDto>();
        }
    }
}