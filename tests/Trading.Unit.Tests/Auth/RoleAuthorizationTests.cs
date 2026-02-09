using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class RoleAuthorizationTests : IAsyncLifetime
{
    private IdentityTestFixture _fixture = null!;

    public Task InitializeAsync()
    {
        _fixture = new IdentityTestFixture();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _fixture.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateRole_NewRole_Succeeds()
    {
        var result = await _fixture.RoleManager.CreateAsync(new IdentityRole("TestRole"));

        Assert.True(result.Succeeded);
        Assert.True(await _fixture.RoleManager.RoleExistsAsync("TestRole"));
    }

    [Fact]
    public async Task CreateRole_DuplicateRole_Fails()
    {
        await _fixture.RoleManager.CreateAsync(new IdentityRole("DupRole"));

        var result = await _fixture.RoleManager.CreateAsync(new IdentityRole("DupRole"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AddUserToRole_ValidRole_Succeeds()
    {
        await _fixture.RoleManager.CreateAsync(new IdentityRole("Trader"));
        var user = await _fixture.CreateTestUserAsync("trader@example.com", "Password1234");

        var result = await _fixture.UserManager.AddToRoleAsync(user, "Trader");

        Assert.True(result.Succeeded);
        Assert.True(await _fixture.UserManager.IsInRoleAsync(user, "Trader"));
    }

    [Fact]
    public async Task RemoveUserFromRole_ExistingRole_Succeeds()
    {
        var user = await _fixture.CreateTestUserAsync("remove@example.com", "Password1234", role: "Admin");

        var result = await _fixture.UserManager.RemoveFromRoleAsync(user, "Admin");

        Assert.True(result.Succeeded);
        Assert.False(await _fixture.UserManager.IsInRoleAsync(user, "Admin"));
    }

    [Fact]
    public async Task GetRoles_UserWithMultipleRoles_ReturnsAll()
    {
        await _fixture.RoleManager.CreateAsync(new IdentityRole("RoleA"));
        await _fixture.RoleManager.CreateAsync(new IdentityRole("RoleB"));
        var user = await _fixture.CreateTestUserAsync("multi@example.com", "Password1234");

        await _fixture.UserManager.AddToRoleAsync(user, "RoleA");
        await _fixture.UserManager.AddToRoleAsync(user, "RoleB");

        var roles = await _fixture.UserManager.GetRolesAsync(user);

        Assert.Contains("RoleA", roles);
        Assert.Contains("RoleB", roles);
        Assert.Equal(2, roles.Count);
    }

    [Fact]
    public async Task GetUsersInRole_ReturnsCorrectUsers()
    {
        await _fixture.RoleManager.CreateAsync(new IdentityRole("Analyst"));
        var user1 = await _fixture.CreateTestUserAsync("a1@example.com", "Password1234");
        var user2 = await _fixture.CreateTestUserAsync("a2@example.com", "Password1234");
        await _fixture.CreateTestUserAsync("a3@example.com", "Password1234"); // not in role

        await _fixture.UserManager.AddToRoleAsync(user1, "Analyst");
        await _fixture.UserManager.AddToRoleAsync(user2, "Analyst");

        var usersInRole = await _fixture.UserManager.GetUsersInRoleAsync("Analyst");

        Assert.Equal(2, usersInRole.Count);
        Assert.Contains(usersInRole, u => u.Email == "a1@example.com");
        Assert.Contains(usersInRole, u => u.Email == "a2@example.com");
    }

    [Fact]
    public async Task DeleteRole_ExistingRole_Succeeds()
    {
        await _fixture.RoleManager.CreateAsync(new IdentityRole("Temporary"));

        var role = await _fixture.RoleManager.FindByNameAsync("Temporary");
        var result = await _fixture.RoleManager.DeleteAsync(role!);

        Assert.True(result.Succeeded);
        Assert.False(await _fixture.RoleManager.RoleExistsAsync("Temporary"));
    }

    [Fact]
    public async Task IsInRole_UserNotInRole_ReturnsFalse()
    {
        var user = await _fixture.CreateTestUserAsync("norole@example.com", "Password1234");

        Assert.False(await _fixture.UserManager.IsInRoleAsync(user, "Admin"));
    }
}
