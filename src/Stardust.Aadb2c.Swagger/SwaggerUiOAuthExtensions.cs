using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Swashbuckle.Application;

namespace Stardust.Aadb2c.Swagger
{
    public static class SwaggerUiOAuthExtensions
    {
        public static void EnableAzureAdB2cOAuth2(this SwaggerUiConfig c, string clientId, string policyName, string clientSecret = null, string realm = null, string appName = "Swagger UI", bool enableDiscoveryUrlSelector = true)
        {
            c.EnableOAuth2Support(
                clientId,
                clientSecret,
                realm,
                appName,
                additionalQueryStringParams: new Dictionary<string, string>
                {
                    {"p", policyName},
                    {"response_mode", "fragment"}
                }
            );
            if (enableDiscoveryUrlSelector)
                c.EnableDiscoveryUrlSelector();
        }

        public static void EnableAzureAdB2cOAuth2(this SwaggerUiConfig c)
        {
            c.EnableAzureAdB2cOAuth2(B2cGlobalSwaggerConfig.ClientId,B2cGlobalSwaggerConfig.Policy,B2cGlobalSwaggerConfig.AppName,B2cGlobalSwaggerConfig.ClientSecret);
        }
    }
}
