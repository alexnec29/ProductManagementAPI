using System.ComponentModel.DataAnnotations;

namespace ProductManagementAPI.Features.Products;

public class Product
{
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Brand { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public DateTime ReleaseDate { get; set; }

    [MaxLength(2000)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
