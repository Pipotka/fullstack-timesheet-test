using System.Net;
using System.Text.Json;
using FluentValidation;
using Timesheet.Api.Contracts;
using Timesheet.Application.Common.Errors;
using Timesheet.Domain;
using Timesheet.Domain.Common;

namespace Timesheet.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, context.RequestAborted);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (statusCode, errorResponse) = MapException(exception);

        if (statusCode >= (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Необработанное исключение: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(errorResponse, JsonOptions);
        await context.Response.WriteAsync(json, ct);
    }

    private static (int StatusCode, ErrorResponse Response) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                ((int)HttpStatusCode.BadRequest,
                 new ErrorResponse(
                     ErrorCodes.ValidationError,
                     string.Join("; ", validationException.Errors.Select(e => e.ErrorMessage)))),

            BusinessException businessException => MapBusinessException(businessException),

            _ => ((int)HttpStatusCode.InternalServerError,
                  new ErrorResponse("INTERNAL_ERROR", "Внутренняя ошибка сервера"))
        };
    }

    private static (int StatusCode, ErrorResponse Response) MapBusinessException(BusinessException ex)
    {
        return ex.Code switch
        {
            ErrorCodes.PeriodClosed =>
                ((int)HttpStatusCode.Conflict, new ErrorResponse(ex.Code, ex.Message)),

            ErrorCodes.ConcurrencyConflict =>
                ((int)HttpStatusCode.Conflict, new ErrorResponse(ex.Code, ex.Message)),

            ErrorCodes.TimeEntryNotFound =>
                ((int)HttpStatusCode.NotFound, new ErrorResponse(ex.Code, ex.Message)),

            ErrorCodes.EmployeeNotFound =>
                ((int)HttpStatusCode.NotFound, new ErrorResponse(ex.Code, ex.Message)),

            ErrorCodes.ProjectNotFound =>
                ((int)HttpStatusCode.NotFound, new ErrorResponse(ex.Code, ex.Message)),

            _ => ((int)HttpStatusCode.BadRequest, new ErrorResponse(ex.Code, ex.Message))
        };
    }
}
