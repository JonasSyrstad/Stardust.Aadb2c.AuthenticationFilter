using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    /// <summary>
    /// Supports the Azure AD V1 endpoint
    /// </summary>
    public class OAuthV1AuthenticationFilter : IAuthenticationFilter
    {
        public virtual Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
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
            return Task.FromResult(0);
        }

        private static void AuthorizeRequest(HttpActionContext actionContext, HttpContext context)
        {
            var auth = context?.Request.Headers["Authorization"];

            if (auth?.StartsWith("Bearer ") ?? false)
            {
                var credentials = AuthenticationHeaderValue.Parse(auth);
                var token = credentials.Parameter;
                AdalTokenValidationHelper.ValidateToken(token);
            }
            else
            {
                Logging.DebugMessage($"Unknown Token: {auth}", additionalDebugInformation: "Authentication Failure");
                //throw new UnauthorizedAccessException("Unable to validate token");
            }
        }

        public bool AllowMultiple => false;
    }
}