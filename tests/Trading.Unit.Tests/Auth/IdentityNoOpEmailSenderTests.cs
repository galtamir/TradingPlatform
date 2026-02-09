using Microsoft.AspNetCore.Identity;
using Trading.Web.Components.Account;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class IdentityNoOpEmailSenderTests
{
    private readonly IdentityNoOpEmailSender _sender = new();
    private readonly ApplicationUser _user = new()
    {
        Email = "test@example.com",
        DisplayName = "Test"
    };

    [Fact]
    public void Sender_ImplementsIEmailSender()
    {
        Assert.IsAssignableFrom<IEmailSender<ApplicationUser>>(_sender);
    }

    [Fact]
    public async Task SendConfirmationLink_DoesNotThrow()
    {
        await _sender.SendConfirmationLinkAsync(_user, "test@example.com", "https://example.com/confirm");
    }

    [Fact]
    public async Task SendPasswordResetLink_DoesNotThrow()
    {
        await _sender.SendPasswordResetLinkAsync(_user, "test@example.com", "https://example.com/reset");
    }

    [Fact]
    public async Task SendPasswordResetCode_DoesNotThrow()
    {
        await _sender.SendPasswordResetCodeAsync(_user, "test@example.com", "123456");
    }
}
