using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Exceptions;

public class PaymentRequestValidationException : ValidationException
{
    public IEnumerable<string> Errors { get; }

    public PaymentStatus Status { get; } = PaymentStatus.Rejected;

    public PaymentRequestValidationException(IEnumerable<string> errors)
        : base(ExceptionMessages.PAYMENT_REQUEST)
    {
        Errors = errors;
    }
}
