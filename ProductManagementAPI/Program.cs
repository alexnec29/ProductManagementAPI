using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductManagementAPI.Common.Logging;
using ProductManagementAPI.Common.Mapping;
using ProductManagementAPI.Common.Middleware;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Validators;
using ProductManagementAPI.Validators.BusinessRules;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase("ProductsDb"); });

// AutoMapper
builder.Services.AddAutoMapper(typeof(AdvancedProductMappingProfile));

// Validators
builder.Services.AddScoped<CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

// Business rules
builder.Services.AddScoped<ProductBusinessRules>();

// Memory cache
builder.Services.AddMemoryCache();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Handlers
builder.Services.AddScoped<CreateProductHandler>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Product Management API",
        Version = "v1",
        Description = "Advanced .NET API for Product Management with AutoMapper, Validation, and Logging"
    });
});

var app = builder.Build();

// Middleware
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1"); });
}

app.UseAuthorization();

// POST /products
app.MapPost("/products", async (
        CreateProductProfileRequest request,
        CreateProductHandler handler,
        CreateProductProfileValidator validator,
        ILogger<Program> logger,
        CancellationToken ct) =>
    {
        logger.LogInformation(LogEvents.ProductCreationStarted,
            "Product creation started for {ProductName} | SKU={SKU}", request.Name, request.Sku);

        var validationStart = DateTime.UtcNow;
        var validationResult = await validator.ValidateAsync(request, ct);
        var validationEnd = DateTime.UtcNow;

        if (!validationResult.IsValid)
        {
            logger.LogWarning(LogEvents.ProductValidationFailed,
                "Validation failed for product {ProductName} | SKU={SKU}", request.Name, request.Sku);

            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        logger.LogInformation(LogEvents.DatabaseOperationStarted,
            "Saving product {ProductName} | SKU={SKU} to database", request.Name, request.Sku);

        var dbStart = DateTime.UtcNow;
        var result = await handler.Handle(request, ct);
        var dbEnd = DateTime.UtcNow;

        logger.LogInformation(LogEvents.DatabaseOperationCompleted,
            "Database operation completed for product {ProductName} | SKU={SKU}", request.Name, request.Sku);

        var totalDuration = DateTime.UtcNow - validationStart;

        var metrics = new LoggingModels
        {
            OperationId = Guid.NewGuid().ToString(),
            ProductName = request.Name,
            Sku = request.Sku,
            Category = request.Category,
            ValidationDuration = validationEnd - validationStart,
            DatabaseSaveDuration = dbEnd - dbStart,
            TotalDuration = totalDuration,
            Success = true
        };

        logger.LogProductCreationMetrics(metrics);

        logger.LogInformation(LogEvents.ProductCreationCompleted,
            "Product creation completed for {ProductName} | SKU={SKU}", request.Name, request.Sku);

        return Results.Ok(result);
    })
    .WithName("CreateProduct")
    .WithTags("Products")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Create a new product";
        operation.Description = "Creates a new product with advanced validation, logging, and caching";
        return operation;
    })
    .Produces<AdvancedProductDtos>()
    .ProducesValidationProblem();

// GET /products
app.MapGet("/products", async (ApplicationDbContext context, CancellationToken ct) =>
    {
        var products = await context.Products.ToListAsync(ct);
        return Results.Ok(products);
    })
    .WithName("GetAllProducts")
    .WithTags("Products")
    .WithOpenApi(operation =>
    {
        operation.Summary = "Get all products";
        operation.Description = "Retrieves all products from the database";
        return operation;
    });

app.Run();