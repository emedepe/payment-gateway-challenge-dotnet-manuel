namespace PaymentGateway.Api.Constants;

public static class Currencies
{
    public static readonly HashSet<string> VALID_CURRENCIES = new() 
    { 
        "EUR",
        "GBP",          
        "JPY" 
    };
}
