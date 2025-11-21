using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProductManagementAPI.Common.Logging;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products.DTOs;

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

    public async Task<AdvancedProductDtos> Handle(CreateProductProfileRequest request, CancellationToken cancellationToken)
    {
        var stopwatchTotal = Stopwatch.StartNew();
        var operationId = Guid.NewGuid().ToString("N")[..8]; // 8 chars

        using (_logger.BeginScope("OperationId={OperationId}", operationId))
        {
            _logger.LogInformation(LogEvents.ProductCreationStarted,
                "Starting product creation: Name={Name}, Brand={Brand}, SKU={SKU}, Category={Category}",
                request.Name, request.Brand, request.Sku, request.Category);

            var stopwatchValidation = Stopwatch.StartNew();
            try
            {
                // SKU validation
                _logger.LogInformation(LogEvents.SkuValidationPerformed, "Validating SKU {SKU}", request.Sku);

                if (await _context.Products.AnyAsync(p => p.Sku == request.Sku, cancellationToken))
                {
                    _logger.LogWarning(LogEvents.ProductValidationFailed,
                        "SKU '{SKU}' already exists", request.Sku);

                    throw new ValidationException($"Product with SKU '{request.Sku}' already exists.");
                }

                // Stock validation (optional logic)
                _logger.LogInformation(LogEvents.StockValidationPerformed,
                    "Stock validation: Quantity={Quantity}", request.StockQuantity);

                stopwatchValidation.Stop();

                // Database save
                var stopwatchDb = Stopwatch.StartNew();
                _logger.LogInformation(LogEvents.DatabaseOperationStarted, "Saving product to database");

                var product = _mapper.Map<Product>(request);
                _context.Products.Add(product);
                await _context.SaveChangesAsync(cancellationToken);

                stopwatchDb.Stop();
                _logger.LogInformation(LogEvents.DatabaseOperationCompleted, "Saved product Id={ProductId}", product.Id);

                // Cache
                _cache.Remove("all_products");
                _logger.LogInformation(LogEvents.CacheOperationPerformed, "Cache 'all_products' updated");

                stopwatchTotal.Stop();

                // Log metrics
                var metrics = new LoggingModels
                {
                    OperationId = operationId,
                    ProductName = request.Name,
                    Sku = request.Sku,
                    Category = request.Category,
                    ValidationDuration = stopwatchValidation.Elapsed,
                    DatabaseSaveDuration = stopwatchDb.Elapsed,
                    TotalDuration = stopwatchTotal.Elapsed,
                    Success = true
                };

                _logger.LogProductCreationMetrics(metrics);

                return _mapper.Map<AdvancedProductDtos>(product);
            }
            catch (Exception ex)
            {
                stopwatchTotal.Stop();

                var metrics = new LoggingModels
                {
                    OperationId = operationId,
                    ProductName = request.Name,
                    Sku = request.Sku,
                    Category = request.Category,
                    ValidationDuration = stopwatchValidation.Elapsed,
                    DatabaseSaveDuration = TimeSpan.Zero,
                    TotalDuration = stopwatchTotal.Elapsed,
                    Success = false,
                    ErrorReason = ex.Message
                };

                _logger.LogProductCreationMetrics(metrics);
                throw; // rethrow for global handler
            }
        }
    }
}
