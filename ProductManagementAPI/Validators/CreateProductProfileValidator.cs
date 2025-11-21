using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Validators.BusinessRules;
using ProductManagementAPI.Validators.Helpers;

namespace ProductManagementAPI.Validators;

public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateProductProfileValidator> _logger;
    private readonly ProductBusinessRules _businessRules;

    public CreateProductProfileValidator(
        ApplicationDbContext context,
        ILogger<CreateProductProfileValidator> logger,
        ProductBusinessRules businessRules)
    {
        _context = context;
        _logger = logger;
        _businessRules = businessRules;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(1, 200)
            .Must(BeValidName).WithMessage("Name contains inappropriate words")
            .MustAsync(BeUniqueName).WithMessage("Name must be unique per brand");

        RuleFor(x => x.Brand)
            .NotEmpty().WithMessage("Brand is required")
            .Length(2, 100)
            .Must(BeValidBrandName).WithMessage("Brand contains invalid characters");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .Must(BeValidSku).WithMessage("Invalid SKU format (5-20 alphanumeric characters with hyphens)")
            .MustAsync(BeUniqueSku).WithMessage("SKU already exists");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid product category");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(10000).WithMessage("Price must be less than $10,000");

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future")
            .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before 1900");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative")
            .LessThanOrEqualTo(100_000).WithMessage("Stock quantity cannot exceed 100,000");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidImageUrl).WithMessage("Invalid image URL format")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x)
            .MustAsync(PassBusinessRules)
            .WithMessage("Product violates business rules");

        When(x => x.Category == ProductCategory.Electronics, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(50).WithMessage("Electronics must cost at least $50.00");

            RuleFor(x => x.Name)
                .Must(ValidationHelpers.ContainTechnologyKeywords)
                .WithMessage("Electronics products must contain technology-related keywords");

            RuleFor(x => x.ReleaseDate)
                .GreaterThan(DateTime.UtcNow.AddYears(-5))
                .WithMessage("Electronics must be released within the last 5 years");
        });

        When(x => x.Category == ProductCategory.Home, () =>
        {
            RuleFor(x => x.Price)
                .LessThanOrEqualTo(200).WithMessage("Home products cannot exceed $200");

            RuleFor(x => x.Name)
                .Must(ValidationHelpers.BeAppropriateForHome)
                .WithMessage("Home product name contains inappropriate content");
        });

        When(x => x.Category == ProductCategory.Clothing, () =>
        {
            RuleFor(x => x.Brand)
                .MinimumLength(3).WithMessage("Clothing brand must be at least 3 characters");
        });

        RuleFor(x => x)
            .Must(ValidationHelpers.ExpensiveStockRule)
            .WithMessage("Expensive products (>$100) must have limited stock (≤20 units)");
    }

    private async Task<bool> BeUniqueName(CreateProductProfileRequest req, string name, CancellationToken ct)
    {
        _logger.LogInformation("Checking name uniqueness: Name={Name}, Brand={Brand}", name, req.Brand);

        var exists = await _context.Products
            .AnyAsync(p => p.Name == name && p.Brand == req.Brand, ct);

        if (exists)
            _logger.LogWarning("Duplicate product name found: Name={Name}, Brand={Brand}", name, req.Brand);

        return !exists;
    }

    private async Task<bool> BeUniqueSku(string sku, CancellationToken ct)
    {
        _logger.LogInformation("Checking SKU uniqueness: SKU={SKU}", sku);

        var exists = await _context.Products.AnyAsync(p => p.Sku == sku, ct);

        if (exists)
            _logger.LogWarning("Duplicate SKU found: SKU={SKU}", sku);

        return !exists;
    }

    private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Validating business rules for product: {Name}", request.Name);

        var result = await _businessRules.ValidateBusinessRules(request, ct);

        if (!result.IsValid)
            _logger.LogWarning("Business rule validation failed: {Error}", result.ErrorMessage);

        return result.IsValid;
    }

    private bool BeValidName(string name)
    {
        return !ValidationHelpers.ContainsInappropriateWords(name);
    }

    private bool BeValidBrandName(string brand)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(brand, @"^[\w\s\-\.'0-9]+$");
    }

    private bool BeValidSku(string sku)
    {
        sku = sku.Replace(" ", "");
        return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Za-z0-9\-]{5,20}$");
    }

    private bool BeValidImageUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return ValidationHelpers.BeValidImageUrl(url);
    }
}