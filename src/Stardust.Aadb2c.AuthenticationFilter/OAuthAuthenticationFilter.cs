using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IdentityModel.Tokens;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Stardust.Particles;
using System.Diagnostics;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public class OAuthAuthenticationFilter : OAuthV1AuthenticationFilter
    {
        private static void AuthorizeRequest(HttpActionContext actionContext, HttpContext context)
        {
            AppendSupportCode();
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
                    Logging.DebugMessage($"Anonymous service request {actionContext.ActionDescriptor.ActionName} from {context.Request.UserHostAddress} ({context.Request.UserAgent})");
            }
            catch (Exception ex)
            {
                ex.Log();
                throw;
            }
        }

        private static void AppendSupportCode()
        {
            if (!B2CGlobalConfiguration.UseSupportCode) return;
            try
            {
                var supportCode = HttpContext.Current?.Request.Headers.GetValues("x-supportCode")?.SingleOrDefault();
                if (string.IsNullOrWhiteSpace(supportCode))
                {
                    supportCode = Guid.NewGuid().ToString();
                }
                HttpContext.Current?.Items.Add("x-supportCode", supportCode);
            }
            catch (Exception)
            {
            }
        }

        public static ClaimsPrincipal ValidateToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler
            {
                Configuration = new SecurityTokenHandlerConfiguration { /*CertificateValidationMode = X509CertificateValidationMode.None*/ }
            };

            try
            {
                SecurityToken validatedToken;
                //no need to read the token even one more time...
                //var decodedToken = handler.ReadToken(accessToken) as JwtSecurityToken;
                //var isUserToken = decodedToken != null && decodedToken.Claims.Any(claim => claim.Type == "upn");
                //Logging.DebugMessage($"Validating {(isUserToken ? "User" : "ServicePrincipal")}");
                var settings = Settings;// : Settings4App;

                var securityToken = handler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidAudience = Resource,
                    ValidIssuers = new[] { B2CGlobalConfiguration.ValidIssuer, B2CGlobalConfiguration.ValidIssuer + "/" },
                    //ValidIssuer = B2CGlobalConfiguration.ValidIssuer,
                    IssuerSigningTokens = settings.SecurityTokens

                }, out validatedToken);
                var principal = new ClaimsPrincipal(securityToken);
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;
                return principal;
            }
            catch (Exception ex)
            {
                ex.Log("Token validation failed");
                throw new UnauthorizedAccessException("Unable to validate token", ex);
            }
        }

        private readonly static OpenIdConnectCachingSecurityTokenProvider Settings = new OpenIdConnectCachingSecurityTokenProvider(MetadataEndpoint);

        private readonly static OpenIdConnectCachingSecurityTokenProvider Settings4App = new OpenIdConnectCachingSecurityTokenProvider($"https://login.microsoftonline.com/{B2CGlobalConfiguration.AadTenant}/v2.0/.well-known/openid-configuration");


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

        /// <summary>Gets or sets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.</summary>
        /// <returns>true if more than one instance is allowed to be specified; otherwise, false. The default is false.</returns>
        public bool AllowMultiple => false;

        /// <summary>Authenticates the request.</summary>
        /// <returns>A Task that will perform authentication.</returns>
        /// <param name="context">The authentication context.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var timer = Stopwatch.StartNew();
            if(!context.Request.Headers.Contains("Authorization"))
            {
                HttpContext.Current?.Response.AddHeader("x-auth-state", "missing_token");
                 timer.Stop();
                return Task.FromResult(0);
            }
            var auth = context?.Request?.Headers?.GetValues("Authorization")?.FirstOrDefault();
            if (auth == null)
            {
                HttpContext.Current?.Response.AddHeader("x-auth-state", "missing_token");
                timer.Stop();
                return Task.FromResult(0);
            }
            var credentials = AuthenticationHeaderValue.Parse(auth);
            var jwt = new JwtSecurityToken(credentials.Parameter);
            if(jwt.Claims.SingleOrDefault(c => c.Type == "userId")!=null)
            {
                Logging.DebugMessage("Validating user token");
                try
                {
                    AuthorizeRequest(context.ActionContext, HttpContext.Current);
                }
                catch (Exception ex)
                {
                    context.ErrorResult = new StatusCodeResult(HttpStatusCode.Unauthorized, context.Request);

                }
            }
            else
            {
                Logging.DebugMessage("Validating client token");
                return base.AuthenticateAsync(context, cancellationToken);
            }
            timer.Stop();
            Logging.DebugMessage($"Authorization took {timer.ElapsedMilliseconds}ms");
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(context.Result);
        }
    }
}
