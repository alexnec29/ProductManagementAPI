using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Common.Mapping.Resolvers
{
    public class ProductAgeResolver : IValueResolver<Product, AdvancedProductDtos, string>
    {
        public string Resolve(Product source, AdvancedProductDtos destination, string destMember, ResolutionContext context)
        {
            var days = (DateTime.UtcNow - source.ReleaseDate).Days;

            if (days < 30)
                return "New Release";
            if (days < 365)
                return $"{days / 30} months old";
            if (days < 1825)
                return $"{days / 365} years old";
            
            return "Classic";
        }
    }
}