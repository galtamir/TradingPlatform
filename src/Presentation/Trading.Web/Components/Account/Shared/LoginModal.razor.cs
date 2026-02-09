using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Trading.Web.Data;

namespace Trading.Web.Components.Account.Shared;

public partial class LoginModal
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [Parameter]
    public bool IsVisible { get; set; }

    [Parameter]
    public EventCallback<bool> IsVisibleChanged { get; set; }

    [Parameter]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromForm(FormName = "loginModal")]
    private InputModel Input { get; set; } = default!;

    protected string? ErrorMessage { get; private set; }
    protected bool IsLoading { get; private set; }
    protected EditContext EditContext { get; private set; } = default!;

    protected override void OnInitialized()
    {
        Input ??= new();
        EditContext = new EditContext(Input);
    }

    protected async Task Close()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(false);
    }

    protected string GetRegisterUrl()
    {
        var queryParams = new Dictionary<string, object?> { ["ReturnUrl"] = ReturnUrl };
        return NavigationManager.GetUriWithQueryParameters("Account/Register", queryParams);
    }

    protected async Task LoginUser()
    {
        ErrorMessage = null;
        IsLoading = true;

        try
        {
            if (!string.IsNullOrEmpty(Input.Passkey?.Error))
            {
                ErrorMessage = $"Error: {Input.Passkey.Error}";
                return;
            }

            SignInResult result;
            if (!string.IsNullOrEmpty(Input.Passkey?.CredentialJson))
            {
                result = await SignInManager.PasskeySignInAsync(Input.Passkey.CredentialJson);
            }
            else
            {
                if (!EditContext.Validate())
                {
                    return;
                }

                result = await SignInManager.PasswordSignInAsync(
                    Input.Email, 
                    Input.Password, 
                    Input.RememberMe, 
                    lockoutOnFailure: false);
            }

            if (result.Succeeded)
            {
                var user = await UserManager.FindByEmailAsync(Input.Email);
                if (user is not null)
                    await ActivityService.LogAsync(user.Id, Input.Email, "Login", "Modal sign-in");

                Logger.LogInformation("User logged in via modal.");
                await Close();
                RedirectManager.RedirectTo(ReturnUrl);
            }
            else if (result.RequiresTwoFactor)
            {
                await Close();
                RedirectManager.RedirectTo(
                    "Account/LoginWith2fa",
                    new() { ["returnUrl"] = ReturnUrl, ["rememberMe"] = Input.RememberMe });
            }
            else if (result.IsLockedOut)
            {
                Logger.LogWarning("User account locked out.");
                await Close();
                RedirectManager.RedirectTo("Account/Lockout");
            }
            else
            {
                ErrorMessage = "Invalid email or password. Please try again.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private sealed class InputModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public PasskeyInputModel? Passkey { get; set; }
    }
}
