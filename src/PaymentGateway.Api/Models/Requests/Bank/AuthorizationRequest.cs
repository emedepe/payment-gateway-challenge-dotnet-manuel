using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Requests.Bank;

public class AuthorizationRequest
{
    [JsonPropertyName("card_number")]
    public string CardNumber { get; set; } = default!;

    [JsonPropertyName("expiry_date")]
    public string ExpiryDate { get; set; } = default!;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("cvv")]
    public string Cvv { get; set; } = default!;
}
