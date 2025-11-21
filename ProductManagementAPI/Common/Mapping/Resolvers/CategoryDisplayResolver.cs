using AutoMapper;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;

namespace ProductManagementAPI.Common.Mapping.Resolvers
{
    public class CategoryDisplayResolver : IValueResolver<Product, AdvancedProductDtos, string>
    {
        public string Resolve(Product source, AdvancedProductDtos destination, string destMember, ResolutionContext context)
        {
            return source.Category switch
            {
                ProductCategory.Electronics => "Electronics & Technology",
                ProductCategory.Clothing => "Clothing & Fashion",
                ProductCategory.Books => "Books & Media",
                ProductCategory.Home => "Home & Garden",
                _ => "Uncategorized"
            };
        }
    }
}