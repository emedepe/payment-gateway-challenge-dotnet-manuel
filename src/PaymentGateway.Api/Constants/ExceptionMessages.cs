namespace PaymentGateway.Api.Constants;

public static class ExceptionMessages
{
    public const string BANK_INTERNAL = "Payment processor is experiencing issues.";
    public const string BANK_SERVICE_UNAVAILABLE = "Payment processor is unavailable.";

    public const string PAYMENT_REQUEST = "Payment request validation failed.";
}
