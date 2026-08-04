using Microsoft.AspNetCore.Authentication;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Options for the Veracity multi-token authentication handler.
/// B2C configuration is inherited from B2CGlobalConfiguration.
/// Auth0 (IdP V2) configuration is provided via the static Auth0Configuration class.
/// </summary>
public sealed class VeracityTokenOptions : AuthenticationSchemeOptions
{
}
