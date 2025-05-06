using System.Net;
using System.Net.Http.Json;

using Microsoft.Extensions.Logging;

using Moq;
using Moq.Protected;

using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Factories.Contracts;
using PaymentGateway.Api.Models.Responses.Bank;

namespace PaymentGateway.Api.Tests.Services;

public class PaymentsProcessingServiceTests
{
    private readonly Mock<IBankHttpClientFactory> _httpBankClientFactoryMock;
    private readonly Mock<ILogger<PaymentsProcessingService>> _loggerMock;
    private readonly Mock<IPaymentsRepository> _paymentsRepositoryMock;
    private readonly HttpClient _httpClient;

    public PaymentsProcessingServiceTests()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://test-url.com/")
        };

        _httpBankClientFactoryMock = new Mock<IBankHttpClientFactory>();
        _httpBankClientFactoryMock.Setup(f => f.CreateClient()).Returns(_httpClient);

        _loggerMock = new Mock<ILogger<PaymentsProcessingService>>();

        _paymentsRepositoryMock = new Mock<IPaymentsRepository>();
    }

    [Fact]
    public async Task ProcessPayment_SuccessfulPayment_ReturnsAuthorizedResponse()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var authorizationResponse = new AuthorizationResponse
        {
            Authorized = true,
            AuthorizationCode = Guid.NewGuid().ToString()
        };

        var handlerMock = MockHttpResponse(HttpStatusCode.OK, authorizationResponse);

        var service = new PaymentsProcessingService(
            _httpBankClientFactoryMock.Object,
            _paymentsRepositoryMock.Object,
            _loggerMock.Object);

        // Act
        var response = await service.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(PaymentStatus.Authorized, response.Status);
        AssertPropertiesMapping(response, paymentRequest);

        _paymentsRepositoryMock.Verify(r => r.Add(It.IsAny<PostPaymentResponse>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_UnsuccessfulPayment_ReturnsDeclinedResponse()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var authorizationResponse = new AuthorizationResponse
        {
            Authorized = false,
            AuthorizationCode = ""
        };

        var handlerMock = MockHttpResponse(HttpStatusCode.OK, authorizationResponse);

        var service = new PaymentsProcessingService(
            _httpBankClientFactoryMock.Object,
            _paymentsRepositoryMock.Object,
            _loggerMock.Object);

        // Act
        var response = await service.ProcessPaymentAsync(paymentRequest);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(PaymentStatus.Declined, response.Status);
        AssertPropertiesMapping(response, paymentRequest);

        _paymentsRepositoryMock.Verify(r => r.Add(It.IsAny<PostPaymentResponse>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_BankReturnsServiceUnavailable_ReturnsBankServiceUnavailableException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var handlerMock = MockHttpResponse(HttpStatusCode.ServiceUnavailable, null);

        var service = new PaymentsProcessingService(
            _httpBankClientFactoryMock.Object,
            _paymentsRepositoryMock.Object, 
            _loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => service.ProcessPaymentAsync(paymentRequest));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<BankServiceUnavailableException>(exception);

        _paymentsRepositoryMock.Verify(r => r.Add(It.IsAny<PostPaymentResponse>()), Times.Never);

        Helpers.AssertLogger(
            _loggerMock,
            LogLevel.Error,
            ((int)HttpStatusCode.ServiceUnavailable).ToString(),
            1);
    }

    [Fact]
    public async Task ProcessPayment_BankReturnsInternalServerErrorException_ReturnsBankInternalException()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var handlerMock = MockHttpResponse(HttpStatusCode.InternalServerError, null);

        var service = new PaymentsProcessingService(
            _httpBankClientFactoryMock.Object,
            _paymentsRepositoryMock.Object, 
            _loggerMock.Object);

        // Act
        var exception = await Record.ExceptionAsync(() => service.ProcessPaymentAsync(paymentRequest));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<BankInternalException>(exception);

        _paymentsRepositoryMock.Verify(r => r.Add(It.IsAny<PostPaymentResponse>()), Times.Never);

        Helpers.AssertLogger(
            _loggerMock,
            LogLevel.Error,
            ((int)HttpStatusCode.InternalServerError).ToString(),
            1);
    }

    private Mock<HttpMessageHandler> MockHttpResponse(HttpStatusCode statusCode, object content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content != null
                    ? JsonContent.Create(content)
                    : null
            });

        _httpBankClientFactoryMock.Setup(f => f.CreateClient()).Returns(new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://test-url.com/")
        });

        return handlerMock;
    }

    private void AssertPropertiesMapping(PostPaymentResponse response, PostPaymentRequest request)
    {
        Assert.Equal(request.CardNumber[^4..], response.CardNumberLastFour.ToString());
        Assert.Equal(request.ExpiryMonth, response.ExpiryMonth);
        Assert.Equal(request.ExpiryYear, response.ExpiryYear);
        Assert.Equal(request.Currency, response.Currency);
        Assert.Equal(request.Amount, response.Amount);
    }
}
