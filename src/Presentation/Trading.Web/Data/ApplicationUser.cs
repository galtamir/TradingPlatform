using Microsoft.AspNetCore.Identity;

namespace Trading.Web.Data;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string PreferredCurrency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
