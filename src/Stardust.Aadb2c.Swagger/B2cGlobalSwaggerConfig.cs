using System;
using System.Collections.Generic;
using System.Linq;
using Stardust.Aadb2c.AuthenticationFilter.Core;
using Stardust.Particles;

namespace Stardust.Aadb2c.Swagger
{
    public static class B2cGlobalSwaggerConfig
    {
        private static readonly object Triowing = new object();
        private static ScopeDescription[] _scopes;
        public static ScopeDescription[] Scopes
        {
            get
            {
                if (_scopes != null) return _scopes;
                lock (Triowing)
                {
                    if (_scopes == null)
                    {
                        var internalList = new List<ScopeDescription>();
                        var scopes = ConfigurationManagerHelper.GetValueOnKey("aadScopes").Split('|');
                        foreach (var scope in scopes)
                        {
                            var parts = scope.Split(';');
                            if (parts.Length == 0) continue;
                            // ReSharper disable once NotResolvedInText
                            if (parts.Length > 2)
                                throw new ArgumentException(
                                    $"scope {parts[0]} contains ; in either the scope name part or in the description ({scope})",
                                    "aadScopes");
                            internalList.Add(
                                new ScopeDescription { ScopeName = parts[0], Description = parts.Last() });
                        }
                        _scopes = internalList.ToArray();
                        return _scopes;
                    }
                    else
                    {
                        return _scopes;
                    }
                }

            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadScopes", string.Join("|", value.Select(v => $"{v.ScopeName};{v.Description}")));
            }
        }

        public static string ApplicationDescription
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("aadFlowDescription", "OAuth2 Implicit Grant");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadFlowDescription", value, true);
            }
        }
        public static bool UseV2Endpoint
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("aadUseV2Endpoint", true);
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadUseV2Endpoint", value.ToString(), true);
            }
        }
        public static string TenantId
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("aadTenantId");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadTenantId", value, true);
            }
        }
        public static string ClientId
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("swaggerClientId");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("swaggerClientId", value, true);
            }
        }

        public static string ClientSecret
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("swaggerClientSecret");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("swaggerClientSecret", value, true);
            }
        }
        public static string AppName
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("swaggerAppName");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("swaggerAppName", value, true);
            }
        }
        public static string Policy
        {
            get { return B2CGlobalConfiguration.AadPolicy; }
            set
            {
                B2CGlobalConfiguration.AadPolicy = value;
            }
        }
    }
}