using System.ComponentModel.DataAnnotations;

namespace ProductManagementAPI.Validators.Attributes;

public class PriceRangeAttribute : ValidationAttribute
{
    private readonly decimal _min;
    private readonly decimal _max;

    public PriceRangeAttribute(double min, double max)
    {
        _min = (decimal)min;
        _max = (decimal)max;
    }

    public override bool IsValid(object? value)
    {
        if (value is not decimal price) return false;
        return price >= _min && price <= _max;
    }
}