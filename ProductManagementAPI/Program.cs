using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Common.Mapping;
using ProductManagementAPI.Common.Middleware;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("ProductsDb");
});

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
    c.SwaggerDoc("v1", new() 
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Management API v1");
    });
}

app.UseAuthorization();

app.MapPost("/products", async (
        CreateProductProfileRequest request,
        CreateProductHandler handler,
        CreateProductProfileValidator validator,
        CancellationToken ct) =>
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
        
        var result = await handler.Handle(request, ct);
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
    .Produces<AdvancedProductDtos>(StatusCodes.Status200OK)
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