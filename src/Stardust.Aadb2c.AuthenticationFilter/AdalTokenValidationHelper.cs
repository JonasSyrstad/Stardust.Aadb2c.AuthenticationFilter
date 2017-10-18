using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Threading;
using System.Web;
using System.Xml;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public class AdalTokenValidationHelper
    {
        public static ClaimsPrincipal ValidateToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            handler.Configuration = new SecurityTokenHandlerConfiguration { CertificateValidationMode = X509CertificateValidationMode.None };
            var audience = B2CGlobalConfiguration.AudienceV1;
            if (!audience.StartsWith("https://")) audience = string.Format("https://{0}/", audience);
           // Logging.DebugMessage($"Vaidating token: {audience} {B2CGlobalConfiguration.ValidIssuerV1} {B2CGlobalConfiguration.AadTenant}");
            var validationParameters = new TokenValidationParameters()
            {
                ValidAudience = audience,
                ValidIssuer = B2CGlobalConfiguration.ValidIssuerV1,
                IssuerSigningTokens = GetSigningCertificates(string.Format("https://login.microsoftonline.com/{0}/federationmetadata/2007-06/federationmetadata.xml", B2CGlobalConfiguration.AadTenant))
            };
            try
            {
                SecurityToken validatedToken;
                var securityToken = handler.ValidateToken(accessToken, validationParameters, out validatedToken);

                ((ClaimsIdentity)securityToken.Identity).AddClaim(new Claim("token", accessToken));
               // Logging.DebugMessage($"Token is validated");
                var principal = new ClaimsPrincipal(securityToken);

                var identity = principal.Identity as ClaimsIdentity;
                //Logging.DebugMessage($"User: {Resolver.Activate<IIdentityLookup>().GetUserName(identity)} validated");
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
                return principal;
               // Logging.DebugMessage("Principal set on http context")
            }
            catch (Exception ex)
            {
                Logging.Exception(ex);  
                Logging.DebugMessage($"bearer token: {accessToken}", additionalDebugInformation: "Authentication Failure");
                throw new UnauthorizedAccessException("Unable to validate bearer token", ex);
            }
        }
        public static ConcurrentDictionary<string, List<SecurityToken>> cache = new ConcurrentDictionary<string, List<SecurityToken>>();
        public static List<SecurityToken> GetSigningCertificates(string metadataAddress)
        {
            List<SecurityToken> tokens;
            if (cache.TryGetValue(metadataAddress, out tokens)) return tokens;
            tokens = new List<SecurityToken>();

            if (metadataAddress == null)
            {
                throw new ArgumentNullException(metadataAddress);
            }

            using (XmlReader metadataReader = XmlReader.Create(metadataAddress))
            {
                MetadataSerializer serializer = new MetadataSerializer()
                {
                    // Do not disable for production code
                    CertificateValidationMode = X509CertificateValidationMode.None
                };

                EntityDescriptor metadata = serializer.ReadMetadata(metadataReader) as EntityDescriptor;

                if (metadata != null)
                {
                    var stsd = metadata.RoleDescriptors.OfType<SecurityTokenServiceDescriptor>().First();

                    if (stsd != null)
                    {

                        var x509DataClauses = stsd.Keys.Where(key => key.KeyInfo != null && (key.Use == KeyType.Signing || key.Use == KeyType.Unspecified)).
                            Select(key => key.KeyInfo.OfType<X509RawDataKeyIdentifierClause>().First());
                        tokens.AddRange(x509DataClauses.Select(token => new X509SecurityToken(new X509Certificate2(token.GetX509RawData()))));
                    }
                    else
                    {
                        throw new InvalidOperationException("There is no RoleDescriptor of type SecurityTokenServiceType in the metadata");
                    }
                }
                else
                {
                    throw new Exception("Invalid Federation Metadata document");
                }
            }
            cache.TryAdd(metadataAddress, tokens);
            return tokens;
        }
    }
}