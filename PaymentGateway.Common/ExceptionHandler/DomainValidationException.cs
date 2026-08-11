namespace PaymentGateway.Common.ExceptionHandler;

public sealed class DomainValidationException(string message) : DomainException(message)
{
}