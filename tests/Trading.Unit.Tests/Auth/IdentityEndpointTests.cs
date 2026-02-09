using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Verifies that the identity endpoint extension methods exist and are properly defined.
/// </summary>
public class IdentityEndpointTests
{
    [Fact]
    public void MapAdditionalIdentityEndpoints_ExtensionMethodExists()
    {
        var method = typeof(IdentityComponentsEndpointRouteBuilderExtensions)
            .GetMethod("MapAdditionalIdentityEndpoints", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(method);
    }

    [Fact]
    public void MapAdditionalIdentityEndpoints_IsExtensionMethod()
    {
        var method = typeof(IdentityComponentsEndpointRouteBuilderExtensions)
            .GetMethod("MapAdditionalIdentityEndpoints", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.True(method.IsStatic);

        var parameters = method.GetParameters();
        Assert.NotEmpty(parameters);
        Assert.True(typeof(IEndpointRouteBuilder).IsAssignableFrom(parameters[0].ParameterType));
    }

    [Fact]
    public void MapAdditionalIdentityEndpoints_ReturnsIEndpointConventionBuilder()
    {
        var method = typeof(IdentityComponentsEndpointRouteBuilderExtensions)
            .GetMethod("MapAdditionalIdentityEndpoints", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        Assert.NotNull(method);
        Assert.True(typeof(IEndpointConventionBuilder).IsAssignableFrom(method.ReturnType));
    }
}
