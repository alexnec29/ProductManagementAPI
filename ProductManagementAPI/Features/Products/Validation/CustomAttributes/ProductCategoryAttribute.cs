using System;
using System.ComponentModel.DataAnnotations;

namespace ProductManagementAPI.Features.Products.Validation;

public class ProductCategoryAttribute : ValidationAttribute
{
    private readonly ProductCategory[] _allowedCategories;

    public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
    {
        _allowedCategories = allowedCategories;
    }

    public override bool IsValid(object? value)
    {
        if (value is not ProductCategory category) return false;
        return Array.Exists(_allowedCategories, c => c == category);
    }
}