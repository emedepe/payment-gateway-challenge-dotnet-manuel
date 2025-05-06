using PaymentGateway.Api.Exceptions;

namespace PaymentGateway.Api.Services;

public class PaymentsValidationService : IPaymentsValidationService
{
    private readonly ILogger<PaymentsValidationService> _logger;

    public PaymentsValidationService(ILogger<PaymentsValidationService> logger) 
    {
        _logger = logger;
    }

    public PostPaymentRequest ValidatePayment(PostPaymentRequest request)
    {
        request.Currency = request.Currency?.Trim() ?? "";
        request.CardNumber = request.CardNumber?.Trim() ?? "";

        var errors = new List<string>();

        // Validate card number
        if (string.IsNullOrWhiteSpace(request.CardNumber))
        {
            errors.Add(ValidationMessages.PaymentRequest.CARD_NUMBER_REQUIRED);
        }
        else
        {
            if (request.CardNumber.Length < 14 || request.CardNumber.Length > 19)
            {
                errors.Add(ValidationMessages.PaymentRequest.CARD_NUMBER_LENGTH);
            }
            if (!request.CardNumber.All(char.IsDigit))
            {
                errors.Add(ValidationMessages.PaymentRequest.CARD_NUMBER_DIGITS_ONLY);
            }
        }

        // Validate expiry month
        if (request.ExpiryMonth < 1 || request.ExpiryMonth > 12)
        {
            errors.Add(ValidationMessages.PaymentRequest.EXPIRY_MONTH_RANGE);
        }

        // Validate expiry year and date is in the future
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;
        if (request.ExpiryYear < currentYear || 
            (request.ExpiryYear == currentYear && request.ExpiryMonth < currentMonth))
        {
            errors.Add(ValidationMessages.PaymentRequest.EXPIRY_DATE_IN_FUTURE);
        }

        // Validate currency
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors.Add(ValidationMessages.PaymentRequest.CURRENCY_REQUIRED);
        }
        else if (request.Currency.Length != 3)
        {
            errors.Add(ValidationMessages.PaymentRequest.CURRENCY_LENGTH);
        }
        else if (!Currencies.VALID_CURRENCIES.Contains(request.Currency.ToUpperInvariant()))
        {
            errors.Add(string.Format(
                ValidationMessages.PaymentRequest.CURRENCY_INVALID, 
                string.Join(", ", Currencies.VALID_CURRENCIES)));
        }

        // Validate amount
        if (request.Amount <= 0)
        {
            errors.Add(ValidationMessages.PaymentRequest.AMOUNT_POSITIVE);
        }

        // Validate CVV
        var cvvLength = request.Cvv.ToString().Length;
        if (cvvLength < 3 || cvvLength > 4)
        {
            errors.Add(ValidationMessages.PaymentRequest.CVV_LENGTH);
        }

        if (errors.Any())
        {
            _logger.LogWarning(string.Format(ValidationMessages.PaymentRequest.LOG_WARNING, 
                string.Join(", ", errors)));
            
            throw new PaymentRequestValidationException(errors);
        }

        return request;
    }
}
