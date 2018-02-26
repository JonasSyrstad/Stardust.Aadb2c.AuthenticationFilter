using System.Collections.Specialized;
using System.Configuration;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    internal class WebConfigManager : IConfigurationReader
    {
        public NameValueCollection AppSettings => ConfigurationManager.AppSettings;
    }
}