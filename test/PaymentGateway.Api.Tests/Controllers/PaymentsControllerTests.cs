using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Exceptions;

namespace PaymentGateway.Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Random _random = new();
    
    [Fact]
    public async Task GetPaymentAsync_PaymentExists_ReturnsOkObjectResult()
    {
        // Arrange
        var payment = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            ExpiryYear = _random.Next(2023, 2030),
            ExpiryMonth = _random.Next(1, 12),
            Amount = _random.Next(1, 10000),
            CardNumberLastFour = _random.Next(1111, 9999),
            Currency = "GBP"
        };

        var factory = CreateWebApplicationFactory(services =>
        {
            services.AddSingleton<IPaymentsRepository>(provider =>
            {
                var paymentsRepository = new PaymentsRepository();
                paymentsRepository.Add(payment);
                return paymentsRepository;
            });

            services.AddScoped(provider =>
            { 
                var paymentProcessingServiceMock = new Mock<IPaymentsProcessingService>();
                return paymentProcessingServiceMock.Object;
            });

            services.AddSingleton(provider =>
            {
                var paymentsValidationServiceMock = new Mock<IPaymentsValidationService>();
                return paymentsValidationServiceMock.Object;
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{payment.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task GetPaymentAsync_PaymentDoesNotExist_Returns404()
    {
        // Arrange
        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        var client = webApplicationFactory.CreateClient();
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostPaymentAsync_SuccessfulPayment_ReturnsOkObjectResult()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var paymentResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Authorized,
            CardNumberLastFour = int.Parse(paymentRequest.CardNumber[^4..]),
            ExpiryMonth = paymentRequest.ExpiryMonth,
            ExpiryYear = paymentRequest.ExpiryYear,
            Currency = paymentRequest.Currency,
            Amount = paymentRequest.Amount
        };

        var factory = CreateWebApplicationFactory(services =>
        {
            services.AddSingleton<IPaymentsRepository>(provider =>
            {
                var paymentsRepository = new PaymentsRepository();
                return paymentsRepository;
            });

            services.AddScoped(provider =>
            { 
                var paymentProcessingServiceMock = new Mock<IPaymentsProcessingService>();
                paymentProcessingServiceMock
                    .Setup(s => s.ProcessPaymentAsync(paymentRequest))
                    .ReturnsAsync(paymentResponse);
                return paymentProcessingServiceMock.Object;
            });

            services.AddSingleton(provider =>
            {
                var paymentsValidationServiceMock = new Mock<IPaymentsValidationService>();
                paymentsValidationServiceMock
                    .Setup(s => s.ValidatePayment(It.IsAny<PostPaymentRequest>()))
                    .Returns(paymentRequest);
                return paymentsValidationServiceMock.Object;
            });
        });

        var client = factory.CreateClient();
        
        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/", paymentRequest);

        var paymentContent = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentContent);
    }

    [Fact]
    public async Task PostPaymentAsync_PaymentProcessingError_ReturnsBadGateway()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var factory = CreateWebApplicationFactory(services =>
        {
            services.AddSingleton<IPaymentsRepository>(provider =>
            {
                var paymentsRepository = new PaymentsRepository();
                return paymentsRepository;
            });

            services.AddScoped(provider =>
            { 
                var paymentProcessingServiceMock = new Mock<IPaymentsProcessingService>();
                paymentProcessingServiceMock
                .Setup(s => s.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>()))
                    .Throws(new BankInternalException());

                return paymentProcessingServiceMock.Object;
            });

            services.AddSingleton(provider =>
            {
                var paymentsValidationServiceMock = new Mock<IPaymentsValidationService>();
                paymentsValidationServiceMock
                    .Setup(s => s.ValidatePayment(paymentRequest))
                    .Returns(paymentRequest);
                return paymentsValidationServiceMock.Object;
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/", paymentRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Fact]
    public async Task PostPaymentAsync_PaymentProcessingServiceUnavailable_ReturnsServiceUnavailable()
    {
        // Arrange
        var paymentRequest = Helpers.CreateValidPaymentRequest();

        var factory = CreateWebApplicationFactory(services =>
        {
            services.AddSingleton<IPaymentsRepository>(provider =>
            {
                var paymentsRepository = new PaymentsRepository();
                return paymentsRepository;
            });

            services.AddScoped(provider =>
            { 
                var paymentProcessingServiceMock = new Mock<IPaymentsProcessingService>();
                paymentProcessingServiceMock
                .Setup(s => s.ProcessPaymentAsync(It.IsAny<PostPaymentRequest>()))
                    .Throws(new BankServiceUnavailableException());

                return paymentProcessingServiceMock.Object;
            });

            services.AddSingleton(provider =>
            {
                var paymentsValidationServiceMock = new Mock<IPaymentsValidationService>();
                paymentsValidationServiceMock
                    .Setup(s => s.ValidatePayment(paymentRequest))
                    .Returns(paymentRequest);
                return paymentsValidationServiceMock.Object;
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/", paymentRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task PostPaymentAsync_ValidationFailure_ReturnsBadRequest()
    {
        // Arrange
        var factory = CreateWebApplicationFactory(services =>
        {
            services.AddSingleton<IPaymentsRepository>(provider =>
            {
                var paymentsRepository = new PaymentsRepository();
                return paymentsRepository;
            });

            services.AddScoped(provider =>
            { 
                var paymentProcessingServiceMock = new Mock<IPaymentsProcessingService>();
                return paymentProcessingServiceMock.Object;
            });

            services.AddSingleton(provider =>
            {
                var paymentsValidationServiceMock = new Mock<IPaymentsValidationService>();
                paymentsValidationServiceMock
                    .Setup(s => s.ValidatePayment(It.IsAny<PostPaymentRequest>()))
                    .Throws(new PaymentRequestValidationException([]));
                return paymentsValidationServiceMock.Object;
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/api/Payments/", new PostPaymentRequest());
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private WebApplicationFactory<PaymentsController> CreateWebApplicationFactory(Action<IServiceCollection> configureServices)
    {
        return new WebApplicationFactory<PaymentsController>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(s => 
                        s.ServiceType == typeof(IPaymentsRepository));

                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                
                configureServices(services);
            });
        });
    }
}
