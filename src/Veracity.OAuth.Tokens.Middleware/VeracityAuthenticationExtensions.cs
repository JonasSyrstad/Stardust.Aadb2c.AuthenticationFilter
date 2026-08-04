using Microsoft.AspNetCore.Authentication;
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Particles;

namespace Veracity.OAuth.Tokens.Middleware;

/// <summary>
/// Extension methods for registering the Veracity multi-token authentication middleware.
/// Configures both B2C (via <see cref="B2CGlobalConfiguration"/>) and Auth0/IdP V2
/// (via <see cref="Auth0Configuration"/>) in a single call.
/// </summary>
public static class VeracityAuthenticationExtensions
{
    /// <summary>
    /// The default authentication scheme name.
    /// </summary>
    public const string DefaultScheme = "VeracityOAuth";

    /// <summary>
    /// Adds the Veracity authentication handler that supports both B2C and Auth0 tokens.
    /// B2C configuration is read from <see cref="B2CGlobalConfiguration"/>.
    /// Auth0 (IdP V2) configuration is read from <see cref="Auth0Configuration"/>.
    /// Both must be set before calling this method.
    /// </summary>
    public static AuthenticationBuilder AddVeracityAuthentication(
        this AuthenticationBuilder builder,
        string scheme,
        string displayName,
        Action<VeracityTokenOptions>? configureOptions = null)
    {
        // Initialize Auth0 validator if the IdP V2 domain is configured
        if (Auth0Configuration.IsConfigured)
        {
            Auth0TokenValidator.Initialize();
        }

        builder.AddScheme<VeracityTokenOptions, VeracityTokenAuthHandler>(scheme, displayName, options =>
        {
            configureOptions?.Invoke(options);
        });

        return builder;
    }

    /// <summary>
    /// Adds the Veracity authentication handler with the default scheme name.
    /// </summary>
    public static AuthenticationBuilder AddVeracityAuthentication(
        this AuthenticationBuilder builder,
        Action<VeracityTokenOptions>? configureOptions)
    {
        return builder.AddVeracityAuthentication(DefaultScheme, DefaultScheme, configureOptions);
    }

    /// <summary>
    /// Adds the Veracity authentication handler with default scheme and no extra options.
    /// </summary>
    public static AuthenticationBuilder AddVeracityAuthentication(
        this AuthenticationBuilder builder)
    {
        return builder.AddVeracityAuthentication(DefaultScheme, DefaultScheme);
    }
}
