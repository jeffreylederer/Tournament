using System;
using System.ComponentModel.DataAnnotations;

public class CustomCompareAttribute : ValidationAttribute
{
    private readonly string _otherProperty;

    public CustomCompareAttribute(string otherProperty)
    {
        _otherProperty = otherProperty;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var property = validationContext.ObjectType.GetProperty(_otherProperty);
        if (property == null)
            throw new ArgumentException($"Property {_otherProperty} not found");

        var otherValue = property.GetValue(validationContext.ObjectInstance);
        if (Equals(value, otherValue))
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}
