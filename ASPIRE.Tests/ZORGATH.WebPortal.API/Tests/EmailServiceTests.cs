using Moq;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace ASPIRE.Tests.ZORGATH.WebPortal.API.Tests;

public class EmailServiceTests
{
    private readonly Mock<IOptions<OperationalConfiguration>> _mockOptions;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ISendGridClient> _mockSendGridClient;
    private readonly EmailService _emailService;
    private readonly OperationalConfiguration _configuration;

    public EmailServiceTests()
    {
        _mockOptions = new Mock<IOptions<OperationalConfiguration>>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockSendGridClient = new Mock<ISendGridClient>();

        _configuration = new OperationalConfiguration
        {
            Email = new OperationalConfigurationEmail
            {
                ApiKey = "SG.TEST_API_KEY",
                FromEmail = "test@test.com",
                FromName = "PK Test"
            },
            JWT = new OperationalConfigurationJWT
            {
                SigningKey = "dummy-key-for-testing",
                Issuer = "test-issuer",
                Audience = "test-audience",
                DurationInHours = 1
            },
            UIBaseUrl = "https://test.localhost"
        };
        _mockOptions.Setup(o => o.Value).Returns(_configuration);

        _emailService = new EmailService(
            _mockOptions.Object,
            _mockLogger.Object,
            _mockEnvironment.Object,
            _mockSendGridClient.Object
        );
    }

    [Test]
    public async Task SendEmailAddressRegistrationLink_WithValidConfig_CallsSendGridClient()
    {
        // Arrange
        string email = "user@example.com";
        string token = "test-token-123";
        string expectedSubject = "Verify Email Address";
        string expectedLink = "https://test.localhost/register/test-token-123";

        SendGridMessage? capturedMessage = null;

        _mockSendGridClient
            .Setup(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()))
            .Callback<SendGridMessage, CancellationToken>((msg, ct) => capturedMessage = msg)
            .ReturnsAsync(new Response(System.Net.HttpStatusCode.Accepted, null, null));

        // Act
        bool result = await _emailService.SendEmailAddressRegistrationLink(email, token);

        // Assert
        await Assert.That(result).IsTrue();

        await Assert.That(capturedMessage).IsNotNull();
        // MailHelper.CreateSingleEmail might put the subject in Personalizations
        string? actualSubject = string.IsNullOrEmpty(capturedMessage!.Subject)
            ? capturedMessage.Personalizations[0].Subject
            : capturedMessage.Subject;

        await Assert.That(actualSubject).IsEqualTo(expectedSubject);
        await Assert.That(capturedMessage.Personalizations[0].Tos[0].Email).IsEqualTo(email);
        await Assert.That(capturedMessage.From?.Email).IsEqualTo(_configuration.Email.FromEmail);
        // Check PlainTextContent property or Contents list
        string? content = capturedMessage.PlainTextContent;
        if (string.IsNullOrEmpty(content) && capturedMessage.Contents != null)
        {
            content = capturedMessage.Contents.FirstOrDefault(c => c.Type == "text/plain")?.Value;
        }

        await Assert.That(content).IsNotNull();
        await Assert.That(content!.Contains(expectedLink)).IsTrue();
    }

    [Test]
    public async Task SendEmailAddressRegistrationLink_WithMissingApiKey_LogsVariableAndReturnsTrue()
    {
        // Arrange
        OperationalConfiguration invalidConfig = new OperationalConfiguration
        {
            Email = new OperationalConfigurationEmail
            {
                ApiKey = "", // Missing key
                FromEmail = "test@test.com",
                FromName = "PK Test"
            },
            JWT = new OperationalConfigurationJWT
            {
                SigningKey = "dummy",
                Issuer = "dummy",
                Audience = "dummy",
                DurationInHours = 1
            },
            UIBaseUrl = "https://test.localhost"
        };

        Mock<IOptions<OperationalConfiguration>> mockOptions = new Mock<IOptions<OperationalConfiguration>>();
        mockOptions.Setup(o => o.Value).Returns(invalidConfig);

        EmailService emailService = new EmailService(
            mockOptions.Object,
            _mockLogger.Object,
            _mockEnvironment.Object,
            _mockSendGridClient.Object
        );

        // Act
        bool result = await emailService.SendEmailAddressRegistrationLink("user@example.com", "token");

        // Assert
        await Assert.That(result).IsTrue();

        // Verify SendGrid was NOT called
        _mockSendGridClient.Verify(c => c.SendEmailAsync(It.IsAny<SendGridMessage>(), It.IsAny<CancellationToken>()), Times.Never);

        // Verify Logger warning
        _mockLogger.Verify(logger => logger.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("SendGrid API Key Not Configured")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
