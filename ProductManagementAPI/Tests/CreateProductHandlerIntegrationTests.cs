using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using ProductManagementAPI.Common.Logging;
using ProductManagementAPI.Common.Mapping;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
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
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AdvancedProductMappingProfile>();
        });
        
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<CreateProductHandler>>();
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
            Brand = "Tech Corp",
            Sku = "ELEC-12345",
            Category = ProductCategory.Electronics,
            Price = 999.99m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-2),
            StockQuantity = 10,
            ImageUrl = "https://example.com/image.jpg"
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.NotNull(result);
        Assert.IsType<AdvancedProductDtos>(result);
        
        Assert.Equal("Electronics & Technology", result.CategoryDisplayName);
        
        Assert.Equal("TC", result.BrandInitials);
        
        Assert.Contains("month", result.ProductAge.ToLower());
        
        Assert.StartsWith("$", result.FormattedPrice);
        
        Assert.Equal("In Stock", result.AvailabilityStatus);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.ProductCreationStarted),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
    {
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Existing Product",
            Brand = "BrandA",
            Sku = "DUPL-001",
            Category = ProductCategory.Books,
            Price = 19.99m,
            ReleaseDate = DateTime.UtcNow,
            StockQuantity = 5,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();
        
        var request = new CreateProductProfileRequest
        {
            Name = "New Product",
            Brand = "BrandB",
            Sku = "DUPL-001",
            Category = ProductCategory.Books,
            Price = 25m,
            ReleaseDate = DateTime.UtcNow,
            StockQuantity = 5
        };
        
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(request, CancellationToken.None));
        
        Assert.Contains("already exists", exception.Message);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.Is<EventId>(e => e.Id == LogEvents.ProductValidationFailed),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Sofa Set",
            Brand = "Home Deco",
            Sku = "HOME-001",
            Category = ProductCategory.Home,
            Price = 500m,
            ReleaseDate = DateTime.UtcNow.AddMonths(-6),
            StockQuantity = 3,
            ImageUrl = "https://example.com/sofa.jpg"
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Equal("Home & Garden", result.CategoryDisplayName);
        
        Assert.Equal(450m, result.Price);
        
        Assert.Null(result.ImageUrl);
        
        Assert.Equal("HD", result.BrandInitials);
        Assert.Equal("Limited Stock", result.AvailabilityStatus);
    }

    [Fact]
    public async Task Handle_ValidProduct_LogsMetricsCorrectly()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Test Product",
            Brand = "TestBrand",
            Sku = "TEST-001",
            Category = ProductCategory.Books,
            Price = 29.99m,
            ReleaseDate = DateTime.UtcNow.AddYears(-1),
            StockQuantity = 15
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.SkuValidationPerformed),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.StockValidationPerformed),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.DatabaseOperationStarted),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.DatabaseOperationCompleted),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.Is<EventId>(e => e.Id == LogEvents.CacheOperationPerformed),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClothingProduct_CorrectMappings()
    {
        var request = new CreateProductProfileRequest
        {
            Name = "Designer Shirt",
            Brand = "FashionBrand",
            Sku = "CLOTH-001",
            Category = ProductCategory.Clothing,
            Price = 79.99m,
            ReleaseDate = DateTime.UtcNow.AddDays(-10),
            StockQuantity = 1,
            ImageUrl = "https://example.com/shirt.jpg"
        };
        
        var result = await _handler.Handle(request, CancellationToken.None);
        
        Assert.Equal("Clothing & Fashion", result.CategoryDisplayName);
        Assert.Equal("New Release", result.ProductAge);
        Assert.Equal("Last Item", result.AvailabilityStatus);
        Assert.Equal("F", result.BrandInitials);
        Assert.NotNull(result.ImageUrl);
    }
}