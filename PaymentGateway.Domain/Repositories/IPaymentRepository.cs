using PaymentGateway.Entities.Models;

namespace PaymentGateway.Domain.Repositories;

public interface IPaymentRepository
{
    Task AddPayment(PaymentModel paymentRequest);
    Task<PaymentModel?> GetPayment(Guid id);
}
