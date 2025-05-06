namespace PaymentGateway.Api.Exceptions;

public class BankServiceUnavailableException : Exception
{
    public BankServiceUnavailableException()
        : base(ExceptionMessages.BANK_SERVICE_UNAVAILABLE)
    {
    }
}
