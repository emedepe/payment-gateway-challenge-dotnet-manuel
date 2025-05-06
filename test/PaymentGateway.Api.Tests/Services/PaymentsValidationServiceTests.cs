using Microsoft.Extensions.Logging;

using Moq;

using PaymentGateway.Api.Exceptions;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentsValidationServiceTests
{
    private readonly Mock<ILogger<PaymentsValidationService>> _loggerMock;
    private readonly PaymentsValidationService _validationService;

    public PaymentsValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<PaymentsValidationService>>();
        _validationService = new PaymentsValidationService(_loggerMock.Object);
    }

    [Fact]
    public void ValidatePaymentRequest_ValidRequest_DoesNotThrowException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        // Act
        var exception = Record.Exception(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePaymentRequest_ValidRequest_ReturnsPaymentRequest()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        // Act
        var response = _validationService.ValidatePayment(paymentRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(paymentRequest.CardNumber, response.CardNumber);
        Assert.Equal(paymentRequest.ExpiryMonth, response.ExpiryMonth);
        Assert.Equal(paymentRequest.ExpiryYear, response.ExpiryYear);
        Assert.Equal(paymentRequest.Currency, response.Currency);
        Assert.Equal(paymentRequest.Amount, paymentRequest.Amount);
        Assert.Equal(paymentRequest.Cvv, paymentRequest.Cvv);
    }

    [Fact]
    public void ValidatePaymentRequest_MissingCardNumber_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.CardNumber = null;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CARD_NUMBER_REQUIRED, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.CARD_NUMBER_REQUIRED, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidCardNumberLength_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.CardNumber = "123456";

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CARD_NUMBER_LENGTH, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.CARD_NUMBER_LENGTH,
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidCardNumberFormat_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.CardNumber = "ABC123452999828763";

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CARD_NUMBER_DIGITS_ONLY, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock,
            LogLevel.Warning,
            ValidationMessages.PaymentRequest.CARD_NUMBER_DIGITS_ONLY,
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidExpiryMonth_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.ExpiryMonth = 13;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.EXPIRY_MONTH_RANGE, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.EXPIRY_MONTH_RANGE, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_ExpiredCard_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.ExpiryYear = DateTime.UtcNow.Year - 1;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.EXPIRY_DATE_IN_FUTURE, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.EXPIRY_DATE_IN_FUTURE, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_MissingCurrency_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.Currency = null;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CURRENCY_REQUIRED, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.CURRENCY_REQUIRED, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidCurrencyLength_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.Currency = "GBPGBP";

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CURRENCY_LENGTH, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.CURRENCY_LENGTH, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidCurrency_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.Currency = "USD";

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(string.Format(
            ValidationMessages.PaymentRequest.CURRENCY_INVALID, 
            string.Join(", ", Currencies.VALID_CURRENCIES)), exception.Errors);

        Helpers.AssertLogger(
            _loggerMock,
            LogLevel.Warning,
            string.Format(
                ValidationMessages.PaymentRequest.CURRENCY_INVALID,
                string.Join(", ", Currencies.VALID_CURRENCIES)),
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_NegativeAmount_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.Amount = -100;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.AMOUNT_POSITIVE, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.AMOUNT_POSITIVE, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_InvalidCvvLength_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.Cvv = 12;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Contains(ValidationMessages.PaymentRequest.CVV_LENGTH, exception.Errors);

        Helpers.AssertLogger(
            _loggerMock, 
            LogLevel.Warning, 
            ValidationMessages.PaymentRequest.CVV_LENGTH, 
            1);
    }

    [Fact]
    public void ValidatePaymentRequest_MultipleInvalidProperties_ThrowsValidationException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();
        paymentRequest.CardNumber = "123";
        paymentRequest.ExpiryMonth = 13;
        paymentRequest.Currency = null;

        // Act
        var exception = Assert.Throws<PaymentRequestValidationException>(() => _validationService.ValidatePayment(paymentRequest));

        // Assert
        Assert.Contains(ExceptionMessages.PAYMENT_REQUEST, exception.Message);
        Assert.Equal(3, exception.Errors.Count());
        Assert.Contains(ValidationMessages.PaymentRequest.CARD_NUMBER_LENGTH, exception.Errors);
        Assert.Contains(ValidationMessages.PaymentRequest.EXPIRY_MONTH_RANGE, exception.Errors);
        Assert.Contains(ValidationMessages.PaymentRequest.CURRENCY_REQUIRED, exception.Errors);

        var expectedErrors = new[]
        {
            ValidationMessages.PaymentRequest.CARD_NUMBER_LENGTH,
            ValidationMessages.PaymentRequest.EXPIRY_MONTH_RANGE,
            ValidationMessages.PaymentRequest.CURRENCY_REQUIRED
        };

        Helpers.AssertLogger(
            _loggerMock,
            LogLevel.Warning,
            string.Format(
                ValidationMessages.PaymentRequest.LOG_WARNING,
                string.Join(", ", expectedErrors)),
            1);
    }
}
