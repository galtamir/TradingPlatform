using System.Reflection;
using Trading.Web.Components.Layout;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Tests the UserPanel helper methods via reflection since they are protected.
/// </summary>
public class UserPanelTests
{
    private static readonly MethodInfo GetInitialsMethod =
        typeof(UserPanel).GetMethod("GetInitials", BindingFlags.Static | BindingFlags.NonPublic)!;

    private static string? GetInitials(string? name) =>
        (string?)GetInitialsMethod.Invoke(null, [name]);

    [Theory]
    [InlineData(null, "?")]
    [InlineData("", "?")]
    [InlineData("   ", "?")]
    public void GetInitials_NullOrEmpty_ReturnsQuestionMark(string? input, string expected)
    {
        Assert.Equal(expected, GetInitials(input));
    }

    [Theory]
    [InlineData("admin@trading.local", "A")]
    [InlineData("demo@example.com", "D")]
    public void GetInitials_Email_ReturnsFirstLetter(string email, string expected)
    {
        Assert.Equal(expected, GetInitials(email));
    }

    [Theory]
    [InlineData("John", "J")]
    [InlineData("alice", "A")]
    public void GetInitials_SingleName_ReturnsFirstLetter(string name, string expected)
    {
        Assert.Equal(expected, GetInitials(name));
    }

    [Theory]
    [InlineData("John Doe", "JD")]
    [InlineData("jane smith", "JS")]
    [InlineData("Alice Bob Charlie", "AC")]
    public void GetInitials_FullName_ReturnsFirstAndLastInitials(string name, string expected)
    {
        Assert.Equal(expected, GetInitials(name));
    }
}
