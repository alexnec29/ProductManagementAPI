using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.RegularExpressions;

public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value)
    {
        if (value is not string s) return false;
        s = s.Replace(" ", "");
        return Regex.IsMatch(s, @"^[A-Za-z0-9\-]{5,20}$");
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes.Add("data-val", "true");
        context.Attributes.Add("data-val-sku", "Invalid SKU format");
    }
}