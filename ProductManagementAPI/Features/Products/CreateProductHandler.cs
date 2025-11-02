using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ProductManagementAPI.Features.Products.DTOs;
using System.ComponentModel.DataAnnotations;
using ProductManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ProductManagementAPI.Features.Products;

public class CreateProductHandler
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IMemoryCache cache,
        ILogger<CreateProductHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProductProfileDto> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        // Log product creation attempt
        _logger.LogInformation("Creating product: Name={Name}, Brand={Brand}, Category={Category}, SKU={SKU}",
            request.Name, request.Brand, request.Category, request.SKU);

        // Check SKU uniqueness
        bool skuExists = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU, cancellationToken);

        if (skuExists)
        {
            _logger.LogWarning("Product creation failed: SKU '{SKU}' already exists", request.SKU);
            throw new ValidationException($"Product with SKU '{request.SKU}' already exists.");
        }

        // Map request → Product entity
        var product = _mapper.Map<Product>(request);

        // Save to database
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully: Id={ProductId}", product.Id);

        // Remove cache for all products
        _cache.Remove("all_products");

        // Map Product → ProductProfileDto
        return _mapper.Map<ProductProfileDto>(product);
    }
}
