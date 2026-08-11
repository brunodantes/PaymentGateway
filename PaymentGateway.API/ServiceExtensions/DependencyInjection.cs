using PaymentGateway.Application.Services;
using PaymentGateway.Application.Validators;
using PaymentGateway.Data.Respositories;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.Services;
using PaymentGateway.Infrastructure.BankSimulator.Domain;
using PaymentGateway.Infrastructure.BankSimulator.Rest;
using PaymentGateway.Infrastructure.BankSimulator.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Refit;

namespace PaymentGateway.API.ServiceExtensions;

public static class DependencyInjection
{
    internal static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddValidatorsFromAssemblyContaining<PaymentRequestValidator>();
    }

    internal static void AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentRepository, PaymentRepository>();
    }

    internal static void AddInfraestructure(this IServiceCollection services)
    {
        services.AddSingleton<IBankSimulatorService, BankSimulatorService>();
        services.AddRefitClient<IBankSimulatorApi>().ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:8080"));
    }
}
