using PaymentGateway.Api.Factories.Contracts;

namespace PaymentGateway.Api.Factories;

public class BankHttpClientFactory : IBankHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BankHttpClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateClient()
    {
        return _httpClientFactory.CreateClient(HttpClients.BANK_CLIENT_NAME);
    }
}
