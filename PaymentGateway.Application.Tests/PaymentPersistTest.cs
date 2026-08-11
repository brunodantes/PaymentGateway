using PaymentGateway.Application.Services;
using PaymentGateway.Contract.Requests;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;

namespace PaymentGateway.Application.Tests;
public class PaymentPersistTest
{
    private readonly BankSimulatorFake _bankSimulatorFake;
    private readonly PaymentGatewayService _sut;

    public PaymentPersistTest()
    {
        _bankSimulatorFake = new BankSimulatorFake();

        _sut = new PaymentGatewayService(new PaymentRepositoryFake(),
            new Mock<IValidator<PaymentRequest>>().Object, 
            _bankSimulatorFake, 
            new Mock<ILogger<PaymentGatewayService>>().Object);
    }
    
    [Fact]
    public async Task TestRequest_AddNewPayment()
    {
        //arrange
        var payment = new PaymentRequest()
        {
            Amount = 1000m,
            CardNumber = "465654564545447",
            Currency = "USD",
            CVV = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2030
        };
        
        var bankModel = PaymentGatewayService.CreateToBankSimulatorModel(payment);
        var bankResponse = await _bankSimulatorFake.ExecuteBankValidation(bankModel);

        //act
        var paymentResponse = await _sut.InsertPaymentToDatabase(payment, bankResponse);
        var paymentDetails = await _sut.GetPaymentDetails(paymentResponse.Id);

        //assert
        Assert.NotNull(paymentDetails);
        Assert.Equal(paymentResponse.Id, paymentDetails.Id);
    }
}
