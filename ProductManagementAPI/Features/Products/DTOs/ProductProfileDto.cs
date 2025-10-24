﻿namespace ProductManagementAPI.Features.Products.DTOs;

public class ProductProfileDto
{
    public Guid Id;
    public string Name { get; set; }
    public string Brand { get; set; }
    public string SKU { get; set; }
    public string CategoryDisplayName { get; set; }
    public decimal Price { get; set; }
    public string FormattedPrice { get; set; }
    public DateTime ReleaseDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageURL { get; set; }
    public bool IsAvailable { get; set; }
    public int StockQuantity { get; set; }
    public string ProductAge { get; set; }
    public string BrandInitials { get; set; }
    public string AvailabilityStatus { get; set; }
}