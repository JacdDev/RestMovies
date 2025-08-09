using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    private static readonly string[] ValidSortFields = { "title", "yearofrelease" };
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(options => options.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(options => options.SortField)
            .Must(field => string.IsNullOrEmpty(field) || ValidSortFields.Contains(field, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortField must be one of the following: {string.Join(", ", ValidSortFields)}");

        RuleFor(options => options.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(options => options.PageSize)
            .InclusiveBetween(1, 25);
    }
}