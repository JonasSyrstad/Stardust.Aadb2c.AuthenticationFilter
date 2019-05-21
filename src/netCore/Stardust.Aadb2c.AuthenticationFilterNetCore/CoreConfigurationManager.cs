using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    //public class CoreConfigurationManager : IConfigurationReader
    //{
    //    private NameValueCollection _collection;

    //    public CoreConfigurationManager(IConfigurationBuilder builder)
    //    {
    //        _collection = new NameValueCollection();
    //        foreach (var builderProperty in builder.Properties)
    //        {
    //            _collection.Add(builderProperty.Key, builderProperty.Value.ToString());

    //        }
    //    }

    //    public CoreConfigurationManager(IDictionary<string, string> configSettings)
    //    {
    //        foreach (var builderProperty in configSettings)
    //        {
    //            _collection.Add(builderProperty.Key, builderProperty.Value);
    //        }
    //    }

    //    public NameValueCollection AppSettings => _collection;
    //}
}