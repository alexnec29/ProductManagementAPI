using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

public class PriceResolver : IValueResolver<Product, ProductProfileDto, decimal>
{
    public decimal Resolve(Product source, ProductProfileDto destination, decimal destMember, ResolutionContext context)
    {
        return source.Category == ProductCategory.Home ? source.Price * 0.9m : source.Price;
    }
}