namespace Trading.Contracts.DTOs;

public sealed record UserProfileDto(
    string UserId,
    string DisplayName,
    string Email,
    string PreferredCurrency);
