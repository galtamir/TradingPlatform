namespace Trading.Contracts.Interfaces;

/// <summary>
/// Platform-agnostic notification service.
/// Implemented differently by Web (push) and Mobile (native toast).
/// </summary>
public interface INotificationService
{
    Task SendAsync(string userId, string title, string message, CancellationToken ct = default);
}
