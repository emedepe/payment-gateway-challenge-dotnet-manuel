namespace PaymentGateway.Api.Services.Contracts;

public interface IPaymentsProcessingService
{
    Task<PostPaymentResponse> ProcessPaymentAsync(PostPaymentRequest request);
}
