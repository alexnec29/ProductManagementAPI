using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Features.Products.Mapping.Resolvers
{
    public class BrandInitialsResolver : IValueResolver<Product, ProductProfileDto, string>
    {
        public string Resolve(Product source, ProductProfileDto destination, string destMember, ResolutionContext context)
        {
            if (string.IsNullOrWhiteSpace(source.Brand))
                return "?";

            var words = source.Brand.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return words.Length == 1
                ? words[0][0].ToString().ToUpper()
                : $"{words[0][0]}{words[^1][0]}".ToUpper();
        }
    }
}