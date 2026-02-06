using Trading.Contracts.DTOs;

namespace Trading.Unit.Tests;

public class ContractDtoTests
{
    [Fact]
    public void UserProfileDto_CreatesWithExpectedValues()
    {
        var dto = new UserProfileDto("user-1", "Jane Doe", "jane@example.com", "USD");

        Assert.Equal("user-1", dto.UserId);
        Assert.Equal("Jane Doe", dto.DisplayName);
        Assert.Equal("jane@example.com", dto.Email);
        Assert.Equal("USD", dto.PreferredCurrency);
    }

    [Fact]
    public void UserProfileDto_SupportsValueEquality()
    {
        var a = new UserProfileDto("u1", "Name", "email@example.com", "EUR");
        var b = new UserProfileDto("u1", "Name", "email@example.com", "EUR");

        Assert.Equal(a, b);
    }

    [Fact]
    public void UserProfileDto_Deconstruct_Works()
    {
        var dto = new UserProfileDto("u1", "Name", "email@example.com", "GBP");
        var (userId, displayName, email, currency) = dto;

        Assert.Equal("u1", userId);
        Assert.Equal("Name", displayName);
        Assert.Equal("email@example.com", email);
        Assert.Equal("GBP", currency);
    }
}
