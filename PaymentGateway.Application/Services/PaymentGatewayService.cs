using PaymentGateway.Common.ExceptionHandler;
using PaymentGateway.Contract.Enum;
using PaymentGateway.Contract.Requests;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Services;
using PaymentGateway.Entities.Models;
using PaymentGateway.Infrastructure.BankSimulator.Domain;
using PaymentGateway.Infrastructure.BankSimulator.Models;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Refit;

namespace PaymentGateway.Application.Services;
public class PaymentGatewayService(IPaymentRepository paymentRepository, 
    IValidator<PaymentRequest> validator,
    IBankSimulatorService bankSimulatorClient,
    ILogger<PaymentGatewayService> logger) : IPaymentGatewayService
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;
    private readonly IValidator<PaymentRequest> _validator = validator;
    private readonly IBankSimulatorService _bankSimulatorClient = bankSimulatorClient;
    private readonly ILogger<PaymentGatewayService> _logger = logger;

    public async Task<PaymentModel> AddPayment(PaymentRequest paymentRequest)
    {
        try
        {
            var validationResult = await ValidateRequest(paymentRequest);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var bankModel = CreateToBankSimulatorModel(paymentRequest);

            var bankSimulatorResponse = await ExecuteBankSimulator(bankModel);

            var response = await InsertPaymentToDatabase(paymentRequest, bankSimulatorResponse);

            return response;
        }
        catch (ValidationException validationEx)
        {
            _logger.LogWarning("An error occurred during the validation process. Message: {validationExMessage}", validationEx.Message);
            throw;
        }
        catch(ApiException apiEx)
        {
            _logger.LogError(apiEx, "An unexpected error occurred during the banking validation : {rawMessage}", apiEx.Content);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during the payment creation process");
            throw;
        }
        
    }

    public async Task<PaymentModel> InsertPaymentToDatabase(PaymentRequest paymentRequest, BankResponseModel bankResponseModel)
    {
        _logger.LogInformation("Adding payment request to the database");

        var paymentModel = CreatePaymentModel(paymentRequest, bankResponseModel);

        _logger.LogInformation("Inserting ID {id} with authorization code {AuthorizationCode}.",
            paymentModel.Id, paymentModel.AuthorizationCode);

        await _paymentRepository.AddPayment(paymentModel);

        return paymentModel;
    }

    private static PaymentModel CreatePaymentModel(PaymentRequest paymentRequest, BankResponseModel bankResponseModel)
    {
        var authorizationCode = string.IsNullOrEmpty(bankResponseModel.AuthorizationCode) ? Guid.Empty : new Guid(bankResponseModel.AuthorizationCode);

        return new PaymentModel()
        {
            PaymentStatusEnum = bankResponseModel.Authorized ? PaymentStatusEnum.Authorized : PaymentStatusEnum.Declined,
            Id = Guid.NewGuid(),
            AuthorizationCode = authorizationCode,
            Amount = paymentRequest.Amount,
            CardNumber = paymentRequest.CardNumber,
            Currency = paymentRequest.Currency,
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear
        };
    }

    private async Task<ValidationResult> ValidateRequest(PaymentRequest paymentRequest)
    {
        _logger.LogInformation("Initializing request validation");

        return await _validator.ValidateAsync(paymentRequest);
    }

    private async Task<BankResponseModel> ExecuteBankSimulator(BankRequestModel bankRequestModel)
    {
        _logger.LogInformation("Execute bank simulation and return result");

        var result = await _bankSimulatorClient.ExecuteBankValidation(bankRequestModel);

        return result;
    }

    public static BankRequestModel CreateToBankSimulatorModel(PaymentRequest paymentRequest)
    {
        return new BankRequestModel()
        {
            Amount = paymentRequest.Amount,
            CardNumber = paymentRequest.CardNumber,
            Currency = paymentRequest.Currency,
            CVV = paymentRequest.CVV,
            ExpiryDate = paymentRequest.ExpiryDate,
        };
    }

    public async Task<PaymentModel> GetPaymentDetails(Guid paymentId)
    {
        _logger.LogInformation("Retrieving payment details from the database");

        var result = await _paymentRepository.GetPayment(paymentId) 
            ?? throw new NotFoundException("PAYMENT", paymentId.ToString()); ;

        return result;
    }
}
