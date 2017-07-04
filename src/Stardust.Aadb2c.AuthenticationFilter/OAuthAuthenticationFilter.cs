using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public class OAuthAuthenticationFilter : IAuthenticationFilter
    {
        private static void AuthorizeRequest(HttpActionContext actionContext, HttpContext context)
        {
            try
            {
                var supportCode = HttpContext.Current?.Request.Headers.GetValues("x-supportCode")?.SingleOrDefault();
                if (string.IsNullOrWhiteSpace(supportCode))
                {
                    supportCode = Guid.NewGuid().ToString();
                    //if (HttpContext.Current != null) HttpContext.Current.Request.Headers.Add("x-supportCode", supportCode);
                }
                HttpContext.Current?.Items.Add("x-supportCode", supportCode);
            }
            catch (Exception)
            {
            }
            try
            {
                var auth = context?.Request.Headers["Authorization"];
                if (!string.IsNullOrWhiteSpace(auth) && auth.Contains("Bearer"))
                {
                    var credentials = AuthenticationHeaderValue.Parse(auth);
                    var token = credentials.Parameter;
                    ValidateToken(token);
                }

                else
                    Logging.DebugMessage(
                        $"Anonnomous service request {actionContext.ActionDescriptor.ActionName} from {context.Request.UserHostAddress} ({context.Request.UserAgent})");
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }

        public static void ValidateToken(string accessToken)
        {


            var jwt = new JwtSecurityToken(accessToken);
            var handler = new JwtSecurityTokenHandler
            {
                Configuration = new SecurityTokenHandlerConfiguration { CertificateValidationMode = X509CertificateValidationMode.None }
            };
            var audience = Resource;
            try
            {
                SecurityToken validatedToken;
                var settings =
                    Settings;
                var securityToken = handler.ValidateToken(accessToken, new System.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidAudience = audience,
                    ValidIssuer = ConfigurationManager.AppSettings["issuerHostName"],
                    IssuerSigningTokens = settings.SecurityTokens

                }, out validatedToken);

                ((ClaimsIdentity)securityToken.Identity).AddClaim(new Claim("token", accessToken));

                var principal = new ClaimsPrincipal(securityToken);

                var identity = principal.Identity as ClaimsIdentity;
                //Logging.DebugMessage($"User: {Resolver.Activate<IIdentityLookup>().GetUserName(identity)} validated");
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
            }
            catch (Exception ex)
            {
                ex.Log("Token validation failed");
                throw new UnauthorizedAccessException("Unable to validate token", ex);
            }
        }

        private static OpenIdConnectCachingSecurityTokenProvider Settings { get; } = new OpenIdConnectCachingSecurityTokenProvider(
            MetadataEndpoint);

        private static string MetadataEndpoint
        {
            get
            { return $"https://login.microsoftonline.com/{ConfigurationManagerHelper.GetValueOnKey("aadTennant", "dnvglb2ctest.onmicrosoft.com")}/v2.0/.well-known/openid-configuration?p={ConfigurationManagerHelper.GetValueOnKey("aadPolicy", "B2C_1A_SignInWithADFSIdp")}"; }
        }

        private static string Resource
        {
            get
            {
                if (audience != null) return audience;
                audience = ConfigurationManager.AppSettings["audience"];
                // if (!audience.EndsWith("/")) audience = $"{audience}/";
                return audience;
            }
        }
        public static ConcurrentDictionary<string, List<SecurityToken>> cache = new ConcurrentDictionary<string, List<SecurityToken>>();
        private static string audience;




        /// <summary>Gets or sets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.</summary>
        /// <returns>true if more than one instance is allowed to be specified; otherwise, false. The default is false.</returns>
        public bool AllowMultiple => false;

        /// <summary>Authenticates the request.</summary>
        /// <returns>A Task that will perform authentication.</returns>
        /// <param name="context">The authentication context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            try
            {
                //Logging.DebugMessage("authorize filter");
                AuthorizeRequest(context.ActionContext, HttpContext.Current);
            }
            catch (Exception ex)
            {
                context.ErrorResult = new StatusCodeResult(HttpStatusCode.Unauthorized, context.Request);//new AuthenticationFailureResult("Unable to autorize request", context.Request);

            }
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            //if(context.ActionContext.Request.)
            //context.Request.CreateErrorResponse(HttpStatusCode.Forbidden)
            return Task.FromResult(context.Result);
        }
    }
}
