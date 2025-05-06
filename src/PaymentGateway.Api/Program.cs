using PaymentGateway.Api.Factories;
using PaymentGateway.Api.Factories.Contracts;
using PaymentGateway.Api.Middleware;

using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();

builder.Services.AddHttpClient(HttpClients.BANK_CLIENT_NAME, (provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["Bank:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException(ConfigurationMessages.BANK_BASEURL_REQUIRED);
    }

    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uriResult))
    {
        throw new InvalidOperationException(ConfigurationMessages.BANK_BASEURL_INVALID);
    }

    client.BaseAddress = uriResult;
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddSingleton<IPaymentsRepository, PaymentsRepository>();

builder.Services.AddSingleton<IBankHttpClientFactory, BankHttpClientFactory>();

builder.Services.AddSingleton<IPaymentsValidationService, PaymentsValidationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(message => message.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
