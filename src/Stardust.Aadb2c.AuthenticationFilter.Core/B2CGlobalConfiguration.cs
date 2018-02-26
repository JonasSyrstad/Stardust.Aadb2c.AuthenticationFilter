using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter.Core
{
    public static class B2CGlobalConfiguration
    {
        public static string Audience
        {
            get => ConfigurationManagerHelper.GetValueOnKey("audience");
            set => ConfigurationManagerHelper.SetValueOnKey("audience", value, true);
        }

        public static string AadTenant
        {
            get => ConfigurationManagerHelper.GetValueOnKey("aadTenant");
            set => ConfigurationManagerHelper.SetValueOnKey("aadTenant", value, true);
        }

        public static string AadPolicy
        {
            get => ConfigurationManagerHelper.GetValueOnKey("aadPolicy");
            set => ConfigurationManagerHelper.SetValueOnKey("aadPolicy", value, true);
        }

        public static string ValidIssuer
        {
            get => ConfigurationManagerHelper.GetValueOnKey("issuerHostName");
            set => ConfigurationManagerHelper.SetValueOnKey("issuerHostName", value, true);
        }

        public static string ValidIssuerV1
        {
            get => ConfigurationManagerHelper.GetValueOnKey("issuerHostName_v1");
            set => ConfigurationManagerHelper.SetValueOnKey("issuerHostName_v1", value, true);
        }

        public static bool UseSupportCode
        {
            get => ConfigurationManagerHelper.GetValueOnKey("stardust.useSupportCode", true);
            set => ConfigurationManagerHelper.SetValueOnKey("stardust.useSupportCode", value.ToString(), true);
        }

        public static string AudienceV1
        {
            get => ConfigurationManagerHelper.GetValueOnKey("audience_v1");
            set => ConfigurationManagerHelper.SetValueOnKey("audience_v1", value, true);
        }
    }

    //public interface IServiceLocator
    //{
    //    T GetService<T>();

    //    IEnumerable<T> GetServices<T>();
    //}



    //public static class ServiceLocatorFactory
    //{
    //    private static IServiceLocator _serviceLocator;

    //    public static void AddServiceLocator(IServiceLocator locator)
    //    {
    //        _serviceLocator = locator;
    //    }

    //    internal static T GetService<T>()
    //    {
    //        if (_serviceLocator == null) return default(T);
    //        return _serviceLocator.GetService<T>();
    //    }

    //    internal static IEnumerable<T> GetServices<T>()
    //    {
    //        return _serviceLocator?.GetServices<T>();
    //    }
    //}
}
