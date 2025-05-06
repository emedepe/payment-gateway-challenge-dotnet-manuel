namespace PaymentGateway.Api.Factories.Contracts;

public interface IBankHttpClientFactory
{
    HttpClient CreateClient();
}
