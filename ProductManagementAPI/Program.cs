using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagementAPI.Data;
using ProductManagementAPI.Features.Products;
using ProductManagementAPI.Features.Products.DTOs;
using ProductManagementAPI.Features.Products.Mapping;
using ProductManagementAPI.Features.Products.Validation;
using ProductManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// In-memory database for demonstration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("ProductsDb");
});

// AutoMapper profiles
builder.Services.AddAutoMapper(typeof(ProductMappingProfile), typeof(AdvancedProductMappingProfile));

// Validators
builder.Services.AddScoped<CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

// Memory cache
builder.Services.AddMemoryCache();

// Logging
builder.Services.AddLogging();

// CreateProductHandler
builder.Services.AddScoped<CreateProductHandler>();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapPost("/products", async (
        CreateProductProfileRequest request, 
        CreateProductHandler handler, 
        CancellationToken ct) =>
    {
        var result = await handler.Handle(request, ct);
        return Results.Ok(result);
    })
    .WithName("CreateProduct")
    .WithTags("Products")
    .Produces<ProductProfileDto>(StatusCodes.Status200OK)
    .ProducesValidationProblem();

app.Run();