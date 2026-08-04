using System.IdentityModel.Tokens.Jwt;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Determines the token type by inspecting the JWT issuer claim without full validation.
/// </summary>
internal static class TokenIssuerDetector
{
    /// <summary>
    /// Detects whether the token was issued by Auth0 (IdP V2) or B2C/AAD.
    /// </summary>
    internal static TokenIssuerType Detect(string token)
    {
        if (!Auth0Configuration.IsConfigured)
        {
            return TokenIssuerType.B2C;
        }

        try
        {
            var jwt = new JwtSecurityToken(token);
            var issuer = jwt.Issuer;

            if (!string.IsNullOrEmpty(issuer) &&
                Uri.TryCreate(issuer, UriKind.Absolute, out var issuerUri) &&
                string.Equals(issuerUri.Scheme, "https", StringComparison.OrdinalIgnoreCase) &&
                issuerUri.Host.Equals(Auth0Configuration.Domain, StringComparison.OrdinalIgnoreCase))
            {
                return TokenIssuerType.Auth0;
            }
        }
        catch
        {
            // If we can't read the token, fall through to B2C validation
        }

        return TokenIssuerType.B2C;
    }
}

internal enum TokenIssuerType
{
    B2C,
    Auth0
}
