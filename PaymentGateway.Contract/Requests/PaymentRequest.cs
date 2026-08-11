using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PaymentGateway.Contract.Requests
{
    public class PaymentRequest
    {
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; } = string.Empty;
        [JsonPropertyName("expiry_month")]
        public int ExpiryMonth { get; set; }
        [JsonPropertyName("expiry_year")]
        public int ExpiryYear { get; set; }
        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CVV { get; set; } = string.Empty;

        public string ExpiryDate { get { return $"{ExpiryMonth:00}/{ExpiryYear}"; } }
    }
}
