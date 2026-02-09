using Bunit;
using Trading.Web.Components.Layout;
using Trading.Web.Data;

namespace Trading.Unit.Tests.UI;

public class UserPanelComponentTests : BunitContext
{
    [Fact]
    public void UserPanel_Unauthenticated_ShowsSignInButton()
    {
        AddAuthorization();

        var cut = Render<UserPanel>();

        var signInBtn = cut.Find(".btn-login");
        Assert.Equal("Sign In", signInBtn.TextContent);
    }

    [Fact]
    public void UserPanel_Unauthenticated_ShowsSignUpLink()
    {
        AddAuthorization();

        var cut = Render<UserPanel>();

        var signUpLink = cut.Find(".btn-register");
        Assert.Equal("Sign Up", signUpLink.TextContent);
        Assert.Equal("Account/Register", signUpLink.GetAttribute("href"));
    }

    [Fact]
    public void UserPanel_Unauthenticated_HidesAvatarButton()
    {
        AddAuthorization();

        var cut = Render<UserPanel>();

        Assert.Empty(cut.FindAll(".avatar-btn"));
    }

    [Fact]
    public void UserPanel_Authenticated_ShowsAvatarButton()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();

        var avatarBtn = cut.Find(".avatar-btn");
        Assert.NotNull(avatarBtn);
    }

    [Fact]
    public void UserPanel_Authenticated_ShowsUserName()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();

        var userName = cut.Find(".user-name");
        Assert.Equal("demo@trading.local", userName.TextContent);
    }

    [Fact]
    public void UserPanel_Authenticated_ShowsInitials()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();

        var initials = cut.Find(".avatar-circle span");
        Assert.Equal("D", initials.TextContent); // First letter of email
    }

    [Fact]
    public void UserPanel_Authenticated_HidesAuthButtons()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();

        Assert.Empty(cut.FindAll(".auth-buttons"));
    }

    [Fact]
    public void UserPanel_ClickAvatar_TogglesDropdown()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();

        // Initially dropdown is closed
        Assert.Empty(cut.FindAll(".dropdown-menu"));

        // Click avatar button
        cut.Find(".avatar-btn").Click();

        // Dropdown should now be visible
        var dropdown = cut.Find(".dropdown-menu");
        Assert.NotNull(dropdown);
    }

    [Fact]
    public void UserPanel_DropdownOpen_ShowsProfileLinks()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        var dropdownItems = cut.FindAll(".dropdown-item");
        Assert.Contains(dropdownItems, i => i.TextContent.Contains("My Profile"));
        Assert.Contains(dropdownItems, i => i.TextContent.Contains("Email Settings"));
        Assert.Contains(dropdownItems, i => i.TextContent.Contains("Change Password"));
    }

    [Fact]
    public void UserPanel_DropdownOpen_ShowsSignOutButton()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        var signOut = cut.Find(".dropdown-item-danger");
        Assert.Contains("Sign Out", signOut.TextContent);
    }

    [Fact]
    public void UserPanel_NonAdmin_HidesAdminSection()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");
        authContext.SetRoles(SeedData.UserRole);

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        Assert.DoesNotContain(cut.Markup, "Administration");
        Assert.DoesNotContain(cut.Markup, "Data Management");
    }

    [Fact]
    public void UserPanel_Admin_ShowsAdminSection()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetRoles(SeedData.AdminRole);
        authContext.SetPolicies("AdminOnly");

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        // Check for admin-specific content
        Assert.Contains("Data Management", cut.Markup);
    }

    [Fact]
    public void UserPanel_Admin_ShowsRoleBadge()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetRoles(SeedData.AdminRole);
        authContext.SetPolicies("AdminOnly");

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        var badge = cut.Find(".role-badge");
        Assert.Equal("Admin", badge.TextContent);
    }

    [Fact]
    public void UserPanel_SignInButton_InvokesCallback()
    {
        AddAuthorization();
        var callbackInvoked = false;

        var cut = Render<UserPanel>(parameters => parameters
            .Add(p => p.OnLoginRequested, () => { callbackInvoked = true; return Task.CompletedTask; }));

        cut.Find(".btn-login").Click();

        Assert.True(callbackInvoked);
    }

    [Fact]
    public void UserPanel_ClickOverlay_ClosesDropdown()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");

        var cut = Render<UserPanel>();
        cut.Find(".avatar-btn").Click();

        // Dropdown is open
        Assert.NotEmpty(cut.FindAll(".dropdown-menu"));

        // Click overlay
        cut.Find(".dropdown-overlay").Click();

        // Dropdown should be closed
        Assert.Empty(cut.FindAll(".dropdown-menu"));
    }
}
