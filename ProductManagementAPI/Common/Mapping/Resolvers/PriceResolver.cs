using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Common.Mapping.Resolvers;

public class PriceResolver : IValueResolver<Product, AdvancedProductDtos, decimal>
{
    public decimal Resolve(Product source, AdvancedProductDtos destination, decimal destMember, ResolutionContext context)
    {
        return source.Category == ProductCategory.Home ? source.Price * 0.9m : source.Price;
    }
}