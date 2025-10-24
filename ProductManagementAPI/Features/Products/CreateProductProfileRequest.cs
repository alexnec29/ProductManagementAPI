namespace ProductManagementAPI.Features.Products;

public class CreateProductProfileRequest
{
    private string Name;
    private string Brand;
    private string SKU;
    private ProductCategory Category;
    private decimal Price;
    private DateTime ReleaseDate;
    private string? ImageURL;
    private int StockQuantity = 1;
}