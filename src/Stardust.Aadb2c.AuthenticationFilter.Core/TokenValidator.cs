using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{

    public static class TokenValidator
    {
        private static ILogging _logger;

        public static void SetLogger(ILogging logger)
        {
            _logger = logger;
        }

        public static ClaimsPrincipal Validate(string token)
        {
            _logger?.DebugMessage("Validating bearer token.");
            var jwt = new JwtSecurityToken(token);
            var userIdClaim = jwt.Claims.SingleOrDefault(c => c.Type == "userId");
            if (userIdClaim != null)
            {
                _logger?.DebugMessage($"Validating user token: {userIdClaim?.Value}");
                try
                {
                    return TokenValidatorV2.ValidateToken(token, _logger);
                }
                catch (Exception ex)
                {
                    _logger?.Exception(ex);
                    throw;

                }
            }
            else
            {
                try
                {
                    _logger?.DebugMessage($"Validating client token {jwt.Claims.SingleOrDefault(c => c.Type == "appid")?.Value}");
                    return TokenValidatorV1.ValidateToken(token, _logger);
                }
                catch (Exception ex)
                {
                    _logger?.Exception(ex);
                    throw;
                }
            }
        }
    }
}