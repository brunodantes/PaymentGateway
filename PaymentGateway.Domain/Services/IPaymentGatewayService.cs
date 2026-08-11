using PaymentGateway.Contract.Requests;
using PaymentGateway.Entities.Models;

namespace PaymentGateway.Domain.Services;
public interface IPaymentGatewayService
{
    Task<PaymentModel> AddPayment(PaymentRequest paymentRequest);
    Task<PaymentModel> GetPaymentDetails(Guid paymentDetail);
}
