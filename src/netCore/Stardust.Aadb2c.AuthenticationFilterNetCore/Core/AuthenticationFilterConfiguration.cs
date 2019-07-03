using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public static class AuthenticationFilterConfiguration
    {
        public static IApplicationBuilder AddConfigurationManager(this IApplicationBuilder app, IConfigurationReader manager)
        {
            ConfigurationManagerHelper.SetManager(manager);
            return app;
        }

        public static IApplicationBuilder AddConfigurationManager(this IApplicationBuilder app, IDictionary<string, string> configSettings)
        {
            ConfigurationManagerHelper.SetManager(new CoreConfigurationManager(configSettings));
            return app;
        }

        public static IApplicationBuilder AddConfigurationManager(this IApplicationBuilder app, IConfigurationBuilder builder)
        {
            return app.AddConfigurationManager(new CoreConfigurationManager(builder));
        }

        public static AuthenticationBuilder AddB2CAuthentication(this AuthenticationBuilder builder, string name, string displayName)
        {
            builder.AddScheme<B2COptions, B2COAuthHandler>(name, displayName, o =>
            {
                o.CallbackPath = "/";

            });
            return builder;
        }

        public static AuthenticationBuilder AddB2CAuthentication(this AuthenticationBuilder builder, string name)
        {
            return builder.AddB2CAuthentication(name, name);
        }

        public static AuthenticationBuilder AddB2CAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddB2CAuthentication("OAuth2");
        }
    }
}