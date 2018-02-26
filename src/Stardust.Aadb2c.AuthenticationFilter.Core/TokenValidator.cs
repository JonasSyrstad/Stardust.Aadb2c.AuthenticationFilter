using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{
    public static class TokenValidator
    {
        public static ClaimsPrincipal Validate(string token)
        {
            var jwt = new JwtSecurityToken(token);
            if (jwt.Claims.SingleOrDefault(c => c.Type == "userId") != null)
            {
                // Logging.DebugMessage("Validating user token");
                try
                {
                    return TokenValidatorV2.ValidateToken(token);
                }
                catch (Exception ex)
                {
                    throw;

                }
            }
            else
            {
                return TokenValidatorV1.ValidateToken(token);
            }
        }
    }
}