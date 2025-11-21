using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Common.Mapping.Resolvers
{
    public class PriceFormatterResolver : IValueResolver<Product, AdvancedProductDtos, string>
    {
        public string Resolve(Product source, AdvancedProductDtos destination, string destMember, ResolutionContext context)
        {
            return source.Price.ToString("C2"); // formats as currency
        }
    }
}