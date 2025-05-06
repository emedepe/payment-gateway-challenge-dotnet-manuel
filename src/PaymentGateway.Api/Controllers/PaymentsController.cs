using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController : Controller
{
    private readonly IPaymentsRepository _paymentsRepository;

    private readonly IPaymentsProcessingService _paymentProcessingService;
    private readonly IPaymentsValidationService _paymentsValidationService;

    public PaymentsController(
        IPaymentsRepository paymentsRepository,
        IPaymentsProcessingService paymentProcessingService,
        IPaymentsValidationService paymentsValidationService)
    {
        _paymentsRepository = paymentsRepository;
        _paymentProcessingService = paymentProcessingService;
        _paymentsValidationService = paymentsValidationService;
    }

    /// <summary>
    /// Retrieves a payment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>The payment details if found; otherwise, a 404 response.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = _paymentsRepository.Get(id);

        return payment == null
            ? NotFound()
            : new OkObjectResult(payment);
    }

    /// <summary>
    /// Processes a payment request and returns the result of the payment operation.
    /// </summary>
    /// <param name="request">The payment request containing card details, amount, and other payment information.</param>
    /// <returns>
    /// A <see cref="PostPaymentResponse"/> object with the details of the processed payment:
    /// - Returns a 200 OK response with the payment details if the payment is accepted or declined.
    /// - Returns a 400 Bad Request response if the payment request is invalid and rejected.
    /// - Returns a 500 Internal Server Error response if an error occurs during payment processing.
    /// </returns>
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        var sanitizedRequest = _paymentsValidationService.ValidatePayment(request);

        var response = await _paymentProcessingService.ProcessPaymentAsync(sanitizedRequest);

        return new OkObjectResult(response);
    }
}
