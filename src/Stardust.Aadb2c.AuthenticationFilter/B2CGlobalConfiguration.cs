using Stardust.Particles;

namespace Stardust.Aadb2c.AuthenticationFilter
{
    public static class B2CGlobalConfiguration
    {
        public static string Audience
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("audience");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("audience", value, true);
            }
        }

        public static string AadTenant
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("aadTenant");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadTenant", value, true);
            }
        }

        public static string AadPolicy
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("aadPolicy");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("aadPolicy", value, true);
            }
        }

        public static string ValidIssuer
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("issuerHostName");
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("issuerHostName", value, true);
            }
        }

        public static bool UseSupportCode
        {
            get
            {
                return ConfigurationManagerHelper.GetValueOnKey("stardust.useSupportCode", true);
            }
            set
            {
                ConfigurationManagerHelper.SetValueOnKey("stardust.useSupportCode", value.ToString(), true);
            }
        }
    }
}