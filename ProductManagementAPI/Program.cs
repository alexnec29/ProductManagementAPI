using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProductManagementAPI.Common.Mapping;
using ProductManagementAPI.Common.Middleware;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Validators;
using ProductManagementAPI.Common.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => { options.UseInMemoryDatabase("ProductsDb"); });

builder.Services.AddAutoMapper(typeof(AdvancedProductMappingProfile));

builder.Services.AddScoped<CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

builder.Services.AddMemoryCache();

builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information);
});

builder.Services.AddScoped<CreateProductHandler>();

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

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1"); });
}

app.UseAuthorization();

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