﻿using Microsoft.IdentityModel.Tokens;
using Stardust.Particles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{
    public class TokenValidatorV1
    {
        public static ClaimsPrincipal ValidateToken(string accessToken, ILogging logger)
        {
            var handler = new JwtSecurityTokenHandler();
            //handler..Configuration = new SecurityTokenHandlerConfiguration { CertificateValidationMode = X509CertificateValidationMode.None };
            var audience = B2CGlobalConfiguration.AudienceV1;
            if (!audience.StartsWith("https://")) audience = string.Format("https://{0}/", audience);
            logger?.DebugMessage($"Vaidating token: {audience} {B2CGlobalConfiguration.ValidIssuerV1} {B2CGlobalConfiguration.AadTenant}");
            var validationParameters = new TokenValidationParameters()
            {
                
                ValidAudience = audience,
                ValidIssuer = B2CGlobalConfiguration.ValidIssuerV1,
                IssuerSigningKeys = GetSigningCertificates(string.Format("https://login.microsoftonline.com/{0}/federationmetadata/2007-06/federationmetadata.xml", B2CGlobalConfiguration.AadTenant))
                
            };
            try
            {

                var securityToken = handler.ValidateToken(accessToken, validationParameters, out SecurityToken validatedToken);

                ((ClaimsIdentity)securityToken.Identity).AddClaim(new Claim("token", accessToken));
                var principal = new ClaimsPrincipal(securityToken);

                var identity = principal.Identity as ClaimsIdentity;
                //Thread.CurrentPrincipal = principal;
                logger?.DebugMessage($"V1 token validation success: {identity?.Claims?.SingleOrDefault(c => c.Type == "appid")?.Value}");
                return principal;
            }
            catch (Exception ex)
            {

                throw new UnauthorizedAccessException("Unable to validate bearer token", ex);
            }
        }

        private static string MetadataEndpoint => $"https://login.microsoftonline.com/{B2CGlobalConfiguration.AadTenant}/.well-known/openid-configuration";
        public static ConcurrentDictionary<string, List<SecurityKey>> cache = new ConcurrentDictionary<string, List<SecurityKey>>();
        public static IEnumerable<SecurityKey> GetSigningCertificates(string metadataAddress)
        {
            var settings = new OpenIdConnectCachingSecurityTokenProvider(MetadataEndpoint);
            return settings.SecurityTokens;
        }
    }
}