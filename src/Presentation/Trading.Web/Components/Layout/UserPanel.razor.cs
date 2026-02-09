using Microsoft.AspNetCore.Components;

namespace Trading.Web.Components.Layout;

public partial class UserPanel
{
    [Parameter]
    public EventCallback OnLoginRequested { get; set; }

    protected bool IsOpen { get; private set; }

    protected void ToggleDropdown() => IsOpen = !IsOpen;

    protected void CloseDropdown() => IsOpen = false;

    protected async Task OnLoginClicked()
    {
        await OnLoginRequested.InvokeAsync();
    }

    protected static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        // If it's an email, use first letter
        if (name.Contains('@'))
            return name[..1].ToUpperInvariant();

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant()
        };
    }
}
