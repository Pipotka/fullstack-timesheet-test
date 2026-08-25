using System.Net;
using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Timesheet.Api.Contracts;
using Timesheet.Api.Middleware;
using Timesheet.Domain;
using Timesheet.Domain.Common;
using Timesheet.Application.Common.Errors;

namespace Timesheet.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task ValidationException_Returns400_WithValidationErrorCode()
    {
        var failures = new List<ValidationFailure>
        {
            new("Hours", "Количество часов должно быть больше 0")
        };
        var exception = new ValidationException(failures);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error.Should().NotBeNull();
        error!.Code.Should().Be(ErrorCodes.ValidationError);
        error.Message.Should().Contain("Количество часов должно быть больше 0");
    }

    [Fact]
    public async Task BusinessException_PeriodClosed_Returns409()
    {
        var exception = new BusinessException(ErrorCodes.PeriodClosed, ErrorMessages.PeriodClosed);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.Conflict);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error!.Code.Should().Be(ErrorCodes.PeriodClosed);
        error.Message.Should().Be(ErrorMessages.PeriodClosed);
    }

    [Fact]
    public async Task BusinessException_ConcurrencyConflict_Returns409()
    {
        var exception = new BusinessException(ErrorCodes.ConcurrencyConflict, ErrorMessages.ConcurrencyConflict);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.Conflict);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error!.Code.Should().Be(ErrorCodes.ConcurrencyConflict);
    }

    [Fact]
    public async Task BusinessException_NotFound_Returns404()
    {
        var exception = new BusinessException(ErrorCodes.TimeEntryNotFound, ErrorMessages.TimeEntryNotFound);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.NotFound);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error!.Code.Should().Be(ErrorCodes.TimeEntryNotFound);
    }

    [Fact]
    public async Task BusinessException_EmployeeNotFound_Returns404()
    {
        var exception = new BusinessException(ErrorCodes.EmployeeNotFound, ErrorMessages.EmployeeNotFound);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BusinessException_ProjectNotFound_Returns404()
    {
        var exception = new BusinessException(ErrorCodes.ProjectNotFound, ErrorMessages.ProjectNotFound);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task BusinessException_DailyLimit_Returns400()
    {
        var exception = new BusinessException(ErrorCodes.DailyLimitExceeded, ErrorMessages.DailyLimitExceeded);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error!.Code.Should().Be(ErrorCodes.DailyLimitExceeded);
    }

    [Fact]
    public async Task BusinessException_InvalidHours_Returns400()
    {
        var exception = new BusinessException(DomainErrorCodes.InvalidHours, DomainErrorMessages.InvalidHours);

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UnknownException_Returns500_GenericRussianMessage()
    {
        var exception = new InvalidOperationException("secret internal details");

        var (statusCode, body) = await InvokeMiddleware(exception);

        statusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        var error = JsonSerializer.Deserialize<ErrorResponse>(body, JsonOptions);
        error!.Code.Should().Be("INTERNAL_ERROR");
        error.Message.Should().NotContain("secret internal details");
        error.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ContentType_IsApplicationJson()
    {
        var exception = new InvalidOperationException("test");

        var (_, _, contentType) = await InvokeMiddlewareFull(exception);

        contentType.Should().Contain("application/json");
    }

    [Fact]
    public async Task NoException_CallsNext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            async (ctx) =>
            {
                ctx.Response.StatusCode = 200;
                await ctx.Response.WriteAsync("OK");
            },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(200);
    }

    private static async Task<(int StatusCode, string Body)> InvokeMiddleware(Exception exception)
    {
        var (statusCode, body, _) = await InvokeMiddlewareFull(exception);
        return (statusCode, body);
    }

    private static async Task<(int StatusCode, string Body, string ContentType)> InvokeMiddlewareFull(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body, context.Response.ContentType!);
    }
}
