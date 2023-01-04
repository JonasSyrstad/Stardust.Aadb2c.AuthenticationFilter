using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{

    public static class TokenValidator
    {
        private static ILogging _loggerSingleton;
        public static string NameClaimType { get; set; }

        public static void SetLogger(ILogging logger)
        {
            _loggerSingleton = logger;
        }

        public static ClaimsPrincipal Validate(string token, IServiceProvider provider)
        {
            var logger = provider.GetService(typeof(ILogging)) as ILogging;
            return ValidateTokenInternal(token, logger);
        }

        public static ClaimsPrincipal Validate(string token)
        {
            return ValidateTokenInternal(token, _loggerSingleton);
        }

        private static ClaimsPrincipal ValidateTokenInternal(string token, ILogging logger)
        {
            logger?.DebugMessage("Validating bearer token.");
            var jwt = new JwtSecurityToken(token);
            var userIdClaim = jwt.Claims.SingleOrDefault(c => c.Type == "userId") ?? jwt.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Email);

            if (userIdClaim != null)
            {
                logger?.DebugMessage($"Validating user token: {userIdClaim?.Value}");
                try
                {
                    return TokenValidatorV2.ValidateToken(token, logger);
                }
                catch (Exception ex)
                {
                    logger?.Exception(ex);
                    throw;
                }
            }
            else
            {
                try
                {
                    var tokenVersion = jwt.Claims.SingleOrDefault(c => c.Type == "ver");
                    logger?.DebugMessage($"token version: {tokenVersion?.Value??"na"}");
                    logger?.DebugMessage($"Issuer: {jwt.Issuer}");
                    if (B2CGlobalConfiguration.AllowClientCredentialsOverV2 &&  tokenVersion?.Value== "2.0")
                    {
                        logger?.DebugMessage($"Validating client token over V2 tokens {jwt.Claims.SingleOrDefault(c => c.Type == "azp")?.Value}");
                        return TokenValidatorV2.ValidateToken(token, logger);
                    }
                    logger?.DebugMessage($"Validating client token {jwt.Claims.SingleOrDefault(c => c.Type == "appid")?.Value}");
                    return TokenValidatorV1.ValidateToken(token, logger);
                }
                catch (Exception ex)
                {
                    logger?.Exception(ex);
                    throw;
                }
            }
        }
    }
}