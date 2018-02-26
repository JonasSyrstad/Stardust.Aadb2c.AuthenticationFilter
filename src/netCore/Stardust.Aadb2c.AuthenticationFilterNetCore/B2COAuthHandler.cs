using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Aadb2c.AuthenticationFilter;
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

        public static IApplicationBuilder AddConfigurationManager(this IApplicationBuilder app, IDictionary<string,string> configSettings)
        {
            ConfigurationManagerHelper.SetManager(new CoreConfigurationManager(configSettings));
            return app;
        }
        
        public static IApplicationBuilder AddConfigurationManager(this IApplicationBuilder app,IConfigurationBuilder builder)
        {
            return app.AddConfigurationManager(new CoreConfigurationManager(builder));
        }

        public static AuthenticationBuilder AddB2CAuthentication(this AuthenticationBuilder builder,string name,string displayName)
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

    public class CoreConfigurationManager : IConfigurationReader
    {
        private NameValueCollection _collection;

        public CoreConfigurationManager(IConfigurationBuilder builder)
        {
            _collection = new NameValueCollection();
            foreach (var builderProperty in builder.Properties)
            {
                _collection.Add(builderProperty.Key, builderProperty.Value.ToString());

            }
        }

        public CoreConfigurationManager(IDictionary<string, string> configSettings)
        {
            foreach (var builderProperty in configSettings)
            {
                _collection.Add(builderProperty.Key, builderProperty.Value);
            }
        }

        public NameValueCollection AppSettings => _collection;
    }

    public class B2COAuthHandler : RemoteAuthenticationHandler<B2COptions>
    {
        public B2COAuthHandler(IOptionsMonitor<B2COptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            try
            {
                Logger.LogDebug($"Looking for bearer token! {nameof(HandleRemoteAuthenticateAsync)}");
                var v = Request.Headers["Authorization"].FirstOrDefault();
                if (v != null && v.Split(' ')[0].Equals("bearer", StringComparison.InvariantCultureIgnoreCase))
                {
                    var user = TokenValidator.Validate(v.Split(' ')[1]);
                    Context.User = user;

                    return Task.FromResult(HandleRequestResult.Success(new AuthenticationTicket(user, Scheme.Name)));
                }
                Logger.LogDebug("No bearer token provided");
                return Task.FromResult(HandleRequestResult.SkipHandler());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return Task.FromResult(HandleRequestResult.Fail(ex));
            }
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                Logger.LogDebug($"Looking for bearer token! {nameof(HandleAuthenticateAsync)}");
                var v = Request.Headers["Authorization"].FirstOrDefault();
                if (v != null && v.Split(' ')[0].Equals("bearer", StringComparison.InvariantCultureIgnoreCase))
                {
                    var user = TokenValidator.Validate(v.Split(' ')[1]);
                    Context.User = user;

                    return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
                }
                Logger.LogDebug("No bearer token provided");
                return Task.FromResult(AuthenticateResult.NoResult());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return Task.FromResult(AuthenticateResult.Fail(ex));
            }
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Logger.LogDebug("Test");

            return base.HandleChallengeAsync(properties);
        }
        //protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        //{

        //}
    }
}
