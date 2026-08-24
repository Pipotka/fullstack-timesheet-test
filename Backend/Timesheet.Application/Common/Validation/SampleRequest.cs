using FluentValidation;

namespace Timesheet.Application.Common.Validation;

/// <summary>
/// Sample request for testing FluentValidation DI registration.
/// Can be removed when real validators are added.
/// </summary>
public sealed record SampleRequest(string Value);

/// <summary>
/// Sample validator for testing FluentValidation DI registration.
/// Can be removed when real validators are added.
/// </summary>
public sealed class SampleRequestValidator : AbstractValidator<SampleRequest>
{
    public SampleRequestValidator()
    {
        RuleFor(x => x.Value).NotEmpty();
    }
}
