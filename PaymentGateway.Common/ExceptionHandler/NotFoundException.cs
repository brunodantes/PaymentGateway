namespace PaymentGateway.Common.ExceptionHandler;

public sealed class NotFoundException(string resourceName, string id) : DomainException($"{resourceName} with id '{id}' was not found.")
{
    public string ResourceName { get; } = resourceName;
    public string ResourceId { get; } = id;
}
