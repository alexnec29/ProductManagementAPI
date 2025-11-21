using ProductManagementAPI.Validators.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ProductManagementAPI.Features.Products.DTOs;

public class AdvancedProductDtos
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [ValidSku]
    public string Sku { get; set; } = string.Empty;

    [ProductCategory(ProductCategory.Electronics, ProductCategory.Clothing, ProductCategory.Books, ProductCategory.Home)]
    public ProductCategory Category { get; set; }

    // Read-only string for display
    public string CategoryDisplayName => Category.ToString();

    [PriceRange(1, 10000)]
    public decimal Price { get; set; }

    public string FormattedPrice { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(2000)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }

    public string ProductAge { get; set; } = string.Empty;
    public string BrandInitials { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
}