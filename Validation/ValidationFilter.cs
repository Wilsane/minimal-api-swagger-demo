using System.ComponentModel.DataAnnotations;

namespace IntegratingWithSwagger.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["body"] = [$"A request body of type '{typeof(T).Name}' is required."]
                },
                title: "Missing request body.");
        }

        var results = new List<ValidationResult>();
        var validationContext = new ValidationContext(argument);

        if (Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true))
        {
            return await next(context);
        }

        // A result can name several members and a member can fail several rules,
        // so flatten to member -> messages.
        var errors = results
            .SelectMany(
                result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, member) => (Member: member, Message: result.ErrorMessage ?? "Invalid value."))
            .GroupBy(entry => entry.Member, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(entry => entry.Message).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

        return TypedResults.ValidationProblem(errors, title: "One or more validation errors occurred.");
    }
}
