using PaymentGateway.Infrastructure.BankSimulator.Domain;
using PaymentGateway.Infrastructure.BankSimulator.Models;

namespace PaymentGateway.Application.Tests;

public class BankSimulatorFake : IBankSimulatorService
{
    public async Task<BankResponseModel> ExecuteBankValidation(BankRequestModel bankRequestModel)
    {
        return await Task.FromResult(new BankResponseModel() { AuthorizationCode = Guid.NewGuid().ToString(), Authorized = true });
    }
}
