using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Validators.Helpers;

namespace ProductManagementAPI.Validators.BusinessRules;

public class ProductBusinessRules
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductBusinessRules> _logger;

    public ProductBusinessRules(ApplicationDbContext context, ILogger<ProductBusinessRules> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateBusinessRules(
        CreateProductProfileRequest request,
        CancellationToken cancellationToken)
    {
        var dailyLimitResult = await CheckDailyProductLimit(cancellationToken);
        if (!dailyLimitResult.IsValid)
            return dailyLimitResult;
        
        var electronicsPriceResult = CheckElectronicsMinimumPrice(request);
        if (!electronicsPriceResult.IsValid)
            return electronicsPriceResult;
        
        var homeContentResult = CheckHomeProductContentRestrictions(request);
        if (!homeContentResult.IsValid)
            return homeContentResult;
        
        var highValueStockResult = CheckHighValueProductStockLimit(request);
        if (!highValueStockResult.IsValid)
            return highValueStockResult;

        return (true, null);
    }

    private async Task<(bool IsValid, string? ErrorMessage)> CheckDailyProductLimit(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todayCount = await _context.Products
            .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
            .CountAsync(cancellationToken);

        _logger.LogInformation("Daily product count: {Count}/500", todayCount);

        if (todayCount >= 500)
        {
            _logger.LogWarning("Daily product limit reached: {Count}", todayCount);
            return (false, "Daily product creation limit of 500 has been reached. Please try again tomorrow.");
        }

        return (true, null);
    }
    
    private (bool IsValid, string? ErrorMessage) CheckElectronicsMinimumPrice(CreateProductProfileRequest request)
    {
        if (request.Category == ProductCategory.Electronics && request.Price < 50.00m)
        {
            _logger.LogWarning("Electronics product price too low: {Price}", request.Price);
            return (false, "Electronics products must have a minimum price of $50.00");
        }

        return (true, null);
    }
    
    private (bool IsValid, string? ErrorMessage) CheckHomeProductContentRestrictions(CreateProductProfileRequest request)
    {
        if (request.Category == ProductCategory.Home)
        {
            if (KeywordLists.HomeInappropriateWords.Any(word => 
                request.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Home product contains restricted words: {Name}", request.Name);
                return (false, "Home product name contains inappropriate content");
            }
        }

        return (true, null);
    }
    private (bool IsValid, string? ErrorMessage) CheckHighValueProductStockLimit(CreateProductProfileRequest request)
    {
        if (request.Price > 500m && request.StockQuantity > 10)
        {
            _logger.LogWarning(
                "High-value product stock exceeds limit: Price={Price}, Stock={Stock}", 
                request.Price, 
                request.StockQuantity);
            
            return (false, "Products valued over $500 cannot have more than 10 units in stock");
        }

        return (true, null);
    }
}