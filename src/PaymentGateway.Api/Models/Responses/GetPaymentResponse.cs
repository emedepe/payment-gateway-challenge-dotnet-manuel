using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses;

public class GetPaymentResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("status")]
    public PaymentStatus Status { get; set; }

    [JsonPropertyName("card_number_last_four")]
    public int CardNumberLastFour { get; set; }

    [JsonPropertyName("expiry_month")]
    public int ExpiryMonth { get; set; }

    [JsonPropertyName("expiry_year")]
    public int ExpiryYear { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("amount")]
    public int Amount { get; set; }
}