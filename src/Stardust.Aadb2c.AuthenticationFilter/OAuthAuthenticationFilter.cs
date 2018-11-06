using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public class OAuthAuthenticationFilter : IAuthenticationFilter
    {
        static OAuthAuthenticationFilter()
        {
            ConfigurationManagerHelper.SetManager(new WebConfigManager());
        }
        public bool AllowMultiple => false;
        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            AppendSupportCode();
            try
            {
                var auth = context?.Request.Headers.Authorization;
                ClaimsPrincipal user;
                if (auth != null && string.Equals(auth.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase) && !auth.Parameter.IsNullOrWhiteSpace())
                {

                    var credentials = auth;
                    var token = credentials.Parameter;
                    user = TokenValidator.Validate(auth.Parameter);
                    HttpContext.Current.User = user;
                    context.Principal = HttpContext.Current.User;

                }
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                context.ErrorResult = new StatusCodeResult(HttpStatusCode.Forbidden, context.ActionContext.ControllerContext.Controller as ApiController);
                return Task.CompletedTask;
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

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(context.Result);
        }
    }
}
