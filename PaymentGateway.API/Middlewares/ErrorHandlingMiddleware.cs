using PaymentGateway.API.Middlewares.Models;
using PaymentGateway.Common.ExceptionHandler;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace PaymentGateway.API.Middlewares;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "Exception thrown after the response has already started. TraceId: {TraceId}",
                context.TraceIdentifier);

            throw new InvalidOperationException("Exception occurred after the response has started.", exception);
        }

        var traceId = context.TraceIdentifier;
        var (statusCode, error) = MapExceptionToError(exception, traceId);

        _logger.LogError(
            exception,
            "Unhandled exception caught by middleware. StatusCode: {StatusCode}, TraceId: {TraceId}",
            statusCode,
            traceId);

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var json = JsonSerializer.Serialize(
            error,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode statusCode, ApiError error) MapExceptionToError(Exception exception, string traceId)
    {
        return exception switch
        {
            NotFoundException notFound => (
                HttpStatusCode.NotFound,
                new ApiError(
                    Code: $"{notFound.ResourceName.ToUpperInvariant()}_NOT_FOUND",
                    Message: notFound.Message,
                    TraceId: traceId)
            ),

            DomainValidationException validation => (
                HttpStatusCode.BadRequest,
                new ApiError(
                    Code: "VALIDATION_ERROR",
                    Message: validation.Message,
                    TraceId: traceId)
            ),

            ValidationException fv => (
                HttpStatusCode.BadRequest,
                new ApiError(
                    Code: "VALIDATION_ERROR",
                    Message: string.Join(" | ", fv.Errors.Select(e => e.ErrorMessage)),
                    TraceId: traceId)
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                new ApiError(
                    Code: "UNEXPECTED_ERROR",
                    Message: "An unexpected error has occurred.",
                    TraceId: traceId)
            )
        };
    }
}
