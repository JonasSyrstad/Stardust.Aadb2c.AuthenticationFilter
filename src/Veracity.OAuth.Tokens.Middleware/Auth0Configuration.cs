using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Particles;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Configuration settings for the Veracity Identity Provider V2 (Auth0) token validation.
/// Uses the same ConfigurationManagerHelper pattern as <see cref="B2CGlobalConfiguration"/>
/// with keys prefixed with "IdPV2" to distinguish from B2C settings.
/// </summary>
public static class Auth0Configuration
{
    /// <summary>
    /// The Auth0 domain (e.g., "my-tenant.auth0.com").
    /// Used to construct the issuer and JWKS endpoint.
    /// </summary>
    public static string Domain
    {
        get => ConfigurationManagerHelper.GetValueOnKey("IdPV2Domain");
        set => ConfigurationManagerHelper.SetValueOnKey("IdPV2Domain", value, true);
    }

    /// <summary>
    /// Semicolon-separated list of valid audiences for Auth0 tokens.
    /// </summary>
    public static string Audience
    {
        get => ConfigurationManagerHelper.GetValueOnKey("IdPV2Audience");
        set => ConfigurationManagerHelper.SetValueOnKey("IdPV2Audience", value, true);
    }

    /// <summary>
    /// The expected issuer URL. If not set, defaults to "https://{Domain}/".
    /// </summary>
    public static string ValidIssuer
    {
        get => ConfigurationManagerHelper.GetValueOnKey("IdPV2Issuer");
        set => ConfigurationManagerHelper.SetValueOnKey("IdPV2Issuer", value, true);
    }

    /// <summary>
    /// Gets the resolved issuer value.
    /// </summary>
    internal static string ResolvedIssuer => string.IsNullOrWhiteSpace(ValidIssuer) ? $"https://{Domain}/" : ValidIssuer;

    /// <summary>
    /// Gets the OpenID Connect metadata endpoint for Auth0.
    /// </summary>
    internal static string MetadataEndpoint => $"https://{Domain}/.well-known/openid-configuration";

    /// <summary>
    /// Whether Auth0 (IdP V2) validation is enabled.
    /// </summary>
    internal static bool IsConfigured => !string.IsNullOrWhiteSpace(Domain);
}
