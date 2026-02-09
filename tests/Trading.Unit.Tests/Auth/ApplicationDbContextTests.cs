using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class ApplicationDbContextTests : IDisposable
{
    private readonly ApplicationDbContext _context;

    public ApplicationDbContextTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"DbContextTest_{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void DbContext_InheritsFromIdentityDbContext()
    {
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Identity.EntityFrameworkCore
            .IdentityDbContext<ApplicationUser, IdentityRole, string>>(_context);
    }

    [Fact]
    public void DbContext_CanAddAndRetrieveUser()
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "dbtest@example.com",
            NormalizedUserName = "DBTEST@EXAMPLE.COM",
            Email = "dbtest@example.com",
            NormalizedEmail = "DBTEST@EXAMPLE.COM",
            DisplayName = "DB Test",
            SecurityStamp = Guid.NewGuid().ToString()
        };

        _context.Users.Add(user);
        _context.SaveChanges();

        var found = _context.Users.FirstOrDefault(u => u.Email == "dbtest@example.com");
        Assert.NotNull(found);
        Assert.Equal("DB Test", found.DisplayName);
    }

    [Fact]
    public void DbContext_CanAddAndRetrieveRole()
    {
        var role = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "TestRole",
            NormalizedName = "TESTROLE"
        };

        _context.Roles.Add(role);
        _context.SaveChanges();

        var found = _context.Roles.FirstOrDefault(r => r.Name == "TestRole");
        Assert.NotNull(found);
    }

    [Fact]
    public void DbContext_CanAssignUserRole()
    {
        var userId = Guid.NewGuid().ToString();
        var roleId = Guid.NewGuid().ToString();

        _context.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "roletest@example.com",
            NormalizedUserName = "ROLETEST@EXAMPLE.COM",
            Email = "roletest@example.com",
            NormalizedEmail = "ROLETEST@EXAMPLE.COM",
            DisplayName = "Role Test",
            SecurityStamp = Guid.NewGuid().ToString()
        });

        _context.Roles.Add(new IdentityRole
        {
            Id = roleId,
            Name = "Admin",
            NormalizedName = "ADMIN"
        });

        _context.UserRoles.Add(new IdentityUserRole<string>
        {
            UserId = userId,
            RoleId = roleId
        });

        _context.SaveChanges();

        var userRole = _context.UserRoles.FirstOrDefault(ur => ur.UserId == userId);
        Assert.NotNull(userRole);
        Assert.Equal(roleId, userRole.RoleId);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
