using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Stardust.Particles;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
namespace Stardust.Aadb2c.AuthenticationFilter.Core
{
    public class OpenIdConnectCachingSecurityTokenProviderV2 : IOpenIdConnectCachingSecurityTokenProvider
    {
        private readonly string[] _metadataEndpoints;
        private List<OpenIdConnectCachingSecurityTokenProvider> _caches;

        public OpenIdConnectCachingSecurityTokenProviderV2(string[] metadataEndpoints)
        {
            _metadataEndpoints = metadataEndpoints;
            _caches=new List<OpenIdConnectCachingSecurityTokenProvider>();
            foreach (var metadataEndpoint in metadataEndpoints)
            {
                _caches.Add(new OpenIdConnectCachingSecurityTokenProvider(metadataEndpoint));
            }
        }

        public ICollection<SecurityKey> SecurityTokens
        {
            get
            {
                var list=new List<SecurityKey>();
                foreach (var openIdConnectCachingSecurityTokenProvider in _caches)
                {
                    list.AddRange(openIdConnectCachingSecurityTokenProvider.SecurityTokens);
                }
                return list;
            }
        }

        public string[] Issuers
        {
            get { return _caches.Select(p => p.Issuer).ToArray(); }
        }
    }
    public interface IOpenIdConnectCachingSecurityTokenProvider
    {
        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <value>
        /// All known security tokens.
        /// </value>
        ICollection<SecurityKey> SecurityTokens { get; }
    }

    public class OpenIdConnectCachingSecurityTokenProvider : IOpenIdConnectCachingSecurityTokenProvider
    {
        public ConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private string _issuer;
        private ICollection<SecurityKey> _tokens;
        private readonly string _metadataEndpoint;

        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();

        public OpenIdConnectCachingSecurityTokenProvider(string metadataEndpoint)
        {
            _metadataEndpoint = metadataEndpoint;
            _configManager =
                new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint,
                    new OpenIdConnectConfigurationRetriever())
                {
                    AutomaticRefreshInterval = TimeSpan.FromMinutes(ConfigurationManagerHelper.GetValueOnKey("certificateRefresInterval", 30)),
                    RefreshInterval = TimeSpan.FromMinutes(ConfigurationManagerHelper.GetValueOnKey("certificateRefresInterval", 30) * 2)
                };
            
            RetrieveMetadata();
        }

        /// <summary>
        /// Gets the issuer the credentials are for.
        /// </summary>
        /// <value>
        /// The issuer the credentials are for.
        /// </value>
        public string Issuer
        {
            get
            {
                //RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    return _issuer;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Gets all known security tokens.
        /// </summary>
        /// <value>
        /// All known security tokens.
        /// </value>
        public ICollection<SecurityKey> SecurityTokens
        {
            get
            {
                //RetrieveMetadata();
                _synclock.EnterReadLock();
                try
                {
                    return _tokens;
                }
                finally
                {
                    _synclock.ExitReadLock();
                }
            }
        }

        private void RetrieveMetadata()
        {

            _synclock.EnterWriteLock();
            try
            {
                var config = Task.Run(_configManager.GetConfigurationAsync).Result;
                _issuer = config.Issuer;
                _tokens = config.SigningKeys;
                //if (ConfigurationManagerHelper.GetValueOnKey("stardust.doLogging", false))
                //    Logging.DebugMessage(JsonConvert.SerializeObject(config));
            }
            catch (Exception ex)
            {
                //ex.Log();
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
        }
    }
}