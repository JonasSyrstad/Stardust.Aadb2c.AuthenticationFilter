using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Stardust.Aadb2c.AuthenticationFilter.Core;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Authentication handler that supports both B2C and Auth0 tokens.
/// Inspects the token issuer to determine which validator to use.
/// </summary>
public sealed class VeracityTokenAuthHandler : AuthenticationHandler<VeracityTokenOptions>
{
    private readonly IServiceProvider _provider;

    /// <inheritdoc />
    public VeracityTokenAuthHandler(
        IOptionsMonitor<VeracityTokenOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IServiceProvider provider)
        : base(options, logger, encoder)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader is null ||
                !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogDebug("No bearer token provided");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader["Bearer ".Length..].Trim();
            if (token.Length == 0)
            {
                Logger.LogDebug("Empty bearer token provided");
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var issuerType = TokenIssuerDetector.Detect(token);

            ClaimsPrincipal principal;

            if (issuerType == TokenIssuerType.Auth0)
            {
                Logger.LogDebug("Token identified as Auth0, validating with Auth0 validator");
                principal = Auth0TokenValidator.ValidateToken(token);
            }
            else
            {
                Logger.LogDebug("Token identified as B2C/AAD, validating with B2C validator");
                principal = TokenValidator.Validate(token, _provider);
            }

            Context.User = principal;
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
            return Task.FromResult(AuthenticateResult.Fail("Token validation failed."));
        }
    }
}
