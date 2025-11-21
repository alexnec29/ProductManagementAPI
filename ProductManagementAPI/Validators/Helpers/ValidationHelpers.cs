using ProductManagementAPI.Features.Products;

namespace ProductManagementAPI.Validators.Helpers
{
    public static class ValidationHelpers
    {
        public static bool ContainsInappropriateWords(string input) =>
            KeywordLists.InappropriateWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase));

        public static bool ContainTechnologyKeywords(string name) =>
            KeywordLists.TechnologyKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

        public static bool BeAppropriateForHome(string name) =>
            !KeywordLists.HomeInappropriateWords.Any(w => name.Contains(w, StringComparison.OrdinalIgnoreCase));

        public static bool ExpensiveStockRule(dynamic product)
        {
            decimal price = product.Price;
            int stock = product.StockQuantity;
            ProductCategory category = product.Category;

            if (price > 100 && stock > 20) return false;
            if (category == ProductCategory.Electronics && product.ReleaseDate < DateTime.UtcNow.AddYears(-5)) return false;

            return true;
        }

        public static bool BeValidImageUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            if (!(uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) return false;
            return url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   || url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                   || url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                   || url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                   || url.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
        }
    }
}