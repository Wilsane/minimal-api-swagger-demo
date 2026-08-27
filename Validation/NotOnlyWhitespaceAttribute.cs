using System.ComponentModel.DataAnnotations;

namespace IntegratingWithSwagger.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class NotOnlyWhitespaceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // [Required] handles missing values; this only judges strings that are present.
        if (value is not string text || !string.IsNullOrWhiteSpace(text))
        {
            return ValidationResult.Success;
        }

        var memberName = validationContext.MemberName ?? validationContext.DisplayName;

        return new ValidationResult(
            ErrorMessage ?? $"{memberName} cannot be only whitespace.",
            memberName is null ? null : [memberName]);
    }
}
