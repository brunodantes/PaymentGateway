namespace PaymentGateway.API.Middlewares.Models;

public sealed record ApiError(string Code, string Message, string? TraceId = null);