using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Validates Auth0 (IdP V2) JWT tokens using the Auth0 domain's OIDC discovery document.
/// </summary>
internal static class Auth0TokenValidator
{
    private static ConfigurationManager<OpenIdConnectConfiguration>? _configManager;
    private static bool _initialized;

    internal static void Initialize()
    {
        if (!Auth0Configuration.IsConfigured)
        {
            return;
        }

        _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            Auth0Configuration.MetadataEndpoint,
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever { RequireHttps = true });
        _initialized = true;
    }

    internal static ClaimsPrincipal ValidateToken(string accessToken)
    {
        if (!_initialized || _configManager is null)
        {
            throw new InvalidOperationException("Auth0TokenValidator has not been initialized. Ensure Auth0Configuration.Domain is set and Initialize() has been called.");
        }

        var openIdConfig = _configManager.GetConfigurationAsync(CancellationToken.None)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidAlgorithms = ["RS256"],
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidAudiences = Auth0Configuration.Audience.Split(';'),
            ValidIssuer = Auth0Configuration.ResolvedIssuer,
            IssuerSigningKeys = openIdConfig.SigningKeys
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(accessToken, validationParameters, out _);

        return principal;
    }
}
