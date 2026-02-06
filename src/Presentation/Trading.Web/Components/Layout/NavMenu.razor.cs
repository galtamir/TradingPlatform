using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Trading.Web.Components.Layout;

public partial class NavMenu : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected string? CurrentUrl { get; private set; }

    protected override void OnInitialized()
    {
        CurrentUrl = NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        CurrentUrl = NavigationManager.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
