using FluentValidation;

namespace SchoolERP.Application.Common.Models;

public abstract class PagedRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public abstract class SearchablePagedRequest : PagedRequest
{
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; } = "desc";
}

public class SearchablePagedRequestValidator<T> : AbstractValidator<T>
    where T : SearchablePagedRequest
{
    public SearchablePagedRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.SortDirection)
            .Must(x => string.IsNullOrWhiteSpace(x) || x.Equals("asc", StringComparison.OrdinalIgnoreCase) || x.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("SortDirection must be either 'asc' or 'desc'.");
    }
}
