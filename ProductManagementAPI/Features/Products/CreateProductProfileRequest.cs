namespace ProductManagementAPI.Features.Products;
using System.ComponentModel.DataAnnotations;
using Validators.Attributes;
public class CreateProductProfileRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [ValidSku]
    public string Sku { get; set; } = string.Empty;

    [ProductCategory(ProductCategory.Electronics, ProductCategory.Clothing, ProductCategory.Books, ProductCategory.Home)]
    public ProductCategory Category { get; set; }

    [PriceRange(1, 10000)]
    public decimal Price { get; set; }

    public DateTime ReleaseDate { get; set; }

    [MaxLength(2000)]
    public string? ImageUrl { get; set; }

    public int StockQuantity { get; set; } = 1;
}