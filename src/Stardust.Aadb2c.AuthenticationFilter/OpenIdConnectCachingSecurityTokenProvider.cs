using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public class OpenIdConnectCachingSecurityTokenProvider
    {
        public ConfigurationManager<OpenIdConnectConfiguration> _configManager;
        private string _issuer;
        private IEnumerable<SecurityToken> _tokens;
        private readonly string _metadataEndpoint;

        private readonly ReaderWriterLockSlim _synclock = new ReaderWriterLockSlim();

        public OpenIdConnectCachingSecurityTokenProvider(string metadataEndpoint)
        {
            _metadataEndpoint = metadataEndpoint;
            _configManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataEndpoint);

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
        public IEnumerable<SecurityToken> SecurityTokens
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
                _tokens = config.SigningTokens;
                if (ConfigurationManagerHelper.GetValueOnKey("stardust.doLogging", false))
                    Logging.DebugMessage(JsonConvert.SerializeObject(config));
            }
            catch (Exception ex)
            {
                ex.Log();
            }
            finally
            {
                _synclock.ExitWriteLock();
            }
        }
    }
}