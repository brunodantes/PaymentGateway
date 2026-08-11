using PaymentGateway.Infrastructure.BankSimulator.Models;
using Refit;

namespace PaymentGateway.Infrastructure.BankSimulator.Rest;

public interface IBankSimulatorApi
{
    [Post("/payments")]
    Task<ApiResponse<BankResponseModel>> Publish(BankRequestModel bankRequestModel);
}
