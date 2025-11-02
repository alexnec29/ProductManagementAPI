using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using FluentValidation;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Features.Products.Mapping;
using Xunit;

namespace ProductManagementAPI.Tests;

public class CreateProductHandlerIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<CreateProductHandler>> _loggerMock;
    private readonly CreateProductHandler _handler;

    public CreateProductHandlerIntegrationTests()
    {
        // 1. In-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        // 2. AutoMapper configuration
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<AdvancedProductMappingProfile>();
        });

        // Optional: validate mappings
        config.AssertConfigurationIsValid();

        _mapper = config.CreateMapper();

        // 3. Memory cache
        _cache = new MemoryCache(new MemoryCacheOptions());

        // 4. Mock logger
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();

        // 5. Handler instance
        _handler = new CreateProductHandler(_context, _mapper, _cache, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _cache?.Dispose();
    }

    [Fact]
    public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Smartphone X",
            Brand = "TechCorp",
            SKU = "ELEC-12345",
            Category = ProductCategory.Electronics,
            Price = 999.99m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-2),
            StockQuantity = 10,
            ImageURL = "https://example.com/image.jpg" // fix property name
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Electronics & Technology", result.CategoryDisplayName);
        Assert.Equal("TX", result.BrandInitials);
        Assert.StartsWith("$", result.FormattedPrice);
        Assert.Equal("In Stock", result.AvailabilityStatus);

        _loggerMock.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            2001,
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        var existingProduct = new Product
        {
            Name = "Existing Product",
            Brand = "BrandA",
            SKU = "DUPL-001",
            Category = ProductCategory.Books,
            Price = 19.99m,
            ReleaseDate = DateTime.UtcNow,
            StockQuantity = 5
        };
        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "BrandB",
            SKU = "DUPL-001",
            Category = ProductCategory.Books,
            Price = 25m,
            ReleaseDate = DateTime.UtcNow,
            StockQuantity = 5
        };

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(request, CancellationToken.None));

        _loggerMock.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            2002,
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Sofa Set",
            Brand = "HomeDeco",
            SKU = "HOME-001",
            Category = ProductCategory.Home,
            Price = 500m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-6),
            StockQuantity = 3,
            ImageURL = "https://example.com/sofa.jpg" // fix property name
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.Equal("Home & Garden", result.CategoryDisplayName);
        Assert.Equal(450m, result.Price); // 10% discount applied
        Assert.Null(result.ImageURL); // content filtering for Home category
    }
}