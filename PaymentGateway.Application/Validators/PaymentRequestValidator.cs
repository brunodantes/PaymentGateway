using PaymentGateway.Contract.Requests;
using FluentValidation;

namespace PaymentGateway.Application.Validators;

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    private readonly string[] _allowedCurrencies = ["USD", "EUR", "GBP"];

    public PaymentRequestValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("Card number is required.")
            .Matches("^[0-9]{14,19}$").WithMessage("Card number must contain between 14 and 19 numeric digits.");

        RuleFor(x => x.CardNumber)
            .Must(NotEndWithZero)
            .WithMessage("Card numbers ending with 0 are not allowed.");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12).WithMessage("Expiry month must be between 1 and 12.");

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year).WithMessage("Expiry year must be the current year or later.");

        RuleFor(x => x)
            .Must(BeAValidExpiryDate)
            .WithMessage("The card expiry date must be in the future.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Must(c => _allowedCurrencies.Contains(c.ToUpper())).WithMessage($"Currency must be one of the following: {string.Join(", ", _allowedCurrencies)}.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.CVV)
            .NotEmpty().WithMessage("CVV is required.")
            .Matches("^[0-9]{3,4}$").WithMessage("CVV must contain 3 or 4 digits.");
    }

    public static bool BeAValidExpiryDate(PaymentRequest request)
    {
        try
        {
            var lastDayOfMonth = DateTime.DaysInMonth(request.ExpiryYear, request.ExpiryMonth);
            var expiryDate = new DateTime(request.ExpiryYear, request.ExpiryMonth, lastDayOfMonth, 23, 59, 59);

            return expiryDate > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private bool NotEndWithZero(string? number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return false;

        var lastChar = number[^1];
        return lastChar != '0';
    }
}