namespace ProductManagementAPI.Features.Products;

public class Product
{
    private string Name;
    private string Brand;
    private string SKU;
    private ProductCategory Category;
    private decimal Price;
    private DateTime ReleaseDate;
    private string? ImageURL;
    private bool IsAvailable;
    private int StockQuantity = 0;
}