using System.Text.Json.Serialization;

namespace PaymentGateway.Infrastructure.BankSimulator.Models;

public class BankRequestModel
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; } = string.Empty;
    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CVV { get; set; } = string.Empty;
}
