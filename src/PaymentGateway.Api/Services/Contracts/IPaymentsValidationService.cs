namespace PaymentGateway.Api.Services.Contracts;

public interface IPaymentsValidationService
{
    PostPaymentRequest ValidatePayment(PostPaymentRequest request);
}
