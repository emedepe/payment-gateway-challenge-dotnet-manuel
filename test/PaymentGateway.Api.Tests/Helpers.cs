using Microsoft.Extensions.Logging;

using Moq;

namespace PaymentGateway.Api.Tests;

public static class Helpers
{
    public static PostPaymentRequest CreateValidPaymentRequest()
    {
        var random = new Random();

        var year = DateTime.UtcNow.Year;

        return new PostPaymentRequest
        {
            CardNumber = "1234567890123456",
            ExpiryMonth = random.Next(1, 12),
            ExpiryYear = random.Next(year + 1, year + 10),
            Currency = "GBP",
            Amount = random.Next(1, 10000),
            Cvv = random.Next(1, 9999)
        };
    }

    public static void AssertLogger<T>(
        Mock<ILogger<T>> loggerMock,
        LogLevel expectedLogLevel,
        string expectedMessage,
        int expectedTimes)
    {
        loggerMock.Verify(
            l => l.Log(
                It.Is<LogLevel>(ll => ll == expectedLogLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(expectedTimes));
    }
}
