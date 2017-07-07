using System;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.Application;

namespace Stardust.Aadb2c.Swagger
{
    public static class SwaggerOAuthExtensions
    {
        public static string AadSigninUrl { get; set; } = "https://login.microsoftonline.com";
        private static readonly string[] DefaultScopes = { "openid", "offline_access" };
        private static readonly string[] DefaultScopesNames = { "performs sign-in", "Give the app access to resources on behalf of the user for an extended time" };

        

        public static void EnableAzureAdB2cOAuth2(this SwaggerDocsConfig c)
        {
            c.EnableAzureAdB2cOAuth2(B2cGlobalSwaggerConfig.TenantId,B2cGlobalSwaggerConfig.UseV2Endpoint,B2cGlobalSwaggerConfig.ApplicationDescription,B2cGlobalSwaggerConfig.Scopes);
        }
        public static void EnableAzureAdB2cOAuth2(this SwaggerDocsConfig c, string tenantId, bool useV2Endpoint, string description, params ScopeDescription[] scopes)
        {
            ValidateScopes(scopes.Select(s => s.ScopeName));
            c.OAuth2("oauth2")
                .Description(description)
                .Flow("implicit")
                .AuthorizationUrl(AuthorizationUrl(tenantId, useV2Endpoint))
                //.TokenUrl(TokenUrl(tenantId, useV2Endpoint))
                .Scopes(s =>
                {
                    for (int i = 0; i < DefaultScopes.Length; i++)
                    {
                        s.Add(DefaultScopes[i], DefaultScopesNames[i]);
                    }
                    foreach (var scopeDescription in scopes)
                    {
                        s.Add(scopeDescription.ScopeName, scopeDescription.Description);
                    }
                });
            c.OperationFilter(() => new AssignOAuth2SecurityRequirements(scopes));
        }

        public static void EnableAzureAdB2cOAuth2(this SwaggerDocsConfig c, string tenantId, params ScopeDescription[] scopes)
        {
            c.EnableAzureAdB2cOAuth2(tenantId, true, "OAuth2 Implicit Grant", scopes);
        }

        public static void EnableAzureAdB2cOAuth2(this SwaggerDocsConfig c, string tenantId, bool useV2Endpoint, params ScopeDescription[] scopes)
        {
            c.EnableAzureAdB2cOAuth2(tenantId, useV2Endpoint, "OAuth2 Implicit Grant", scopes);
        }

        public static void EnableAzureAdB2cOAuth2(this SwaggerDocsConfig c, string tenantId, string description, params ScopeDescription[] scopes)
        {
            c.EnableAzureAdB2cOAuth2(tenantId, true, description, scopes);
        }

        private static string TokenUrl(string tenantId, bool useV2Endpoint)
        {
            var type = "token";
            return FormatAuthototyUrl(tenantId, useV2Endpoint, type);
        }

        private static string FormatAuthototyUrl(string tenantId, bool useV2Endpoint, string type)
        {
            return $"{AadSigninUrl}/{tenantId}/oauth2/{useV2Endpoint.EndpointVersion()}{type}";
        }

        private static string EndpointVersion(this bool useV2Endpoint)
        {
            return useV2Endpoint ? "v2.0/" : "";
        }

        private static string AuthorizationUrl(string tenantId, bool useV2Endpoint)
        {
            return FormatAuthototyUrl(tenantId, useV2Endpoint, "authorize");
        }

        private static void ValidateScopes(IEnumerable<string> scopes)
        {
            var validationErrors = new List<ArgumentException>();
            foreach (var scope in DefaultScopes)
            {
                if (scopes.Contains(scope))
                    validationErrors.Add(new ArgumentException($"Do not add {scope} these will be added automatically",
                        nameof(scopes)));
            }
            if (validationErrors.Count != 0)
                throw new AggregateException($"Scope validation errors, see {nameof(AggregateException.InnerExceptions)} for details", validationErrors);
        }
    }
}