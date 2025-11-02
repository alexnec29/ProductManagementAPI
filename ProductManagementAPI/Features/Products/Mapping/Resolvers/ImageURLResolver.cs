using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

public class ImageURLResolver : IValueResolver<Product, ProductProfileDto, string?>
{
    public string? Resolve(Product source, ProductProfileDto destination, string? destMember, ResolutionContext context)
    {
        return source.Category == ProductCategory.Home ? null : source.ImageURL;
    }
}