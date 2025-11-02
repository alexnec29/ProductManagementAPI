using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products.Validation.Helpers;

namespace ProductManagementAPI.Features.Products.Validation
{
    public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateProductProfileValidator> _logger;

        public CreateProductProfileValidator(ApplicationDbContext context, ILogger<CreateProductProfileValidator> logger)
        {
            _context = context;
            _logger = logger;

            // Name rules
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .Length(1, 200)
                .MustAsync(BeUniqueName).WithMessage("Name must be unique per brand")
                .Must(BeValidName).WithMessage("Name contains inappropriate words");

            // Brand rules
            RuleFor(x => x.Brand)
                .NotEmpty()
                .Length(2, 100)
                .Matches(@"^[\w\s\-\.'0-9]+$").WithMessage("Invalid brand characters");

            // SKU rules
            RuleFor(x => x.SKU)
                .NotEmpty()
                .Must(BeValidSKU).WithMessage("Invalid SKU format")
                .MustAsync(BeUniqueSKU).WithMessage("SKU already exists");

            // Category, Price, ReleaseDate
            RuleFor(x => x.Category).IsInEnum();
            RuleFor(x => x.Price).GreaterThan(0).LessThan(10000);
            RuleFor(x => x.ReleaseDate).LessThanOrEqualTo(DateTime.UtcNow)
                                       .GreaterThan(new DateTime(1900, 1, 1));

            // Stock
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100_000);

            // Image URL
            RuleFor(x => x.ImageURL).Must(BeValidImageUrl).When(x => !string.IsNullOrEmpty(x.ImageURL));

            // Conditional / cross-field
            When(x => x.Category == ProductCategory.Electronics, () =>
            {
                RuleFor(x => x.Price).GreaterThanOrEqualTo(50);
                RuleFor(x => x.Name).Must(ValidationHelpers.ContainTechnologyKeywords);
                RuleFor(x => x.ReleaseDate).GreaterThan(DateTime.UtcNow.AddYears(-5));
            });

            When(x => x.Category == ProductCategory.Home, () =>
            {
                RuleFor(x => x.Price).LessThanOrEqualTo(200);
                RuleFor(x => x.Name).Must(ValidationHelpers.BeAppropriateForHome);
            });

            When(x => x.Category == ProductCategory.Clothing, () =>
            {
                RuleFor(x => x.Brand).MinimumLength(3);
            });

            RuleFor(x => x).Must(ValidationHelpers.ExpensiveStockRule)
                .WithMessage("Expensive products must have limited stock");
        }

        // --- Async validators ---
        private async Task<bool> BeUniqueName(CreateProductProfileRequest req, string name, CancellationToken ct)
        {
            bool exists = await _context.Products
                .AnyAsync(p => p.Name == name && p.Brand == req.Brand, ct);
            return !exists;
        }

        private async Task<bool> BeUniqueSKU(string sku, CancellationToken ct)
        {
            bool exists = await _context.Products.AnyAsync(p => p.SKU == sku, ct);
            return !exists;
        }

        // --- Sync helpers ---
        private bool BeValidName(string name) => !ValidationHelpers.ContainsInappropriateWords(name);

        private bool BeValidSKU(string sku)
        {
            sku = sku.Replace(" ", "");
            return System.Text.RegularExpressions.Regex.IsMatch(sku, @"^[A-Za-z0-9\-]{5,20}$");
        }

        private bool BeValidImageUrl(string url) => ValidationHelpers.BeValidImageUrl(url);
    }
}
