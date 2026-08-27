using System.ComponentModel.DataAnnotations;
using IntegratingWithSwagger.Validation;

namespace IntegratingWithSwagger.Models;

public class UpdateBlogRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 120 characters.")]
    [NotOnlyWhitespace(ErrorMessage = "Title cannot be only whitespace.")]
    public string Title { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Content is required.")]
    [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters.")]
    [NotOnlyWhitespace(ErrorMessage = "Content cannot be only whitespace.")]
    public string Content { get; set; } = string.Empty;
}
