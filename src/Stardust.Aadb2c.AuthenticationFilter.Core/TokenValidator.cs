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
            return ValidateTokenInternal(token,logger);
        }

        public static ClaimsPrincipal Validate(string token)
        {
            return ValidateTokenInternal(token, _loggerSingleton);
        }

        private static ClaimsPrincipal ValidateTokenInternal(string token ,ILogging logger)
        {
            logger?.DebugMessage("Validating bearer token.");
            var jwt = new JwtSecurityToken(token);
            var userIdClaim = jwt.Claims.SingleOrDefault(c => c.Type == "userId");
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
                    logger?.DebugMessage(
                        $"Validating client token {jwt.Claims.SingleOrDefault(c => c.Type == "appid")?.Value}");
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