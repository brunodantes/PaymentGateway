using PaymentGateway.Infrastructure.BankSimulator.Models;

namespace PaymentGateway.Infrastructure.BankSimulator.Domain;

public interface IBankSimulatorService
{
    Task<BankResponseModel> ExecuteBankValidation(BankRequestModel bankRequestModel);
}
