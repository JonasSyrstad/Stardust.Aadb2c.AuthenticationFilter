using Stardust.Particles;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public static class TokenValidator
    {
        public static ClaimsPrincipal Validate(string token)
        {
            var jwt = new JwtSecurityToken(token);
            if (jwt.Claims.SingleOrDefault(c => c.Type == "userId") != null)
            {
                Logging.DebugMessage("Validating user token");
                try
                {
                    return OAuthAuthenticationFilter.ValidateToken(token);
                }
                catch (Exception ex)
                {
                    ex.Log();
                    throw;

                }
            }
            else
            {
                Logging.DebugMessage("Validating client token");
                return AdalTokenValidationHelper.ValidateToken(token);
            }
        }
    }
}
