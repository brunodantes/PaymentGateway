using PaymentGateway.Contract.Enum;
using System.Text.Json.Serialization;

namespace PaymentGateway.Entities.Models;

public class PaymentModel
{
    public Guid Id { get; set; }
    public PaymentStatusEnum PaymentStatusEnum { get; set; }
    
    [JsonPropertyName("expiry_month")]
    public int ExpiryMonth { get; set; }
    [JsonPropertyName("expiry_year")]
    public int ExpiryYear { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    [JsonIgnore]
    public string CardNumber { get; set; } = string.Empty;
    [JsonIgnore]
    public Guid AuthorizationCode { get; set; }

    [JsonPropertyName("card_number")]
    public string MaskedCardNumber { get { return CardNumber[^4..]; } }
}
