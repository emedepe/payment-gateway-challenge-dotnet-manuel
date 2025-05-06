namespace PaymentGateway.Api.Services.Contracts;

public interface IPaymentsRepository
{
    void Add(PostPaymentResponse payment);
    PostPaymentResponse? Get(Guid id);
}
