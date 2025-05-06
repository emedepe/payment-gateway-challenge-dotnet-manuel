namespace PaymentGateway.Api.Constants;

public static class ConfigurationMessages
{
    public const string BANK_BASEURL_REQUIRED = "The configuration value for 'Bank:BaseUrl' is missing or empty.";
    public const string BANK_BASEURL_INVALID = "The configuration value for 'Bank:BaseUrl' is not a valid absolute URI.";
}
