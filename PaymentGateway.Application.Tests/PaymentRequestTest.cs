using PaymentGateway.Application.Validators;
using PaymentGateway.Contract.Requests;

namespace PaymentGateway.Application.Tests;

public class PaymentRequestTest
{
    [Fact]
    public void TestRequest_InvalidCardNumber()
    {
        //arrange
        var request = new PaymentRequest()
        {
            Amount = 1000m,
            CardNumber = "465654564545787878747",
            Currency = "USD",
            CVV = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2026
        };
        PaymentRequestValidator validator = new PaymentRequestValidator();

        //act
        var result = validator.Validate(request);

        //assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void TestRequest_InvalidCurrency()
    {
        //arrange
        var request = new PaymentRequest()
        {
            Amount = 1000m,
            CardNumber = "465654564545447",
            Currency = "USD2",
            CVV = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2026
        };
        PaymentRequestValidator validator = new PaymentRequestValidator();

        //act
        var result = validator.Validate(request);

        //assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void TestRequest_ValidExpiryDate()
    {
        //arrange
        var requestOK = new PaymentRequest()
        {
            Amount = 1000m,
            CardNumber = "465654564545447",
            Currency = "USD",
            CVV = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2030
        };

        var requestNOK = new PaymentRequest()
        {
            Amount = 1000m,
            CardNumber = "465654564545447",
            Currency = "USD",
            CVV = "123",
            ExpiryMonth = 1,
            ExpiryYear = 2012
        };

        //act
        var resultOK = PaymentRequestValidator.BeAValidExpiryDate(requestOK);
        var resultNOK = PaymentRequestValidator.BeAValidExpiryDate(requestNOK);

        //assert
        Assert.True(resultOK);
        Assert.False(resultNOK);
    }


}