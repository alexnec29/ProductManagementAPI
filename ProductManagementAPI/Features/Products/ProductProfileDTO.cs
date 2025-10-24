namespace ProductManagementAPI.Features.Products;

public class ProductProfileDTO
{
    private Guid Id;
    private string Name;
    private string Brand;
    private string SKU;
    private string CategoryDisplayName;
    private decimal Price;
    private string FormattedPrice;
    private DateTime ReleaseDate;
    private DateTime CreatedAt;
    private string? ImageURL;
    private bool IsAvailable;
    private int StockQuantity;
    private string ProductAge;
    private string BrandInitials;
    private string AvailabilityStatus;
}