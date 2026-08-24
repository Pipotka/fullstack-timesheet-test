using FluentAssertions;
using FluentValidation;
using MediatR;
using Timesheet.Application.Common.Behaviors;

namespace Timesheet.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Value) : IRequest<string>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Value).NotEmpty();
        }
    }

    private static Task<string> PipelineNext(CancellationToken ct)
        => Task.FromResult("ok");

    [Fact]
    public async Task Handle_WithNoValidators_CallsNext()
    {
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var result = await behavior.Handle(new TestRequest("x"), () => PipelineNext(CancellationToken.None), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithPassingValidator_CallsNext()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var result = await behavior.Handle(new TestRequest("x"), () => PipelineNext(CancellationToken.None), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationException()
    {
        var validators = new IValidator<TestRequest>[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var act = () => behavior.Handle(new TestRequest(""), () => PipelineNext(CancellationToken.None), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
