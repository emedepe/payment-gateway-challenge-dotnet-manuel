using PaymentGateway.Api.Exceptions;
using PaymentGateway.Api.Factories.Contracts;
using PaymentGateway.Api.Models.Requests.Bank;
using PaymentGateway.Api.Models.Responses.Bank;

namespace PaymentGateway.Api.Services;

public class PaymentsProcessingService : IPaymentsProcessingService
{
    private readonly HttpClient _httpClient;
    private readonly IPaymentsRepository _paymentsRepository;
    private readonly ILogger<PaymentsProcessingService> _logger;

    public PaymentsProcessingService(
        IBankHttpClientFactory httpBankClientFactory,
        IPaymentsRepository paymentsRepository,
        ILogger<PaymentsProcessingService> logger)
    {
        _httpClient = httpBankClientFactory.CreateClient();
        _paymentsRepository = paymentsRepository;
        _logger = logger;
    }

    public async Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request)
    {
        var bankRequest = new AuthorizationRequest
        {
            CardNumber = request.CardNumber,
            ExpiryDate = $"{request.ExpiryMonth}/{request.ExpiryYear}",
            Currency = request.Currency,
            Amount = request.Amount,
            Cvv = request.Cvv.ToString()
        };

        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync("payments", bankRequest);

            httpResponse.EnsureSuccessStatusCode();

            var authorizationResponse = await httpResponse.Content.ReadFromJsonAsync<AuthorizationResponse>();

            if (authorizationResponse == null)
            {
                throw new BankInternalException();
            }

            var status = authorizationResponse.Authorized
                ? PaymentStatus.Authorized
                : PaymentStatus.Declined;

            var response = new PostPaymentResponse
            {
                Id = Guid.NewGuid(),
                Status = status,
                CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount
            };

            _paymentsRepository.Add(response);

            return response;
        }
        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 503)
        {
            _logger.LogError(ex.Message);

            throw new BankServiceUnavailableException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);

            throw new BankInternalException();
        }
    }
}
