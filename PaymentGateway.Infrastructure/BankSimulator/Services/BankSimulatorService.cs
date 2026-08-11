using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net;
using PaymentGateway.Infrastructure.BankSimulator.Domain;
using PaymentGateway.Infrastructure.BankSimulator.Models;
using PaymentGateway.Infrastructure.BankSimulator.Rest;

namespace PaymentGateway.Infrastructure.BankSimulator.Services;
public class BankSimulatorService : IBankSimulatorService
{
    private readonly IBankSimulatorApi _bankSimulatorRest;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<BankSimulatorService> _logger;
    private readonly int _maxRetry = 3;

    public BankSimulatorService(IBankSimulatorApi bankSimulatorRest, ILogger<BankSimulatorService> logger)
    {
        _logger = logger;
        _bankSimulatorRest = bankSimulatorRest;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _maxRetry,
                sleepDurationProvider: attempt => { 
                    var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                    var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 1000));

                    return baseDelay + jitter;
                },
                onRetry: OnRetry);
    }

    public async Task<BankResponseModel> ExecuteBankValidation(BankRequestModel bankRequestModel)
    {
        _logger.LogInformation("Initializing banking validation REST process");

        var context = new Context();

        var result = await _retryPolicy.ExecuteAsync(async () => await _bankSimulatorRest.Publish(bankRequestModel));

        if (result.Content is null || result.StatusCode != HttpStatusCode.OK)
        {
            throw result.Error ?? new Exception("An error occurred during banking validation process");
        }

        _logger.LogInformation("Bank Validation result - Authorized : {Authorized} AuthorizationCode : {authorizationCode}",
            result.Content.Authorized, result.Content.AuthorizationCode);

        return result.Content;
    }

    private void OnRetry(Exception exception, TimeSpan delay, int retry, Context context)
    {
        if (retry == _maxRetry)
        {
            _logger.LogError(exception,"BankSimulator publish failed after {RetryCount} attempts. Last delay: {DelaySeconds}s",
                retry, delay.TotalSeconds);
        }
        else
        {
            _logger.LogWarning(exception, "Retry {RetryCount}/{MaxRetry}. Next attempt in {DelaySeconds}s.",
                retry, _maxRetry, delay.TotalSeconds);
        }
    }
}
