namespace ProductManagementAPI.Features.Products;

public class CreateProductProfileRequest
{
    public string Name { get; set; }
    public string Brand { get; set; }
    public string SKU { get; set; }
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string? ImageURL { get; set; }
    public int StockQuantity { get; set; } = 1;
}