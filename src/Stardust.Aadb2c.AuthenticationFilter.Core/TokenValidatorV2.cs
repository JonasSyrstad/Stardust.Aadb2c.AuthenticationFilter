using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{
    public class TokenValidatorV2
    {

        public static ClaimsPrincipal ValidateToken(string accessToken, ILogging logger)
        {
            var handler = new JwtSecurityTokenHandler();
            //{
            //    Configuration = new SecurityTokenHandlerConfiguration { /*CertificateValidationMode = X509CertificateValidationMode.None*/ }
            //};

            try
            {
                SecurityToken validatedToken;

                var securityToken = handler.ValidateToken(accessToken, ValidationParameters(), out validatedToken);
                
                var principal = new ClaimsPrincipal(securityToken);
                
                Thread.CurrentPrincipal = principal;
                logger?.DebugMessage($"User: {principal.FindFirst(TokenValidator.NameClaimType??ClaimTypes.Name)}");
                return principal;
            }
            catch (Exception ex)
            {
                logger?.Exception(ex);
                throw new UnauthorizedAccessException("Unable to validate token", ex);
            }
        }

        private static TokenValidationParameters ValidationParameters()
        {
            var tokenValidationParameters= new TokenValidationParameters
            {
                ValidAudiences = Resource.Split(';'),
                ValidIssuers = new[] { B2CGlobalConfiguration.ValidIssuer, B2CGlobalConfiguration.ValidIssuer + "/" },
                IssuerSigningKeys = Settings.SecurityTokens

            };
            return tokenValidationParameters;

        }

        private static readonly OpenIdConnectCachingSecurityTokenProvider Settings = new OpenIdConnectCachingSecurityTokenProvider(MetadataEndpoint);


        private static string MetadataEndpoint
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ConfigurationManagerHelper.GetValueOnKey("aadPolicy")))
                    return $"https://login.microsoftonline.com/{B2CGlobalConfiguration.AadTenant}/v2.0/.well-known/openid-configuration";
                return $"https://login.microsoftonline.com/{B2CGlobalConfiguration.AadTenant}/v2.0/.well-known/openid-configuration?p={B2CGlobalConfiguration.AadPolicy}";
            }
        }

        private static string Resource
        {
            get
            {
                if (audience != null) return audience;
                audience = B2CGlobalConfiguration.Audience;
                return audience;
            }
        }

        private static string audience;


    }
}