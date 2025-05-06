using System.Text.Json.Serialization;

namespace PaymentGateway.Api.Models.Responses.Bank;

public class AuthorizationResponse
{
    [JsonPropertyName("authorized")]
    public bool Authorized { get; set; }

    [JsonPropertyName("authorization_code")]
    public string AuthorizationCode { get; set; } = default!;
}
