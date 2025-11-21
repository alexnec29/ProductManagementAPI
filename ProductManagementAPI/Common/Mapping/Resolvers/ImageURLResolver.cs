using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Common.Mapping.Resolvers;

public class ImageUrlResolver : IValueResolver<Product, AdvancedProductDtos, string?>
{
    public string? Resolve(Product source, AdvancedProductDtos destination, string? destMember, ResolutionContext context)
    {
        return source.Category == ProductCategory.Home ? null : source.ImageUrl;
    }
}