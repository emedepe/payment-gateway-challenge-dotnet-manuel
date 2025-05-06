namespace PaymentGateway.Api.Exceptions;

public class BankInternalException : Exception
{
    public BankInternalException()
        : base(ExceptionMessages.BANK_INTERNAL)
    {
    }
}
